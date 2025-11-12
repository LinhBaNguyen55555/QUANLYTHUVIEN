using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<Book> BooksNavigation { get; set; } = new List<Book>();
}
