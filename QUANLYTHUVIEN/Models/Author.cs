using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models;

public partial class Author
{
    [Key]
    public int AuthorId { get; set; }

    [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên tác giả không được vượt quá 100 ký tự")]
    [Display(Name = "Tên tác giả")]
    public string AuthorName { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Tiểu sử không được vượt quá 2000 ký tự")]
    [Display(Name = "Tiểu sử")]
    public string? Biography { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public string? Image { get; set; }

    [Display(Name = "Sách")]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    
    public string AvatarUrl
    {
        get
        {
            
            const string basePath = "~/images/Authors/";

            
            const string defaultImage = "default-avatar.jpg";

            return basePath + (string.IsNullOrEmpty(this.Image) ? defaultImage : this.Image);
        }
    }

    
    public string TruncatedBiography
    {
        get
        {
            if (string.IsNullOrEmpty(Biography))
            {
                return "Không có tiểu sử.";
            }

            
            return Biography.Length > 500
                ? Biography.Substring(0, 500)
                : Biography;
        }
    }
  
}
