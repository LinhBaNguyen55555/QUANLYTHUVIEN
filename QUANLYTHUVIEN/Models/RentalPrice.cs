using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Models;

public partial class RentalPrice
{
    public int PriceId { get; set; }

    public int BookId { get; set; }

    public decimal DailyRate { get; set; }

    public decimal? WeeklyRate { get; set; }

    public decimal? MonthlyRate { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public virtual Book Book { get; set; } = null!;
}
