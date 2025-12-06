using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models;

public partial class TbMenu
{
    [Key]
    public int MenuId { get; set; }

    [Required(ErrorMessage = "Tiêu đề menu là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Alias không được vượt quá 100 ký tự")]
    [Display(Name = "Alias")]
    public string? Alias { get; set; }

    [Display(Name = "URL đích")]
    public string? Url { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Cấp menu là bắt buộc")]
    [Range(1, 3, ErrorMessage = "Cấp menu phải từ 1 đến 3")]
    [Display(Name = "Cấp menu")]
    public int Levels { get; set; }

    [Display(Name = "Menu cha")]
    public int? ParentId { get; set; }

    [Required(ErrorMessage = "Vị trí là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "Vị trí phải là số dương")]
    [Display(Name = "Vị trí sắp xếp")]
    public int Position { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime? CreatedDate { get; set; }

    [Display(Name = "Người tạo")]
    public string? CreatedBy { get; set; }

    [Display(Name = "Ngày cập nhật")]
    public DateTime? ModifiedDate { get; set; }

    [Display(Name = "Người cập nhật")]
    public string? ModifiedBy { get; set; }

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; }

    [Display(Name = "Menu con")]
    public virtual ICollection<TbMenu> InverseParent { get; set; } = new List<TbMenu>();

    [Display(Name = "Menu cha")]
    public virtual TbMenu? Parent { get; set; }
}
