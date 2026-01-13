using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace QUANLYTHUVIEN.Models;

public partial class Book
{
    [Key]
    public int BookId { get; set; }

    [Required(ErrorMessage = "Tiêu đề sách là bắt buộc")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
    [Display(Name = "Tiêu đề sách")]
    public string Title { get; set; } = null!;

    [StringLength(20, ErrorMessage = "ISBN không được vượt quá 20 ký tự")]
    [Display(Name = "ISBN")]
    public string? Isbn { get; set; }

    [Required(ErrorMessage = "Thể loại là bắt buộc")]
    [Display(Name = "Thể loại")]
    public int? CategoryId { get; set; }

    [Display(Name = "Nhà xuất bản")]
    public int? PublisherId { get; set; }

    [Display(Name = "Ngôn ngữ")]
    public int? LanguageId { get; set; }

    [Range(1000, 2100, ErrorMessage = "Năm xuất bản phải từ 1000 đến 2100")]
    [Display(Name = "Năm xuất bản")]
    public int? PublishedYear { get; set; }

    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
    [Display(Name = "Số lượng")]
    public int? Quantity { get; set; }

    [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Ảnh bìa")]
    public string? CoverImage { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Language? Language { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<RentalDetail> RentalDetails { get; set; } = new List<RentalDetail>();

    public virtual ICollection<RentalPrice> RentalPrices { get; set; } = new List<RentalPrice>();

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    public virtual ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();


    
    public string CoverImageUrl
    {
        get
        {
            const string basePath = "~/images/books-media/gird-view/";
            const string defaultImage = "book-media-grid-01.jpg";

            
            if (string.IsNullOrEmpty(CoverImage))
            {
                return basePath + defaultImage;
            }

            var coverImage = CoverImage.Trim();

            
            if (coverImage.StartsWith("/images/") || coverImage.StartsWith("~/images/"))
            {
                
                return coverImage.StartsWith("~/") ? coverImage : "~" + coverImage;
            }
            else if (coverImage.StartsWith("images/"))
            {
                
                return "~/" + coverImage;
            }
            else if (coverImage.StartsWith("/"))
            {
                
                return "~" + coverImage;
            }
            else if (coverImage.Contains("/"))
            {
                
                return "~/images/" + coverImage;
            }
            else
            {
                
                return basePath + coverImage;
            }
        }
    }

    
    public string AuthorNames
    {
        get
        {
            
            if (Authors != null && Authors.Any())
            {
                
                return string.Join(", ", Authors.Select(a => a.AuthorName));
            }
            
            return "Updating...";
        }
    }

    
    public string DisplayIsbn
    {
        get => string.IsNullOrEmpty(Isbn) ? "Updating..." : Isbn;
    }

    
    public string TruncatedDescription
    {
        get
        {
            if (string.IsNullOrEmpty(Description))
            {
                return "No description available.";
            }

            return Description.Length > 150
                ? Description.Substring(0, 150) + "..."
                : Description;
        }
    }
}
