using System;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public int? UserId { get; set; } // null = thông báo cho tất cả users

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; }

        public string Type { get; set; } // "info", "success", "warning", "danger"

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ReadDate { get; set; }

        // Navigation property
        public virtual User? User { get; set; }
    }
}

