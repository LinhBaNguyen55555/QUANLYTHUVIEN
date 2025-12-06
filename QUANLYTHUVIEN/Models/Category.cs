using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models;

public partial class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên thể loại là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên thể loại không được vượt quá 100 ký tự")]
    [Display(Name = "Tên thể loại")]
    public string CategoryName { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Sách")]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
