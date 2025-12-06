using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(QlthuvienContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/User
        public async Task<IActionResult> Index(string searchString, int? roleId)
        {
            var usersQuery = _context.Users
                .Include(u => u.RoleNavigation)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Username.Contains(searchString) ||
                    u.FullName.Contains(searchString) ||
                    (u.Email != null && u.Email.Contains(searchString)));
            }

            // Lọc theo vai trò
            if (roleId.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.RoleId == roleId.Value);
            }

            var users = await usersQuery
                .OrderBy(u => u.UserId)
                .ToListAsync();

            // Dữ liệu cho dropdown filters
            ViewBag.Roles = await _context.TbRoles.OrderBy(r => r.RoleName).ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedRoleId = roleId;

            return View(users);
        }

        // GET: Admin/User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.RoleNavigation)
                .Include(u => u.Orders)
                .Include(u => u.Rentals)
                .Include(u => u.TbBlogs)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/User/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
            return View();
        }

        // POST: Admin/User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra username trùng
                    if (await _context.Users.AnyAsync(u => u.Username.ToLower() == user.Username.ToLower()))
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                        ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
                        return View(user);
                    }

                    // Kiểm tra email trùng
                    if (!string.IsNullOrEmpty(user.Email) &&
                        await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower()))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
                        return View(user);
                    }

                    // Hash mật khẩu
                    user.PasswordHash = HashPassword(password);
                    user.CreatedAt = DateTime.Now;

                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Người dùng '{user.Username}' đã được tạo thành công bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Người dùng '{user.Username}' đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo người dùng: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo người dùng: {ex.Message}. Vui lòng thử lại.");
                }
            }

            ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
            return View(user);
        }

        // GET: Admin/User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
            return View(user);
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string password)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra username trùng (trừ chính nó)
                    if (await _context.Users.AnyAsync(u => u.Username.ToLower() == user.Username.ToLower() && u.UserId != id))
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                        ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
                        return View(user);
                    }

                    // Kiểm tra email trùng (trừ chính nó)
                    if (!string.IsNullOrEmpty(user.Email) &&
                        await _context.Users.AnyAsync(u => u.Email.ToLower() == user.Email.ToLower() && u.UserId != id))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
                        return View(user);
                    }

                    // Cập nhật thông tin
                    existingUser.Username = user.Username;
                    existingUser.FullName = user.FullName;
                    existingUser.Email = user.Email;
                    existingUser.Phone = user.Phone;
                    existingUser.RoleId = user.RoleId;

                    // Chỉ cập nhật mật khẩu nếu có nhập mới
                    if (!string.IsNullOrEmpty(password))
                    {
                        existingUser.PasswordHash = HashPassword(password);
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Người dùng '{user.Username}' (ID: {user.UserId}) đã được cập nhật bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Người dùng '{user.Username}' đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật người dùng ID {id}: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật người dùng: {ex.Message}. Vui lòng thử lại.");
                }
            }

            ViewBag.Roles = _context.TbRoles.Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
            return View(user);
        }

        // GET: Admin/User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.RoleNavigation)
                .Include(u => u.Orders)
                .Include(u => u.Rentals)
                .Include(u => u.TbBlogs)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Orders)
                    .Include(u => u.Rentals)
                    .Include(u => u.TbBlogs)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    _logger.LogWarning($"Không tìm thấy người dùng để xóa: ID {id}");
                    TempData["Error"] = "Người dùng không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra xem người dùng có đang được sử dụng không
                if (user.Orders.Any() || user.Rentals.Any() || user.TbBlogs.Any())
                {
                    _logger.LogWarning($"Không thể xóa người dùng '{user.Username}' vì có dữ liệu liên quan");
                    TempData["Error"] = $"Không thể xóa người dùng '{user.Username}' vì có dữ liệu liên quan (đơn hàng, phiếu thuê, bài viết).";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Người dùng '{user.Username}' (ID: {user.UserId}) đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Người dùng '{user.Username}' đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa người dùng ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}

