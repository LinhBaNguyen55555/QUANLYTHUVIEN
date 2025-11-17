using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = null!;

    public string? Biography { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    // --- BẮT ĐẦU THÊM MỚI ---

    /// <summary>
    /// (Helper) Tự động tạo đường dẫn ảnh, dùng ảnh mặc định nếu 'Image' là null
    /// </summary>
    public string AvatarUrl
    {
        get
        {
            // Bạn có thể đổi thư mục avatars nếu muốn
            const string basePath = "~/images/Authors/";

            // Đặt tên cho một ảnh đại diện mặc định
            const string defaultImage = "default-avatar.jpg";

            return basePath + (string.IsNullOrEmpty(this.Image) ? defaultImage : this.Image);
        }
    }

    /// <summary>
    /// (Helper) Tự động cắt ngắn tiểu sử (Biography)
    /// </summary>
    public string TruncatedBiography
    {
        get
        {
            if (string.IsNullOrEmpty(Biography))
            {
                return "Không có tiểu sử.";
            }

            // Cắt ngắn (200 ký tự) để vừa khung trích dẫn
            return Biography.Length > 500
                ? Biography.Substring(0, 500)
                : Biography;
        }
    }
    // --- KẾT THÚC THÊM MỚI ---
}
