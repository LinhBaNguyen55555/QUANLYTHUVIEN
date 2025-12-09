using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Security.Cryptography;
using System.Text;

namespace QUANLYTHUVIEN.Controllers
{
    public class AccountController : Controller
    {
        private readonly QlthuvienContext _context;

        public AccountController(QlthuvienContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
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

            // Lưu thông tin user vào session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "");
            HttpContext.Session.SetString("RoleId", user.RoleId?.ToString() ?? "");
            HttpContext.Session.SetString("RoleName", user.RoleNavigation?.RoleName ?? "");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string fullName, string email, string phone)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Tên đăng nhập là bắt buộc";
                return View();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Mật khẩu là bắt buộc";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ViewBag.Error = "Họ và tên là bắt buộc";
                return View();
            }

            // Kiểm tra username đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại";
                return View();
            }

            // Kiểm tra email đã tồn tại (nếu có)
            if (!string.IsNullOrWhiteSpace(email) && await _context.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower()))
            {
                ViewBag.Error = "Email đã được sử dụng";
                return View();
            }

            // Tạo user mới
            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                FullName = fullName,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                CreatedAt = DateTime.Now,
                Role = "Customer" // Mặc định là Customer
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Tự động đăng nhập sau khi đăng ký
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Email", user.Email ?? "");
                HttpContext.Session.SetString("Role", user.Role ?? "");
                HttpContext.Session.SetString("RoleId", user.RoleId?.ToString() ?? "");
                HttpContext.Session.SetString("RoleName", "");

                TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với thư viện.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại.";
                return View();
            }
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Đã đăng xuất thành công";
            return RedirectToAction("Index", "Home");
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


