using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthorController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<AuthorController> _logger;

        public AuthorController(QlthuvienContext context, ILogger<AuthorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Author
        public async Task<IActionResult> Index(string searchString)
        {
            var authorsQuery = _context.Authors
                .Include(a => a.Books)
                .AsQueryable();

            // Tìm kiếm theo tên tác giả
            if (!string.IsNullOrEmpty(searchString))
            {
                authorsQuery = authorsQuery.Where(a => a.AuthorName.Contains(searchString));
            }

            var authors = await authorsQuery
                .OrderBy(a => a.AuthorName)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            return View(authors);
        }

        // GET: Admin/Author/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var author = await _context.Authors
                .Include(a => a.Books)
                    .ThenInclude(b => b.Category)
                .Include(a => a.Books)
                    .ThenInclude(b => b.Authors)
                .FirstOrDefaultAsync(a => a.AuthorId == id);
            if (author == null) return NotFound();
            return View(author);
        }

        // GET: Admin/Author/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Author/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Authors.AnyAsync(a => a.AuthorName.ToLower() == author.AuthorName.ToLower()))
                    {
                        ModelState.AddModelError("AuthorName", "Tên tác giả đã tồn tại.");
                        return View(author);
                    }

                    _context.Add(author);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Tác giả '{author.AuthorName}' đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo tác giả: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo tác giả.");
                }
            }
            return View(author);
        }

        // GET: Admin/Author/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return NotFound();
            return View(author);
        }

        // POST: Admin/Author/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.AuthorId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Authors.AnyAsync(a => a.AuthorName.ToLower() == author.AuthorName.ToLower() && a.AuthorId != id))
                    {
                        ModelState.AddModelError("AuthorName", "Tên tác giả đã tồn tại.");
                        return View(author);
                    }

                    _context.Update(author);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Tác giả '{author.AuthorName}' đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật tác giả: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật tác giả.");
                }
            }
            return View(author);
        }

        // GET: Admin/Author/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var author = await _context.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.AuthorId == id);
            if (author == null) return NotFound();
            return View(author);
        }

        // POST: Admin/Author/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors.Include(a => a.Books).FirstOrDefaultAsync(a => a.AuthorId == id);
            if (author == null)
            {
                TempData["Error"] = "Tác giả không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (author.Books.Any())
            {
                TempData["Error"] = $"Không thể xóa tác giả '{author.AuthorName}' vì có {author.Books.Count} sách đang sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Tác giả '{author.AuthorName}' đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
//

