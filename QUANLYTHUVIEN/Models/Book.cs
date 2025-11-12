using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string? Isbn { get; set; }

    public int? CategoryId { get; set; }

    public int? PublisherId { get; set; }

    public int? LanguageId { get; set; }

    public int? PublishedYear { get; set; }

    public int? Quantity { get; set; }

    public string? Description { get; set; }

    public string? CoverImage { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Language? Language { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<RentalDetail> RentalDetails { get; set; } = new List<RentalDetail>();

    public virtual ICollection<RentalPrice> RentalPrices { get; set; } = new List<RentalPrice>();

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
