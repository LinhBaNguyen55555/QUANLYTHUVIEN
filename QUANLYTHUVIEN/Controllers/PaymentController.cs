using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using QUANLYTHUVIEN.Services;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace QUANLYTHUVIEN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly IVnPayService _vnPayService;
        private const string PendingRentalKey = "PendingRental";

        public PaymentController(QlthuvienContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // Helper methods
        private Rental? GetPendingRental(int rentalId)
        {
            // Lấy từ session hoặc database
            var rental = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .FirstOrDefault(r => r.RentalId == rentalId);
            
            return rental;
        }

        private void SavePaymentInfo(int rentalId, string orderId, string requestId)
        {
            if (HttpContext.Session != null)
            {
                var paymentInfo = new { rentalId, orderId, requestId, timestamp = DateTime.Now };
                var json = JsonSerializer.Serialize(paymentInfo);
                HttpContext.Session.SetString($"Payment_{rentalId}", json);
            }
        }

        private void SavePaymentResult(int rentalId, string orderId, string status, string message)
        {
            if (HttpContext.Session != null)
            {
                var result = new { rentalId, orderId, status, message, timestamp = DateTime.Now };
                var json = JsonSerializer.Serialize(result);
                HttpContext.Session.SetString($"PaymentResult_{rentalId}", json);
            }
        }

        private object? GetPaymentResult(int rentalId)
        {
            if (HttpContext.Session != null)
            {
                var json = HttpContext.Session.GetString($"PaymentResult_{rentalId}");
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonSerializer.Deserialize<object>(json);
                }
            }
            return null;
        }

        private void ClearPendingRental(int rentalId)
        {
            if (HttpContext.Session != null)
            {
                HttpContext.Session.Remove($"Payment_{rentalId}");
            }
        }


        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                var hashMessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
            }
        }

        // VnPay Payment Methods
        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);

                if (response.Success && response.VnPayResponseCode == "00")
                {
                    int? rentalId = null;
                    
                    // Lấy OrderId từ response (vnp_TxnRef)
                    var orderId = response.OrderId;
                    
                    // Tìm rentalId từ nhiều nguồn
                    if (HttpContext.Session != null)
                    {
                        // Thử 1: Tìm từ session mapping với OrderId
                        if (!string.IsNullOrEmpty(orderId))
                        {
                            var rentalIdFromOrder = HttpContext.Session.GetString($"VnPayOrder_{orderId}");
                            if (!string.IsNullOrEmpty(rentalIdFromOrder) && int.TryParse(rentalIdFromOrder, out int id1))
                            {
                                rentalId = id1;
                            }
                        }
                        
                        // Thử 2: Tìm từ LastVnPayRentalId
                        if (!rentalId.HasValue)
                        {
                            var rentalIdStr = HttpContext.Session.GetString("LastVnPayRentalId");
                            if (!string.IsNullOrEmpty(rentalIdStr) && int.TryParse(rentalIdStr, out int id2))
                            {
                                rentalId = id2;
                            }
                        }
                    }
                    
                    // Thử 3: Parse từ OrderId nếu có format RENTAL_{rentalId}_{timestamp}
                    if (!rentalId.HasValue && !string.IsNullOrEmpty(orderId) && orderId.StartsWith("RENTAL_"))
                    {
                        var parts = orderId.Split('_');
                        if (parts.Length >= 2 && int.TryParse(parts[1], out int id3))
                        {
                            rentalId = id3;
                        }
                    }
                    
                    // Thử 4: Tìm từ OrderDescription trong response
                    if (!rentalId.HasValue && !string.IsNullOrEmpty(response.OrderDescription))
                    {
                        // OrderDescription có format: "Name Thanh toán đơn thuê sách #18 Amount"
                        var match = System.Text.RegularExpressions.Regex.Match(response.OrderDescription, @"#(\d+)");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int id4))
                        {
                            rentalId = id4;
                        }
                    }

                    if (rentalId.HasValue)
                    {
                        // Cập nhật rental trong database
                        var dbRental = await _context.Rentals
                            .Include(r => r.RentalDetails)
                            .FirstOrDefaultAsync(r => r.RentalId == rentalId.Value);

                        if (dbRental != null)
                        {
                            // Chỉ cập nhật nếu chưa thanh toán
                            if (dbRental.Status == "Chờ thanh toán")
                            {
                                // Cập nhật trạng thái thành "Đang thuê" sau khi thanh toán thành công
                                dbRental.Status = "Đang thuê";

                                // Giảm số lượng sách có sẵn
                                foreach (var detail in dbRental.RentalDetails)
                                {
                                    var book = await _context.Books.FindAsync(detail.BookId);
                                    if (book != null && book.Quantity >= (detail.Quantity ?? 0))
                                    {
                                        book.Quantity -= (detail.Quantity ?? 0);
                                    }
                                }

                                await _context.SaveChangesAsync();
                            }

                            // Xóa session
                            if (HttpContext.Session != null)
                            {
                                HttpContext.Session.Remove($"VnPayRental_{rentalId.Value}");
                                HttpContext.Session.Remove("LastVnPayRentalId");
                                if (!string.IsNullOrEmpty(orderId))
                                {
                                    HttpContext.Session.Remove($"VnPayOrder_{orderId}");
                                }
                            }

                            // Redirect đến trang thành công
                            return RedirectToAction("VnPayResult", "Payment", new { rentalId = rentalId.Value, status = "success" });
                        }
                    }

                    // Nếu không tìm thấy rentalId, vẫn hiển thị thông báo thành công
                    TempData["Success"] = "Thanh toán thành công qua VnPay!";
                    return RedirectToAction("VnPayResult", "Payment", new { status = "success" });
                }
                else
                {
                    // Thanh toán thất bại
                    TempData["Error"] = $"Thanh toán thất bại. Mã lỗi: {response.VnPayResponseCode}";
                    return RedirectToAction("VnPayResult", "Payment", new { status = "failed" });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra khi xử lý thanh toán: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"VnPay Callback Error: {ex.Message}\n{ex.StackTrace}");
                return RedirectToAction("VnPayResult", "Payment", new { status = "error" });
            }
        }

        // GET: Payment/VnPayResult (Trang kết quả thanh toán VnPay)
        public async Task<IActionResult> VnPayResult(int? rentalId, string status)
        {
            if (rentalId.HasValue)
            {
                var rental = await _context.Rentals
                    .Include(r => r.Customer)
                    .Include(r => r.RentalDetails)
                        .ThenInclude(rd => rd.Book)
                    .FirstOrDefaultAsync(r => r.RentalId == rentalId.Value);

                if (rental != null)
                {
                    // Tự động cập nhật status nếu là "Đã thanh toán" (đơn hàng cũ) thành "Đang thuê"
                    if (rental.Status == "Đã thanh toán" && !rental.ReturnDate.HasValue)
                    {
                        rental.Status = "Đang thuê";
                        await _context.SaveChangesAsync();
                    }
                    
                    ViewBag.Status = status;
                    ViewBag.Rental = rental;
                    return View(rental);
                }
            }

            ViewBag.Status = status;
            return View();
        }
    }
}

