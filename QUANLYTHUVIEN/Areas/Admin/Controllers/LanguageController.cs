using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LanguageController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<LanguageController> _logger;

        public LanguageController(QlthuvienContext context, ILogger<LanguageController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var languages = await _context.Languages
                .Include(l => l.Books)
                .OrderBy(l => l.LanguageName)
                .ToListAsync();
            return View(languages);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Language language)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Languages.AnyAsync(l => l.LanguageName.ToLower() == language.LanguageName.ToLower()))
                    {
                        ModelState.AddModelError("LanguageName", "Tên ngôn ngữ đã tồn tại.");
                        return View(language);
                    }

                    _context.Add(language);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Ngôn ngữ '{language.LanguageName}' đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo ngôn ngữ: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo ngôn ngữ.");
                }
            }
            return View(language);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var language = await _context.Languages
                .Include(l => l.Books)
                    .ThenInclude(b => b.Authors)
                .Include(l => l.Books)
                    .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(l => l.LanguageId == id);
            if (language == null) return NotFound();
            return View(language);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var language = await _context.Languages.FindAsync(id);
            if (language == null) return NotFound();
            return View(language);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Language language)
        {
            if (id != language.LanguageId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Languages.AnyAsync(l => l.LanguageName.ToLower() == language.LanguageName.ToLower() && l.LanguageId != id))
                    {
                        ModelState.AddModelError("LanguageName", "Tên ngôn ngữ đã tồn tại.");
                        return View(language);
                    }

                    _context.Update(language);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Ngôn ngữ '{language.LanguageName}' đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật ngôn ngữ: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật ngôn ngữ.");
                }
            }
            return View(language);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var language = await _context.Languages.Include(l => l.Books).FirstOrDefaultAsync(l => l.LanguageId == id);
            if (language == null) return NotFound();
            return View(language);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var language = await _context.Languages.Include(l => l.Books).FirstOrDefaultAsync(l => l.LanguageId == id);
            if (language == null)
            {
                TempData["Error"] = "Ngôn ngữ không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            if (language.Books.Any())
            {
                TempData["Error"] = $"Không thể xóa ngôn ngữ '{language.LanguageName}' vì có {language.Books.Count} sách đang sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Ngôn ngữ '{language.LanguageName}' đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
