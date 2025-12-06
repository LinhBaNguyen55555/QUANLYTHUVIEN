using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RentalController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<RentalController> _logger;

        public RentalController(QlthuvienContext context, ILogger<RentalController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Rental
        public async Task<IActionResult> Index(string searchString, string status, int? customerId)
        {
            var rentalsQuery = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.User)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .AsQueryable();

            // Tìm kiếm theo tên khách hàng
            if (!string.IsNullOrEmpty(searchString))
            {
                rentalsQuery = rentalsQuery.Where(r =>
                    r.Customer.FullName.Contains(searchString) ||
                    (r.User != null && r.User.FullName.Contains(searchString)));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                rentalsQuery = rentalsQuery.Where(r => r.Status == status);
            }

            // Lọc theo khách hàng
            if (customerId.HasValue)
            {
                rentalsQuery = rentalsQuery.Where(r => r.CustomerId == customerId.Value);
            }

            var rentals = await rentalsQuery
                .OrderByDescending(r => r.RentalDate)
                .ToListAsync();

            // Dữ liệu cho dropdown filters
            ViewBag.Customers = await _context.Customers.OrderBy(c => c.FullName).ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedCustomerId = customerId;

            return View(rentals);
        }

        // GET: Admin/Rental/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.User)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .FirstOrDefaultAsync(m => m.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // GET: Admin/Rental/Create
        public IActionResult Create()
        {
            ViewBag.Customers = _context.Customers.OrderBy(c => c.FullName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View();
        }

        // POST: Admin/Rental/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Rental rental, int[] selectedBooks, int[] quantities)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    rental.RentalDate = DateTime.Now;
                    rental.Status = "Active";

                    // Tính tổng tiền
                    decimal totalAmount = 0;
                    for (int i = 0; i < selectedBooks.Length; i++)
                    {
                        var bookId = selectedBooks[i];
                        var quantity = quantities[i];

                        var book = await _context.Books.FindAsync(bookId);
                        if (book != null && book.Quantity >= quantity)
                        {
                            var rentalPrice = book.RentalPrices?.FirstOrDefault()?.DailyRate ?? 5000; // Giá mặc định 5000 VNĐ/ngày
                            var rentalDetail = new RentalDetail
                            {
                                BookId = bookId,
                                Quantity = quantity,
                                PricePerDay = rentalPrice
                            };
                            rental.RentalDetails.Add(rentalDetail);
                            totalAmount += rentalPrice * quantity * 7; // Giả sử thuê 7 ngày
                        }
                    }

                    rental.TotalAmount = totalAmount;

                    _context.Add(rental);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Phiếu thuê mới đã được tạo cho khách hàng '{rental.Customer?.FullName}' bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Phiếu thuê đã được tạo thành công cho khách hàng '{rental.Customer?.FullName}'!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo phiếu thuê: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo phiếu thuê: {ex.Message}. Vui lòng thử lại.");
                }
            }

            ViewBag.Customers = _context.Customers.OrderBy(c => c.FullName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View(rental);
        }

        // GET: Admin/Rental/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.RentalDetails)
                .FirstOrDefaultAsync(r => r.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            ViewBag.Customers = _context.Customers.OrderBy(c => c.FullName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View(rental);
        }

        // POST: Admin/Rental/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Rental rental)
        {
            if (id != rental.RentalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRental = await _context.Rentals
                        .Include(r => r.RentalDetails)
                        .FirstOrDefaultAsync(r => r.RentalId == id);

                    if (existingRental == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin cơ bản
                    existingRental.CustomerId = rental.CustomerId;
                    existingRental.UserId = rental.UserId;
                    existingRental.Status = rental.Status;

                    // Nếu trả sách
                    if (rental.ReturnDate.HasValue && !existingRental.ReturnDate.HasValue)
                    {
                        existingRental.ReturnDate = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Phiếu thuê ID {id} đã được cập nhật bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Phiếu thuê đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật phiếu thuê ID {id}: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật phiếu thuê: {ex.Message}. Vui lòng thử lại.");
                }
            }

            ViewBag.Customers = _context.Customers.OrderBy(c => c.FullName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View(rental);
        }

        // GET: Admin/Rental/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.User)
                .Include(r => r.RentalDetails)
                    .ThenInclude(rd => rd.Book)
                .FirstOrDefaultAsync(m => m.RentalId == id);

            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Admin/Rental/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var rental = await _context.Rentals
                    .Include(r => r.RentalDetails)
                    .FirstOrDefaultAsync(r => r.RentalId == id);

                if (rental == null)
                {
                    _logger.LogWarning($"Không tìm thấy phiếu thuê để xóa: ID {id}");
                    TempData["Error"] = "Phiếu thuê không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa chi tiết phiếu thuê trước
                _context.RentalDetails.RemoveRange(rental.RentalDetails);
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Phiếu thuê ID {id} đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Phiếu thuê đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa phiếu thuê ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa phiếu thuê. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Rental/ReturnBook/5
        [HttpPost]
        public async Task<IActionResult> ReturnBook(int id)
        {
            try
            {
                var rental = await _context.Rentals.FindAsync(id);
                if (rental == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phiếu thuê!" });
                }

                if (rental.ReturnDate.HasValue)
                {
                    return Json(new { success = false, message = "Sách đã được trả rồi!" });
                }

                rental.ReturnDate = DateTime.Now;
                rental.Status = "Returned";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Sách đã được trả cho phiếu thuê ID {id} bởi {User.Identity?.Name ?? "Admin"}");
                return Json(new { success = true, message = "Sách đã được trả thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi trả sách cho phiếu thuê ID {id}: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi trả sách!" });
            }
        }
    }
}

