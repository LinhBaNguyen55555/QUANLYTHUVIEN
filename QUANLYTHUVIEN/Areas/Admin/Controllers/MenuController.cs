using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<MenuController> _logger;

        public MenuController(QlthuvienContext context, ILogger<MenuController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Menu
        public async Task<IActionResult> Index(string searchString, string status)
        {
            var menusQuery = _context.TbMenus
                .Include(m => m.Parent)
                .AsQueryable();

            // Tìm kiếm theo tên menu
            if (!string.IsNullOrEmpty(searchString))
            {
                menusQuery = menusQuery.Where(m => m.Title.Contains(searchString));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Active")
                {
                    menusQuery = menusQuery.Where(m => m.IsActive);
                }
                else if (status == "Inactive")
                {
                    menusQuery = menusQuery.Where(m => !m.IsActive);
                }
            }

            var menus = await menusQuery
                .OrderBy(m => m.MenuId)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.SelectedStatus = status;

            return View(menus);
        }

        // GET: Admin/Menu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbMenu = await _context.TbMenus
                .Include(m => m.Parent)
                .FirstOrDefaultAsync(m => m.MenuId == id);
            if (tbMenu == null)
            {
                return NotFound();
            }

            return View(tbMenu);
        }

        // GET: Admin/Menu/Create
        public IActionResult Create()
        {
            ViewBag.ParentMenus = _context.TbMenus
                .Where(m => m.IsActive)
                .OrderBy(m => m.Position)
                .ToList();
            return View();
        }

        // POST: Admin/Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TbMenu tbMenu)
        {
            _logger.LogInformation($"Bắt đầu tạo menu: Title={tbMenu?.Title}, Levels={tbMenu?.Levels}, Position={tbMenu?.Position}, IsActive={tbMenu?.IsActive}");

            // Đảm bảo các giá trị mặc định
            if (tbMenu == null)
            {
                tbMenu = new TbMenu();
                ModelState.AddModelError("", "Dữ liệu form không hợp lệ.");
            }

            // Kiểm tra menu không tự tham chiếu đến chính nó (không áp dụng cho Create vì MenuId = 0)
            // Nhưng vẫn kiểm tra để đảm bảo logic đúng

            // Validate thủ công các trường bắt buộc
            if (string.IsNullOrWhiteSpace(tbMenu.Title))
            {
                ModelState.AddModelError("Title", "Tiêu đề menu là bắt buộc.");
            }

            if (tbMenu.Levels < 1 || tbMenu.Levels > 3)
            {
                ModelState.AddModelError("Levels", "Cấp menu phải từ 1 đến 3.");
            }

            if (tbMenu.Position < 1)
            {
                ModelState.AddModelError("Position", "Vị trí phải là số dương.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    tbMenu.CreatedDate = DateTime.Now;
                    tbMenu.CreatedBy = User.Identity?.Name ?? "Admin";

                    _context.Add(tbMenu);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Menu '{tbMenu.Title}' đã được tạo thành công bởi {tbMenu.CreatedBy} với ID {tbMenu.MenuId}");
                    TempData["Success"] = $"Menu '{tbMenu.Title}' đã được tạo thành công!";

                    // Chuyển hướng về trang nguồn
                    string referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer) && referer.Contains("/Details"))
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = tbMenu.MenuId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo menu: {ex.Message}");
                    _logger.LogError(ex, $"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo menu: {ex.Message}. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo menu:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        _logger.LogWarning($"  - Field '{key}': {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            ViewBag.ParentMenus = _context.TbMenus
                .Where(m => m.IsActive)
                .OrderBy(m => m.Position)
                .ToList();
            return View(tbMenu);
        }

        // GET: Admin/Menu/Edit/5
        public async Task<IActionResult> Edit(int? id, string fromDetails)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbMenu = await _context.TbMenus.FindAsync(id);
            if (tbMenu == null)
            {
                return NotFound();
            }

            ViewBag.ParentMenus = _context.TbMenus
                .Where(m => m.IsActive && m.MenuId != id)
                .OrderBy(m => m.Position)
                .ToList();
            ViewBag.FromDetails = !string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true";
            return View(tbMenu);
        }

        // POST: Admin/Menu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TbMenu tbMenu, string fromDetails)
        {
            if (id != tbMenu.MenuId)
            {
                _logger.LogWarning($"ID không khớp: {id} != {tbMenu.MenuId}");
                return NotFound();
            }

            // Xóa lỗi validation của fromDetails (không phải là trường của model)
            ModelState.Remove("fromDetails");

            // Lấy menu hiện tại từ database
            var existingMenu = await _context.TbMenus.FindAsync(id);
            if (existingMenu == null)
            {
                return NotFound();
            }

            // Kiểm tra menu không tự tham chiếu đến chính nó
            if (tbMenu.ParentId.HasValue && tbMenu.ParentId.Value == tbMenu.MenuId)
            {
                ModelState.AddModelError("ParentId", "Menu không thể là cha của chính nó.");
            }

            // Kiểm tra vòng lặp tham chiếu (menu con không thể là cha của menu cha)
            if (tbMenu.ParentId.HasValue)
            {
                var parentMenu = await _context.TbMenus.FindAsync(tbMenu.ParentId.Value);
                if (parentMenu != null && parentMenu.ParentId.HasValue && parentMenu.ParentId.Value == tbMenu.MenuId)
                {
                    ModelState.AddModelError("ParentId", "Không thể tạo vòng lặp tham chiếu giữa các menu.");
                }
            }

            // Validate thủ công các trường bắt buộc
            if (string.IsNullOrWhiteSpace(tbMenu.Title))
            {
                ModelState.AddModelError("Title", "Tiêu đề menu là bắt buộc.");
            }

            if (tbMenu.Levels < 1 || tbMenu.Levels > 3)
            {
                ModelState.AddModelError("Levels", "Cấp menu phải từ 1 đến 3.");
            }

            if (tbMenu.Position < 1)
            {
                ModelState.AddModelError("Position", "Vị trí phải là số dương.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật các trường từ form
                    existingMenu.Title = tbMenu.Title;
                    existingMenu.Alias = tbMenu.Alias;
                    existingMenu.Url = tbMenu.Url;
                    existingMenu.Description = tbMenu.Description;
                    existingMenu.Levels = tbMenu.Levels;
                    existingMenu.ParentId = tbMenu.ParentId;
                    existingMenu.Position = tbMenu.Position;
                    existingMenu.IsActive = tbMenu.IsActive;
                    existingMenu.ModifiedDate = DateTime.Now;
                    existingMenu.ModifiedBy = User.Identity?.Name ?? "Admin";

                    _context.Update(existingMenu);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Menu '{existingMenu.Title}' (ID: {existingMenu.MenuId}) đã được cập nhật bởi {existingMenu.ModifiedBy}");
                    TempData["Success"] = $"Menu '{existingMenu.Title}' đã được cập nhật thành công!";

                    // Chuyển hướng về trang nguồn
                    if (!string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true")
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = existingMenu.MenuId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbMenuExists(tbMenu.MenuId))
                    {
                        _logger.LogWarning($"Menu không tồn tại: ID {tbMenu.MenuId}");
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError($"Lỗi concurrency khi cập nhật menu ID {tbMenu.MenuId}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật menu ID {tbMenu.MenuId}: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật menu. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning($"ModelState không hợp lệ khi cập nhật menu ID {id}");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
            }

            ViewBag.ParentMenus = _context.TbMenus
                .Where(m => m.IsActive && m.MenuId != id)
                .OrderBy(m => m.Position)
                .ToList();
            
            // Cập nhật model với giá trị từ form để hiển thị lại
            existingMenu.Title = tbMenu.Title;
            existingMenu.Alias = tbMenu.Alias;
            existingMenu.Url = tbMenu.Url;
            existingMenu.Description = tbMenu.Description;
            existingMenu.Levels = tbMenu.Levels;
            existingMenu.ParentId = tbMenu.ParentId;
            existingMenu.Position = tbMenu.Position;
            existingMenu.IsActive = tbMenu.IsActive;
            
            return View(existingMenu);
        }

        // GET: Admin/Menu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tbMenu = await _context.TbMenus
                .Include(m => m.Parent)
                .FirstOrDefaultAsync(m => m.MenuId == id);
            if (tbMenu == null)
            {
                return NotFound();
            }

            return View(tbMenu);
        }

        // POST: Admin/Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var tbMenu = await _context.TbMenus.FindAsync(id);
                if (tbMenu == null)
                {
                    _logger.LogWarning($"Không tìm thấy menu để xóa: ID {id}");
                    TempData["Error"] = "Menu không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }

                // Kiểm tra xem có menu con không
                var hasChildren = await _context.TbMenus.AnyAsync(m => m.ParentId == id);
                if (hasChildren)
                {
                    _logger.LogWarning($"Không thể xóa menu '{tbMenu.Title}' vì có menu con");
                    TempData["Error"] = $"Không thể xóa menu '{tbMenu.Title}' vì nó có menu con. Vui lòng xóa menu con trước.";
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }

                _context.TbMenus.Remove(tbMenu);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Menu '{tbMenu.Title}' (ID: {tbMenu.MenuId}) đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Menu '{tbMenu.Title}' đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa menu ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa menu. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }

        private bool TbMenuExists(int id)
        {
            return _context.TbMenus.Any(e => e.MenuId == id);
        }
    }
}
