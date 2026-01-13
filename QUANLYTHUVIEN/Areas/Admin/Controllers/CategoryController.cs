using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(QlthuvienContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Category
        public async Task<IActionResult> Index(string searchString)
        {
            var categoriesQuery = _context.Categories.AsQueryable();

            
            if (!string.IsNullOrEmpty(searchString))
            {
                categoriesQuery = categoriesQuery.Where(c =>
                    c.CategoryName.Contains(searchString) ||
                    (c.Description != null && c.Description.Contains(searchString)));
            }

            var categories = await categoriesQuery
                .Include(c => c.Books)
                .OrderBy(c => c.CategoryId)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            return View(categories);
        }

        // GET: Admin/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _context.Categories
                    .Include(c => c.Books)
                        .ThenInclude(b => b.Authors)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.CategoryId == id);
                    
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy chi tiết thể loại ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết thể loại.";
                return RedirectToAction("Index", "Category", new { area = "Admin" });
            }
        }

        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == category.CategoryName.ToLower()))
                    {
                        ModelState.AddModelError("CategoryName", "Tên thể loại đã tồn tại.");
                        return View(category);
                    }

                    _context.Add(category);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Thể loại '{category.CategoryName}' đã được tạo thành công bởi {User.Identity?.Name ?? "Admin"} với ID {category.CategoryId}");
                    TempData["Success"] = $"Thể loại '{category.CategoryName}' đã được tạo thành công!";

                    
                    string referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer) && referer.Contains("/Details"))
                    {
                        return RedirectToAction("Details", "Category", new { area = "Admin", id = category.CategoryId });
                    }
                    return RedirectToAction("Index", "Category", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo thể loại: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo thể loại: {ex.Message}. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo thể loại:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        _logger.LogWarning($"  - Field '{key}': {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            return View(category);
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int? id, string fromDetails)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            ViewBag.FromDetails = !string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true";
            return View(category);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category, string fromDetails)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            
            ModelState.Remove("fromDetails");

            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == category.CategoryName.ToLower() && c.CategoryId != id))
                    {
                        ModelState.AddModelError("CategoryName", "Tên thể loại đã tồn tại.");
                        return View(category);
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Thể loại '{category.CategoryName}' (ID: {category.CategoryId}) đã được cập nhật bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Thể loại '{category.CategoryName}' đã được cập nhật thành công!";

                    
                    if (!string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true")
                    {
                        return RedirectToAction("Details", "Category", new { area = "Admin", id = category.CategoryId });
                    }
                    return RedirectToAction("Index", "Category", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật thể loại ID {id}: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật thể loại: {ex.Message}. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning($"ModelState không hợp lệ khi cập nhật thể loại ID {id}");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        _logger.LogWarning($"  - Field '{key}': {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            return View(category);
        }

        // GET: Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Books)
                    .FirstOrDefaultAsync(c => c.CategoryId == id);

                if (category == null)
                {
                    _logger.LogWarning($"Không tìm thấy thể loại để xóa: ID {id}");
                    TempData["Error"] = "Thể loại không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction("Index", "Category", new { area = "Admin" });
                }

                
                if (category.Books.Any())
                {
                    _logger.LogWarning($"Không thể xóa thể loại '{category.CategoryName}' vì có sách đang sử dụng");
                    TempData["Error"] = $"Không thể xóa thể loại '{category.CategoryName}' vì có {category.Books.Count} sách đang thuộc thể loại này.";
                    return RedirectToAction("Index", "Category", new { area = "Admin" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Thể loại '{category.CategoryName}' (ID: {category.CategoryId}) đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Thể loại '{category.CategoryName}' đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa thể loại ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa thể loại. Vui lòng thử lại.";
            }

            return RedirectToAction("Index", "Category", new { area = "Admin" });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}


