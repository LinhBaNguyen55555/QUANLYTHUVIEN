using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class TbMenu
{
    public int MenuId { get; set; }

    public string Title { get; set; } = null!;

    public string? Alias { get; set; }

    public string? Url { get; set; }

    public string? Description { get; set; }

    public int? Levels { get; set; }

    public int? ParentId { get; set; }

    public int? Position { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<TbMenu> InverseParent { get; set; } = new List<TbMenu>();

    public virtual TbMenu? Parent { get; set; }
}
