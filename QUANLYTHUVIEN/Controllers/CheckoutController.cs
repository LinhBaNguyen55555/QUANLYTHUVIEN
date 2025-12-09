using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using QUANLYTHUVIEN.Services;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QUANLYTHUVIEN.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly IVnPayService _vnPayService;
        private const string CartSessionKey = "Cart";

        public CheckoutController(QlthuvienContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // Lấy danh sách giỏ hàng từ session
        private List<CartItem> GetCart()
        {
            if (HttpContext.Session == null)
            {
                return new List<CartItem>();
            }
            
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson, options) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        // Xóa giỏ hàng sau khi thanh toán
        private void ClearCart()
        {
            if (HttpContext.Session != null)
            {
                HttpContext.Session.Remove(CartSessionKey);
            }
        }

        // GET: Checkout
        public IActionResult Index()
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để tiếp tục thanh toán.";
                var returnUrl = Url.Action("Index", "Checkout");
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            var cart = GetCart();
            
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sách vào giỏ hàng trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var totalAmount = cart.Sum(c => c.TotalPrice);
            ViewBag.CartItems = cart;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.TotalQuantity = cart.Sum(c => c.Quantity);

            // Lấy thông tin user
            if (int.TryParse(userId, out int userIdInt))
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userIdInt);
                if (user != null)
                {
                    ViewBag.UserFullName = user.FullName;
                    ViewBag.UserEmail = user.Email;
                    ViewBag.UserPhone = user.Phone;
                }
            }

            return View();
        }

        // POST: Checkout/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(string fullName, string email, string phone, string address, 
            int rentalDays, string paymentMethod, string notes)
        {
            try
            {
                // Debug: Log payment method
                System.Diagnostics.Debug.WriteLine($"Payment Method received: {paymentMethod}");
                
                // Kiểm tra đăng nhập
                var userIdStr = HttpContext.Session?.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr))
                {
                    TempData["Warning"] = "Vui lòng đăng nhập để tiếp tục thanh toán.";
                    var returnUrl = Url.Action("Index", "Checkout");
                    return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
                }

                var cart = GetCart();
                
                if (cart == null || !cart.Any())
                {
                    TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Cart");
                }

                // Validation
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    ModelState.AddModelError("fullName", "Họ tên là bắt buộc");
                }
                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError("email", "Email là bắt buộc");
                }
                if (string.IsNullOrWhiteSpace(phone))
                {
                    ModelState.AddModelError("phone", "Số điện thoại là bắt buộc");
                }
                if (string.IsNullOrWhiteSpace(address))
                {
                    ModelState.AddModelError("address", "Địa chỉ là bắt buộc");
                }
                if (rentalDays < 1)
                {
                    ModelState.AddModelError("rentalDays", "Số ngày thuê phải lớn hơn 0");
                }

                if (!ModelState.IsValid)
                {
                    var cartTotal = cart.Sum(c => c.TotalPrice);
                    ViewBag.CartItems = cart;
                    ViewBag.TotalAmount = cartTotal;
                    ViewBag.TotalQuantity = cart.Sum(c => c.Quantity);
                    return View("Index");
                }

                // Tìm hoặc tạo Customer
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email && c.Phone == phone);

                if (customer == null)
                {
                    customer = new Customer
                    {
                        FullName = fullName,
                        Email = email,
                        Phone = phone,
                        Address = address,
                        CreatedAt = DateTime.Now
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Cập nhật thông tin nếu có thay đổi
                    customer.FullName = fullName;
                    customer.Address = address;
                    await _context.SaveChangesAsync();
                }

                // Lấy UserId từ session (đã kiểm tra ở đầu method)
                int userId = 1;
                if (int.TryParse(userIdStr, out int userIdInt))
                {
                    userId = userIdInt;
                }
                else
                {
                    // Tìm user đầu tiên có role Admin hoặc Librarian
                    var defaultUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Role == "Admin" || u.Role == "Librarian");
                    if (defaultUser != null)
                    {
                        userId = defaultUser.UserId;
                    }
                }

                // Tính tổng tiền
                decimal rentalTotalAmount = 0;
                foreach (var item in cart)
                {
                    rentalTotalAmount += item.DailyRate * item.Quantity * rentalDays;
                }

                // Xác định trạng thái dựa trên phương thức thanh toán
                string rentalStatus = (paymentMethod == "vnpay") ? "Chờ thanh toán" : "Đang thuê";

                // Tạo Rental
                var rental = new Rental
                {
                    CustomerId = customer.CustomerId,
                    UserId = userId,
                    RentalDate = DateTime.Now,
                    ReturnDate = DateTime.Now.AddDays(rentalDays),
                    TotalAmount = rentalTotalAmount,
                    Status = rentalStatus
                };

                // Tạo RentalDetails
                foreach (var item in cart)
                {
                    var rentalDetail = new RentalDetail
                    {
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        PricePerDay = item.DailyRate
                    };
                    rental.RentalDetails.Add(rentalDetail);

                    // Chỉ giảm số lượng sách nếu đã thanh toán (không phải VnPay)
                    if (paymentMethod != "vnpay")
                    {
                        var book = await _context.Books.FindAsync(item.BookId);
                        if (book != null && book.Quantity >= item.Quantity)
                        {
                            book.Quantity -= item.Quantity;
                        }
                    }
                }

                _context.Rentals.Add(rental);
                await _context.SaveChangesAsync();

                // Nếu thanh toán qua VnPay, tạo URL thanh toán
                if (!string.IsNullOrEmpty(paymentMethod) && paymentMethod.ToLower() == "vnpay")
                {
                    try
                    {
                        // Tạo OrderId từ rentalId để có thể tìm lại sau khi callback
                        var orderId = $"RENTAL_{rental.RentalId}_{DateTime.Now:yyyyMMddHHmmss}";
                        
                        var paymentModel = new PaymentInformationModel
                        {
                            OrderType = "other",
                            Amount = (double)rentalTotalAmount,
                            OrderDescription = $"Thanh toán đơn thuê sách #{rental.RentalId}",
                            Name = fullName,
                            OrderId = orderId
                        };

                        // Lưu rentalId vào session để xử lý sau khi thanh toán
                        if (HttpContext.Session != null)
                        {
                            HttpContext.Session.SetString($"VnPayRental_{rental.RentalId}", rental.RentalId.ToString());
                            HttpContext.Session.SetString("LastVnPayRentalId", rental.RentalId.ToString());
                            // Lưu mapping giữa orderId và rentalId
                            HttpContext.Session.SetString($"VnPayOrder_{orderId}", rental.RentalId.ToString());
                        }

                        var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
                        
                        if (!string.IsNullOrEmpty(paymentUrl))
                        {
                            // Chỉ xóa giỏ hàng khi đã tạo được payment URL thành công
                            ClearCart();
                            return Redirect(paymentUrl);
                        }
                        else
                        {
                            TempData["Error"] = "Không thể tạo URL thanh toán VnPay";
                            // Không xóa giỏ hàng nếu có lỗi
                            return RedirectToAction("Index");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = $"Có lỗi xảy ra khi tạo URL thanh toán VnPay: {ex.Message}";
                        // Không xóa giỏ hàng nếu có exception
                        return RedirectToAction("Index");
                    }
                }

                // Các phương thức thanh toán khác (thanh toán khi nhận, chuyển khoản)
                // Xóa giỏ hàng vì đã thanh toán thành công
                ClearCart();
                TempData["Success"] = $"Đặt thuê thành công! Mã đơn: #{rental.RentalId}";
                return RedirectToAction("Success", new { rentalId = rental.RentalId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Checkout/Success
        public async Task<IActionResult> Success(int rentalId)
        {
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .FirstOrDefaultAsync(r => r.RentalId == rentalId);

            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }
    }
}

