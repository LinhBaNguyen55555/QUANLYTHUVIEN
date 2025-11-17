using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class TbBlog
{
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public string? Alias { get; set; }

    public string? Content { get; set; }

    public string? Image { get; set; }

    public int? AuthorId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool IsPublished { get; set; }

    public int? Views { get; set; }

    public virtual User? Author { get; set; }

    public virtual ICollection<TbBlogComment> TbBlogComments { get; set; } = new List<TbBlogComment>();
    public string TruncatedContent
    {
        get
        {
            if (string.IsNullOrEmpty(Content))
            {
                return "No content available.";
            }

            // Cắt ngắn nội dung (ví dụ: 150 ký tự)
            return Content.Length > 1500
                ? Content.Substring(0, 500)
                : Content;
        }
    }
    public string BlogImageUrl
    {
        get
        {
            // 1. Đường dẫn cơ sở
            const string basePath = "~/images/blog/";

            // 2. Ảnh mặc định (nếu 'Image' trong CSDL bị null hoặc rỗng)
            const string defaultImage = "blog-10.jpg";

            // 3. Logic: 
            // Nếu 'Image' rỗng, dùng default. Ngược lại, dùng 'Image'.
            return basePath + (string.IsNullOrEmpty(this.Image) ? defaultImage : this.Image);
        }
    }
}
