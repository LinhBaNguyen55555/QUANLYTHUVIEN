using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models;

public partial class Publisher
{
    [Key]
    public int PublisherId { get; set; }

    [Required(ErrorMessage = "Tên nhà xuất bản là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên nhà xuất bản không được vượt quá 100 ký tự")]
    [Display(Name = "Tên nhà xuất bản")]
    public string PublisherName { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Sách")]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
