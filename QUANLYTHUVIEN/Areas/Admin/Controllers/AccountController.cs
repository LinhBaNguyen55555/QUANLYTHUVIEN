using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Security.Cryptography;
using System.Text;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly QlthuvienContext _context;

        public AccountController(QlthuvienContext context)
        {
            _context = context;
        }

        // GET: Admin/Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            // Nếu đã đăng nhập và là admin, redirect đến dashboard
            var userId = HttpContext.Session.GetString("UserId");
            var roleName = HttpContext.Session.GetString("RoleName");
            var isAdmin = !string.IsNullOrEmpty(roleName) && 
                         (roleName.ToLower() == "admin" || roleName.ToLower() == "administrator");

            if (!string.IsNullOrEmpty(userId) && isAdmin)
            {
                return RedirectToAction("Dashboard", "Report", new { area = "Admin" });
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Admin/Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var hashedPassword = HashPassword(password);
            var user = await _context.Users
                .Include(u => u.RoleNavigation)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Kiểm tra quyền admin
            var roleName = user.RoleNavigation?.RoleName ?? user.Role ?? "";
            var isAdmin = !string.IsNullOrEmpty(roleName) && 
                         (roleName.ToLower() == "admin" || roleName.ToLower() == "administrator");

            if (!isAdmin)
            {
                ViewBag.Error = "Bạn không có quyền truy cập vào trang quản trị. Chỉ tài khoản Admin mới được phép đăng nhập.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Lưu thông tin user vào session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "");
            HttpContext.Session.SetString("RoleId", user.RoleId?.ToString() ?? "");
            HttpContext.Session.SetString("RoleName", roleName);

            // Redirect đến Dashboard sau khi đăng nhập thành công
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Dashboard", "Report", new { area = "Admin" });
        }

        // GET: Admin/Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Đã đăng xuất thành công";
            return RedirectToAction("Login", "Account", new { area = "Admin" });
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








