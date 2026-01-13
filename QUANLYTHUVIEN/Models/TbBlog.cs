using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models;

public partial class TbBlog
{
    [Key]
    public int BlogId { get; set; }

    [Required(ErrorMessage = "Tiêu đề bài viết là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Alias không được vượt quá 200 ký tự")]
    [Display(Name = "Alias")]
    public string? Alias { get; set; }

    [Required(ErrorMessage = "Nội dung bài viết là bắt buộc")]
    [Display(Name = "Nội dung")]
    public string? Content { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? Image { get; set; }

    [Display(Name = "Tác giả")]
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
            
            const string basePath = "~/images/blog/";

            
            const string defaultImage = "blog-10.jpg";

            
            return basePath + (string.IsNullOrEmpty(this.Image) ? defaultImage : this.Image);
        }
    }
}
