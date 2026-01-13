using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYTHUVIEN.Controllers
{
    public class NotificationController : Controller
    {
        private readonly QlthuvienContext _context;

        public NotificationController(QlthuvienContext context)
        {
            _context = context;
        }

        // GET: Notification
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem thông báo.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

           
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userIdInt || n.UserId == null)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

           
            var unreadCount = notifications.Count(n => !n.IsRead);
            ViewBag.UnreadCount = unreadCount;

            return View(notifications);
        }

        // POST: Notification/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && (n.UserId == userIdInt || n.UserId == null));

            if (notification == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông báo." });
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }

           
            var unreadCount = await _context.Notifications
                .CountAsync(n => (n.UserId == userIdInt || n.UserId == null) && !n.IsRead);

            return Json(new { success = true, unreadCount = unreadCount });
        }

        // POST: Notification/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var notifications = await _context.Notifications
                .Where(n => (n.UserId == userIdInt || n.UserId == null) && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã đánh dấu tất cả thông báo là đã đọc." });
        }

        // POST: Notification/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && (n.UserId == userIdInt || n.UserId == null));

            if (notification == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông báo." });
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa thông báo." });
        }

        // GET: Notification/GetUnreadCount 
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
            {
                return Json(new { count = 0 });
            }

            var count = await _context.Notifications
                .CountAsync(n => (n.UserId == userIdInt || n.UserId == null) && !n.IsRead);

            return Json(new { count = count });
        }
    }
}


