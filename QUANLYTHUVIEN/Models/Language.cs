using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models;

public partial class Language
{
    [Key]
    public int LanguageId { get; set; }

    [Required(ErrorMessage = "Tên ngôn ngữ là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tên ngôn ngữ không được vượt quá 50 ký tự")]
    [Display(Name = "Tên ngôn ngữ")]
    public string LanguageName { get; set; } = null!;

    [Display(Name = "Sách")]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
