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

        // GET: Account/Profile - Trang thông tin cá nhân
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return RedirectToAction("Login", "Account");
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

            return View(user);
        }

        // POST: Account/Profile - Cập nhật thông tin cá nhân
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string fullName, string email, string phone, string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                TempData["Error"] = "Vui lòng đăng nhập để cập nhật thông tin.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            // Validation
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ViewBag.Error = "Họ và tên là bắt buộc";
                return View(user);
            }

            // Kiểm tra email trùng (nếu có thay đổi)
            if (!string.IsNullOrWhiteSpace(email) && email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower() && u.UserId != userIdInt))
                {
                    ViewBag.Error = "Email đã được sử dụng bởi tài khoản khác";
                    return View(user);
                }
            }

            // Kiểm tra mật khẩu nếu muốn đổi
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    ViewBag.Error = "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu";
                    return View(user);
                }

                var hashedCurrentPassword = HashPassword(currentPassword);
                if (user.PasswordHash != hashedCurrentPassword)
                {
                    ViewBag.Error = "Mật khẩu hiện tại không đúng";
                    return View(user);
                }

                if (newPassword.Length < 6)
                {
                    ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự";
                    return View(user);
                }

                if (newPassword != confirmPassword)
                {
                    ViewBag.Error = "Mật khẩu xác nhận không khớp";
                    return View(user);
                }

                user.PasswordHash = HashPassword(newPassword);
            }

            // Cập nhật thông tin
            user.FullName = fullName;
            user.Email = string.IsNullOrWhiteSpace(email) ? null : email;
            user.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;

            try
            {
                await _context.SaveChangesAsync();

                // Cập nhật session
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("Email", user.Email ?? "");

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile", "Account");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi xảy ra khi cập nhật thông tin. Vui lòng thử lại.";
                return View(user);
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



