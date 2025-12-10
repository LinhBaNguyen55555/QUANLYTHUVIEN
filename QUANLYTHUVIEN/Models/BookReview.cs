using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QUANLYTHUVIEN.Models;

public partial class BookReview
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReviewId { get; set; }

    public int BookId { get; set; }

    public int CustomerId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
    public int Rating { get; set; }

    [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
    public string? Comment { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}

