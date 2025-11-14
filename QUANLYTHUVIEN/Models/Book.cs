using System;
using System.Collections.Generic;
using System.Linq; // Cần thêm dòng này để dùng .Select() và .Any()

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

    // DÒNG NÀY ĐÃ BỊ XÓA VÌ MÂU THUẪN VỚI CategoryId
    // public virtual ICollection<Category> Categories { get; set; } = new List<Category>();


    // -----------------------------------------------------------------
    // CÁC THUỘC TÍNH HELPER (Dùng cho View - Không lưu vào CSDL)
    // -----------------------------------------------------------------

    // 1. Cải tiến cho đường dẫn ảnh
    public string CoverImageUrl
    {
        get
        {
            const string basePath = "~/images/books-media/gird-view/";
            const string defaultImage = "book-media-grid-01.jpg";

            // Nếu CoverImage là null hoặc rỗng, dùng ảnh mặc định.
            // Ngược lại, dùng ảnh của sách.
            return basePath + (string.IsNullOrEmpty(CoverImage) ? defaultImage : CoverImage);
        }
    }

    // 2. Cải tiến cho việc hiển thị tên tác giả
    public string AuthorNames
    {
        get
        {
            // Kiểm tra Authors có tồn tại và có phần tử nào không
            if (Authors != null && Authors.Any())
            {
                // Nối tên các tác giả, cách nhau bằng dấu ", "
                return string.Join(", ", Authors.Select(a => a.AuthorName));
            }
            // Trả về "Updating..." nếu không có tác giả
            return "Updating...";
        }
    }

    // 3. Cải tiến cho việc hiển thị ISBN
    public string DisplayIsbn
    {
        get => string.IsNullOrEmpty(Isbn) ? "Updating..." : Isbn;
    }

    // 4. Cải tiến cho việc cắt ngắn mô tả
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