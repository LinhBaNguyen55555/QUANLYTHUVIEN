using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class Rental
{
    public int RentalId { get; set; }

    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public DateTime? RentalDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? Status { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<RentalDetail> RentalDetails { get; set; } = new List<RentalDetail>();

    public virtual User User { get; set; } = null!;
}
