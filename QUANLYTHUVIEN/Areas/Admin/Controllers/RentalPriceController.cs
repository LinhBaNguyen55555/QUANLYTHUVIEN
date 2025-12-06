using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RentalPriceController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<RentalPriceController> _logger;

        public RentalPriceController(QlthuvienContext context, ILogger<RentalPriceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/RentalPrice
        public async Task<IActionResult> Index(string searchString, int? bookId)
        {
            var rentalPricesQuery = _context.RentalPrices
                .Include(rp => rp.Book)
                .AsQueryable();

            // Tìm kiếm theo tên sách
            if (!string.IsNullOrEmpty(searchString))
            {
                rentalPricesQuery = rentalPricesQuery.Where(rp =>
                    rp.Book.Title.Contains(searchString));
            }

            // Lọc theo sách
            if (bookId.HasValue)
            {
                rentalPricesQuery = rentalPricesQuery.Where(rp => rp.BookId == bookId.Value);
            }

            var rentalPrices = await rentalPricesQuery
                .OrderByDescending(rp => rp.EffectiveDate)
                .ThenBy(rp => rp.Book.Title)
                .ToListAsync();

            // Load danh sách sách cho dropdown
            ViewBag.Books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.BookId = bookId;

            return View(rentalPrices);
        }

        // GET: Admin/RentalPrice/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalPrice = await _context.RentalPrices
                .Include(rp => rp.Book)
                .FirstOrDefaultAsync(m => m.PriceId == id);

            if (rentalPrice == null)
            {
                return NotFound();
            }

            return View(rentalPrice);
        }

        // GET: Admin/RentalPrice/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
            return View();
        }

        // POST: Admin/RentalPrice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,DailyRate,WeeklyRate,MonthlyRate,EffectiveDate")] RentalPrice rentalPrice)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu không có EffectiveDate, đặt mặc định là ngày hiện tại
                    if (!rentalPrice.EffectiveDate.HasValue)
                    {
                        rentalPrice.EffectiveDate = DateOnly.FromDateTime(DateTime.Now);
                    }

                    _context.Add(rentalPrice);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Bảng giá thuê sách ID {rentalPrice.PriceId} đã được tạo thành công bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = "Bảng giá thuê sách đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo bảng giá thuê sách: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bảng giá. Vui lòng thử lại.");
                }
            }

            ViewBag.Books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
            return View(rentalPrice);
        }

        // GET: Admin/RentalPrice/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalPrice = await _context.RentalPrices.FindAsync(id);
            if (rentalPrice == null)
            {
                return NotFound();
            }

            ViewBag.Books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
            return View(rentalPrice);
        }

        // POST: Admin/RentalPrice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PriceId,BookId,DailyRate,WeeklyRate,MonthlyRate,EffectiveDate")] RentalPrice rentalPrice)
        {
            if (id != rentalPrice.PriceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu không có EffectiveDate, đặt mặc định là ngày hiện tại
                    if (!rentalPrice.EffectiveDate.HasValue)
                    {
                        rentalPrice.EffectiveDate = DateOnly.FromDateTime(DateTime.Now);
                    }

                    _context.Update(rentalPrice);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Bảng giá thuê sách ID {rentalPrice.PriceId} đã được cập nhật thành công bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = "Bảng giá thuê sách đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalPriceExists(rentalPrice.PriceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật bảng giá thuê sách ID {id}: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật bảng giá. Vui lòng thử lại.");
                }
            }

            ViewBag.Books = await _context.Books
                .OrderBy(b => b.Title)
                .ToListAsync();
            return View(rentalPrice);
        }

        // GET: Admin/RentalPrice/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalPrice = await _context.RentalPrices
                .Include(rp => rp.Book)
                .FirstOrDefaultAsync(m => m.PriceId == id);

            if (rentalPrice == null)
            {
                return NotFound();
            }

            return View(rentalPrice);
        }

        // POST: Admin/RentalPrice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rentalPrice = await _context.RentalPrices.FindAsync(id);
            if (rentalPrice != null)
            {
                try
                {
                    _context.RentalPrices.Remove(rentalPrice);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Bảng giá thuê sách ID {id} đã được xóa thành công bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = "Bảng giá thuê sách đã được xóa thành công!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi xóa bảng giá thuê sách ID {id}: {ex.Message}");
                    TempData["Error"] = "Có lỗi xảy ra khi xóa bảng giá. Vui lòng thử lại.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RentalPriceExists(int id)
        {
            return _context.RentalPrices.Any(e => e.PriceId == id);
        }
    }
}

