using Microsoft.AspNetCore.Mvc;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để truy cập trang quản trị";
                return RedirectToAction("Login", "Account", new { area = "Admin", returnUrl = Request.Path });
            }

        
            var roleName = HttpContext.Session.GetString("RoleName");
            var isAdmin = !string.IsNullOrEmpty(roleName) && 
                         (roleName.ToLower() == "admin" || roleName.ToLower() == "administrator");

            if (!isAdmin)
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang quản trị";
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account", new { area = "Admin" });
            }

            return RedirectToAction("Dashboard", "Report", new { area = "Admin" });
        }
    }
}
