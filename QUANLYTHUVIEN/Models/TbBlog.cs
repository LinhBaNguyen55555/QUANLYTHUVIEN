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
}
