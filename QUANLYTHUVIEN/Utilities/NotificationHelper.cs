using QUANLYTHUVIEN.Models;
using Microsoft.EntityFrameworkCore;

namespace QUANLYTHUVIEN.Utilities
{
    public static class NotificationHelper
    {
        /// <summary>
        /// Tạo thông báo cho user
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="userId">User ID (null = thông báo chung cho tất cả)</param>
        /// <param name="title">Tiêu đề thông báo</param>
        /// <param name="content">Nội dung thông báo</param>
        /// <param name="type">Loại thông báo: "info", "success", "warning", "danger"</param>
        public static async Task CreateNotificationAsync(QlthuvienContext context, int? userId, string title, string content, string type = "info")
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Content = content,
                    Type = type,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error nhưng không throw để không ảnh hưởng đến flow chính
                System.Diagnostics.Debug.WriteLine($"Error creating notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo thông báo cho tất cả admin
        /// </summary>
        public static async Task CreateNotificationForAdminsAsync(QlthuvienContext context, string title, string content, string type = "info")
        {
            try
            {
                // Lấy tất cả admin users
                var adminUsers = await context.Users
                    .Where(u => u.Role != null && (u.Role.ToLower() == "admin" || u.Role.ToLower() == "administrator"))
                    .Select(u => u.UserId)
                    .ToListAsync();

                // Tạo thông báo cho từng admin
                foreach (var adminId in adminUsers)
                {
                    await CreateNotificationAsync(context, adminId, title, content, type);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating notification for admins: {ex.Message}");
            }
        }
    }
}

