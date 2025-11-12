using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class TbBlogComment
{
    public int CommentId { get; set; }

    public int BlogId { get; set; }

    public int? CustomerId { get; set; }

    public string CommentText { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public bool IsApproved { get; set; }

    public virtual TbBlog Blog { get; set; } = null!;

    public virtual Customer? Customer { get; set; }
}
