using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class RentalDetail
{
    public int RentalId { get; set; }

    public int BookId { get; set; }

    public int? Quantity { get; set; }

    public decimal? PricePerDay { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Rental Rental { get; set; } = null!;
}
