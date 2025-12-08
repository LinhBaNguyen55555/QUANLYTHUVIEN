namespace QUANLYTHUVIEN.Models
{
    public class CartItem
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string AuthorNames { get; set; } = null!;
        public string CoverImageUrl { get; set; } = null!;
        public decimal DailyRate { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => DailyRate * Quantity;
    }
}

