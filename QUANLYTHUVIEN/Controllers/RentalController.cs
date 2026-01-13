using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using Microsoft.AspNetCore.Http;
using QUANLYTHUVIEN.Services;
using QUANLYTHUVIEN.Utilities;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Controllers
{
    public class RentalController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly IVnPayService _vnPayService;

        public RentalController(QlthuvienContext context, IVnPayService vnPayService)
        {
            _context = context;
            _vnPayService = vnPayService;
        }

        // GET: Rental/MyRentals
        [Route("sach-dang-thue")]
        [Route("Rental/MyRentals")]
        [Route("services/bookrental")]
        public async Task<IActionResult> MyRentals()
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem sách đang thuê.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyRentals", "Rental") });
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => 
                    (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                    (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

            if (customer == null)
            {
                ViewBag.Message = "Bạn chưa có sách đang thuê nào.";
                return View(new List<Rental>());
            }

            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                        .ThenInclude(b => b.Authors)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                        .ThenInclude(b => b.Category)
                .Where(r => r.CustomerId == customer.CustomerId)
                .Where(r => r.Status == "Đang thuê" || r.Status == "Chờ thanh toán" || r.Status == "Quá hạn")
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync();
            
            foreach (var rental in rentals.Where(r => r.Status == "Đã thanh toán" && !r.ReturnDate.HasValue))
            {
                rental.Status = "Đang thuê";
            }
            if (rentals.Any(r => r.Status == "Đã thanh toán"))
            {
                await _context.SaveChangesAsync();
            }

            foreach (var rental in rentals)
            {
                if (rental.ReturnDate.HasValue && rental.ReturnDate.Value < DateTime.Now && rental.Status != "Đã trả")
                {
                    rental.Status = "Quá hạn";
                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.CustomerName = customer.FullName;
            ViewBag.CustomerEmail = customer.Email;
            ViewBag.CustomerPhone = customer.Phone;

            return View(rentals);
        }

        // GET: Rental/RentalHistory 
        [Route("sach-da-thue")]
        [Route("Rental/RentalHistory")]
        public async Task<IActionResult> RentalHistory()
        {
            
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem lịch sử thuê sách.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("RentalHistory", "Rental") });
            }

            
            if (!int.TryParse(userId, out int userIdInt))
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => 
                    (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                    (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

            if (customer == null)
            {
                ViewBag.Message = "Bạn chưa có sách đã thuê nào.";
                return View(new List<Rental>());
            }

            
            var rentals = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                        .ThenInclude(b => b.Authors)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                        .ThenInclude(b => b.Category)
                .Where(r => r.CustomerId == customer.CustomerId)
                .Where(r => r.Status == "Đã trả")
                .OrderByDescending(r => r.ReturnDate ?? r.RentalDate)
                .ToListAsync();

            
            Dictionary<int, bool> bookReviewStatus = new Dictionary<int, bool>();
            try
            {
                var reviewedBookIds = await _context.BookReviews
                    .Where(r => r.CustomerId == customer.CustomerId)
                    .Select(r => r.BookId)
                    .ToListAsync();
                
                foreach (var rental in rentals)
                {
                    foreach (var detail in rental.RentalDetails)
                    {
                        if (!bookReviewStatus.ContainsKey(detail.BookId))
                        {
                            bookReviewStatus[detail.BookId] = reviewedBookIds.Contains(detail.BookId);
                        }
                    }
                }
            }
            catch
            {
            }

            ViewBag.CustomerName = customer.FullName;
            ViewBag.CustomerEmail = customer.Email;
            ViewBag.CustomerPhone = customer.Phone;
            ViewBag.BookReviewStatus = bookReviewStatus;

            return View(rentals);
        }

        // GET: Rental/Details/5
        [Route("sach-dang-thue/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem chi tiết.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Rental", new { id }) });
            }

          
            if (!int.TryParse(userId, out int userIdInt))
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

          
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => 
                    (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                    (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("MyRentals", "Rental");
            }

            
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.User)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                        .ThenInclude(b => b.Authors)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                        .ThenInclude(b => b.Category)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                        .ThenInclude(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            
            if (rental.CustomerId != customer.CustomerId)
            {
                TempData["Error"] = "Bạn không có quyền xem phiếu thuê này.";
                return RedirectToAction("MyRentals", "Rental");
            }

            
            if (rental.ReturnDate.HasValue && rental.ReturnDate.Value < DateTime.Now && rental.Status != "Đã trả")
            {
                rental.Status = "Quá hạn";
                await _context.SaveChangesAsync();
            }

            return View(rental);
        }

        // GET: Rental/Repay/5 - Hiển thị trang thanh toán lại
        [HttpGet]
        [Route("Rental/Repay/{id}")]
        public async Task<IActionResult> Repay(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

           
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để thanh toán.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Repay", "Rental", new { id }) });
            }

           
            if (!int.TryParse(userId, out int userIdInt))
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => 
                    (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                    (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("MyRentals", "Rental");
            }

            
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            
            if (rental.CustomerId != customer.CustomerId)
            {
                TempData["Error"] = "Bạn không có quyền thanh toán đơn này.";
                return RedirectToAction("MyRentals", "Rental");
            }

            
            if (rental.Status != "Chờ thanh toán")
            {
                TempData["Error"] = "Đơn hàng này không thể thanh toán lại.";
                return RedirectToAction("Details", "Rental", new { id });
            }

            ViewBag.Rental = rental;
            ViewBag.CustomerName = customer.FullName;
            ViewBag.CustomerEmail = customer.Email;
            ViewBag.CustomerPhone = customer.Phone;

            return View();
        }

        // POST: Rental/Repay/5 - Xử lý thanh toán lại
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Rental/Repay/{id}")]
        public async Task<IActionResult> Repay(int id, string paymentMethod)
        {
            
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            
            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("MyRentals", "Rental");
            }

            
            if (rental.Status != "Chờ thanh toán")
            {
                TempData["Error"] = "Đơn hàng này không thể thanh toán lại.";
                return RedirectToAction("Details", "Rental", new { id });
            }

            
            if (!string.IsNullOrEmpty(paymentMethod) && paymentMethod.ToLower() == "vnpay")
            {
                
                try
                {
                    var orderId = $"RENTAL_{rental.RentalId}_{DateTime.Now:yyyyMMddHHmmss}";
                    
                    var paymentModel = new PaymentInformationModel
                    {
                        OrderType = "other",
                        Amount = (double)(rental.TotalAmount ?? 0),
                        OrderDescription = $"Thanh toán lại đơn thuê sách #{rental.RentalId}",
                        Name = rental.Customer?.FullName ?? "Khách hàng",
                        OrderId = orderId
                    };

                  
                    if (HttpContext.Session != null)
                    {
                        HttpContext.Session.SetString($"VnPayRental_{rental.RentalId}", rental.RentalId.ToString());
                        HttpContext.Session.SetString("LastVnPayRentalId", rental.RentalId.ToString());
                        HttpContext.Session.SetString($"VnPayOrder_{orderId}", rental.RentalId.ToString());
                    }

                    var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
                    
                    if (!string.IsNullOrEmpty(paymentUrl))
                    {
                        return Redirect(paymentUrl);
                    }
                    else
                    {
                        TempData["Error"] = "Không thể tạo URL thanh toán VnPay";
                        return RedirectToAction("Repay", "Rental", new { id });
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Có lỗi xảy ra khi tạo URL thanh toán: {ex.Message}";
                    return RedirectToAction("Repay", "Rental", new { id });
                }
            }
            else
            {
                
                rental.Status = "Đang thuê";
                
                
                foreach (var detail in rental.RentalDetails)
                {
                    var book = await _context.Books.FindAsync(detail.BookId);
                    if (book != null && book.Quantity >= (detail.Quantity ?? 0))
                    {
                        book.Quantity -= (detail.Quantity ?? 0);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Thanh toán thành công! Đơn hàng #{rental.RentalId} đã được cập nhật.";
                return RedirectToAction("Details", "Rental", new { id });
            }
        }

        // POST: Rental/ReturnBook/5 
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReturnBook(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ReturnBook called with id: {id}");
                
                if (id <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("ReturnBook: Invalid ID");
                    return Json(new { success = false, message = "ID đơn hàng không hợp lệ." });
                }
                
                var rentalId = id;
                
                
                var userId = HttpContext.Session?.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để trả sách." });
                }

                
                if (!int.TryParse(userId, out int userIdInt))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => 
                        (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                        (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
                }

                
                var rental = await _context.Rentals
                    .AsNoTracking()
                    .Include(r => r.RentalDetails)
                        .ThenInclude(rd => rd.Book)
                    .FirstOrDefaultAsync(r => r.RentalId == id);

                if (rental == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                
                if (rental.CustomerId != customer.CustomerId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền trả sách cho đơn này." });
                }

                
                if (rental.Status == "Đã trả")
                {
                    return Json(new { success = false, message = "Sách đã được trả rồi!" });
                }

               
                if (rental.Status == "Chờ thanh toán")
                {
                    return Json(new { success = false, message = "Vui lòng thanh toán đơn hàng trước khi trả sách." });
                }

               
                var rentalToUpdate = await _context.Rentals
                    .FirstOrDefaultAsync(r => r.RentalId == id);
                
                if (rentalToUpdate == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng để cập nhật." });
                }

                
                if (rentalToUpdate.Status == "Đã trả")
                {
                    return Json(new { success = false, message = "Sách đã được trả rồi!" });
                }
                
                
                if (rentalToUpdate.ReturnDate.HasValue && rentalToUpdate.Status != "Đã trả")
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Rental {id} has ReturnDate but status is not 'Đã trả'. Resetting ReturnDate.");
                }

                
                rentalToUpdate.ReturnDate = DateTime.Now;
                rentalToUpdate.Status = "Đã trả";

                
                var bookTitles = string.Join(", ", rental.RentalDetails.Take(3).Select(rd => rd.Book?.Title ?? "Sách"));
                if (rental.RentalDetails.Count > 3)
                {
                    bookTitles += $" và {rental.RentalDetails.Count - 3} cuốn khác";
                }

                await NotificationHelper.CreateNotificationAsync(
                    _context,
                    userIdInt,
                    "Trả sách thành công",
                    $"Bạn đã trả sách thành công cho đơn #{rentalId}. Sách: {bookTitles}. Cảm ơn bạn đã sử dụng dịch vụ!",
                    "success"
                );

               
                foreach (var detail in rental.RentalDetails)
                {
                    if (detail.BookId > 0)
                    {
                        var book = await _context.Books.FindAsync(detail.BookId);
                        if (book != null)
                        {
                            book.Quantity = (book.Quantity ?? 0) + (detail.Quantity ?? 0);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Warning: Book {detail.BookId} not found when returning rental {id}");
                        }
                    }
                }

                try
                {
                    var rowsAffected = await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"ReturnBook success for rental {id}, rows affected: {rowsAffected}");
                    
                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Trả sách thành công!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Không có thay đổi nào được lưu vào database." });
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Database error: {dbEx.Message}\nInner: {dbEx.InnerException?.Message}");
                    return Json(new { success = false, message = $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReturnBook error: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}

