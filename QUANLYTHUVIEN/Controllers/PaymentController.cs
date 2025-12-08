using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace QUANLYTHUVIEN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly QlthuvienContext _context;
        private const string PendingRentalKey = "PendingRental";
        
        // MoMo API Configuration (Sandbox - thay bằng thông tin thật khi triển khai)
        private readonly string MOMO_PARTNER_CODE = "MOMO";
        private readonly string MOMO_ACCESS_KEY = "F8BBA842ECF85";
        private readonly string MOMO_SECRET_KEY = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
        private readonly string MOMO_API_ENDPOINT = "https://test-payment.momo.vn/v2/gateway/api/create";

        public PaymentController(QlthuvienContext context)
        {
            _context = context;
        }

        // GET: Payment/Momo
        public IActionResult Momo(int rentalId)
        {
            // Kiểm tra đăng nhập
            var userIdStr = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để tiếp tục thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin rental từ session hoặc database
            var rental = GetPendingRental(rentalId);
            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.RentalId = rentalId;
            ViewBag.Amount = rental.TotalAmount ?? 0;
            ViewBag.OrderInfo = $"Thanh toán đơn thuê sách #{rentalId}";
            
            return View();
        }

        // POST: Payment/ProcessMomo
        [HttpPost]
        public async Task<IActionResult> ProcessMomo(int rentalId)
        {
            try
            {
                // Kiểm tra đăng nhập
                var userIdStr = HttpContext.Session?.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập" });
                }

                // Lấy thông tin rental
                var rental = GetPendingRental(rentalId);
                if (rental == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                var amount = (long)(rental.TotalAmount ?? 0);
                var orderInfo = $"Thanh toán đơn thuê sách #{rentalId}";
                var orderId = $"RENTAL_{rentalId}_{DateTime.Now:yyyyMMddHHmmss}";
                var requestId = Guid.NewGuid().ToString();
                var extraData = "";

                // Tạo signature cho MoMo
                var rawHash = $"accessKey={MOMO_ACCESS_KEY}&amount={amount}&extraData={extraData}&ipnUrl={Request.Scheme}://{Request.Host}/Payment/MomoCallback&orderId={orderId}&orderInfo={orderInfo}&partnerCode={MOMO_PARTNER_CODE}&redirectUrl={Request.Scheme}://{Request.Host}/Payment/MomoResult&requestId={requestId}&requestType=captureWallet";
                var signature = ComputeHmacSha256(rawHash, MOMO_SECRET_KEY);

                // Tạo payment request
                var paymentRequest = new
                {
                    partnerCode = MOMO_PARTNER_CODE,
                    partnerName = "QUANLYTHUVIEN",
                    storeId = "QUANLYTHUVIEN",
                    requestId = requestId,
                    amount = amount,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = $"{Request.Scheme}://{Request.Host}/Payment/MomoResult",
                    ipnUrl = $"{Request.Scheme}://{Request.Host}/Payment/MomoCallback",
                    lang = "vi",
                    extraData = extraData,
                    requestType = "captureWallet",
                    signature = signature
                };

                // Lưu thông tin payment vào session
                SavePaymentInfo(rentalId, orderId, requestId);

                // Trong môi trường thật, gọi API MoMo
                // Ở đây tôi sẽ mô phỏng bằng cách redirect đến trang thanh toán MoMo
                // Bạn có thể thay thế bằng API call thật:
                /*
                using (var client = new HttpClient())
                {
                    var json = JsonSerializer.Serialize(paymentRequest);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(MOMO_API_ENDPOINT, content);
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var momoResponse = JsonSerializer.Deserialize<MomoPaymentResponse>(responseContent);
                    
                    if (momoResponse.resultCode == 0)
                    {
                        return Redirect(momoResponse.payUrl);
                    }
                }
                */

                // Mô phỏng: Tạo URL thanh toán MoMo
                var payUrl = Url.Action("MomoPayment", "Payment", new { 
                    rentalId = rentalId, 
                    orderId = orderId,
                    amount = amount 
                }, Request.Scheme);

                return Json(new { 
                    success = true, 
                    payUrl = payUrl,
                    orderId = orderId 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Payment/MomoPayment (Trang thanh toán MoMo mô phỏng)
        public IActionResult MomoPayment(int rentalId, string orderId, long amount)
        {
            var rental = GetPendingRental(rentalId);
            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.RentalId = rentalId;
            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.CustomerName = rental.Customer?.FullName ?? "Khách hàng";

            return View();
        }

        // POST: Payment/MomoPayment (Xử lý thanh toán thành công)
        [HttpPost]
        public async Task<IActionResult> MomoPayment(int rentalId, string orderId, string phoneNumber, string otp)
        {
            try
            {
                // Trong thực tế, đây sẽ là callback từ MoMo
                // Ở đây mô phỏng: nếu có phone và otp thì coi như thanh toán thành công
                
                var rental = GetPendingRental(rentalId);
                if (rental == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Mô phỏng xác thực OTP (trong thực tế sẽ gọi API MoMo)
                if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(otp))
                {
                    return Json(new { success = false, message = "Vui lòng nhập số điện thoại và mã OTP" });
                }

                // Giả lập: OTP = "000000" hoặc bất kỳ số nào đều được (để test)
                // Trong thực tế sẽ verify với MoMo API

                // Cập nhật rental trong database
                var dbRental = await _context.Rentals
                    .Include(r => r.RentalDetails)
                    .FirstOrDefaultAsync(r => r.RentalId == rentalId);

                if (dbRental == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng trong hệ thống" });
                }

                // Cập nhật trạng thái thanh toán
                dbRental.Status = "Đã thanh toán";
                
                // Giảm số lượng sách có sẵn
                foreach (var detail in dbRental.RentalDetails)
                {
                    var book = await _context.Books.FindAsync(detail.BookId);
                    if (book != null && book.Quantity >= detail.Quantity)
                    {
                        book.Quantity -= detail.Quantity;
                    }
                    else if (book != null)
                    {
                        // Không đủ sách
                        return Json(new { 
                            success = false, 
                            message = $"Sách '{book.Title}' không còn đủ số lượng" 
                        });
                    }
                }
                
                await _context.SaveChangesAsync();

                // Xóa thông tin pending
                ClearPendingRental(rentalId);

                // Lưu thông tin payment thành công
                SavePaymentResult(rentalId, orderId, "success", "Thanh toán thành công qua MoMo");

                return Json(new { 
                    success = true, 
                    message = "Thanh toán thành công!",
                    redirectUrl = Url.Action("MomoResult", "Payment", new { rentalId = rentalId, status = "success" })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Payment/MomoResult (Trang kết quả thanh toán)
        public async Task<IActionResult> MomoResult(int rentalId, string status)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .FirstOrDefaultAsync(r => r.RentalId == rentalId);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Status = status;
            ViewBag.PaymentResult = GetPaymentResult(rentalId);

            return View(rental);
        }

        // POST: Payment/MomoCallback (Callback từ MoMo - IPN)
        [HttpPost]
        public async Task<IActionResult> MomoCallback([FromBody] object momoData)
        {
            try
            {
                // Xử lý callback từ MoMo
                // Trong thực tế sẽ verify signature và cập nhật trạng thái thanh toán
                
                return Ok(new { resultCode = 0, message = "Success" });
            }
            catch
            {
                return BadRequest();
            }
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
    }
}

