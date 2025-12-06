using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using QUANLYTHUVIEN.Utilities;
using System.Text.RegularExpressions;

namespace QUANLYTHUVIEN.Controllers
{
    public class BookController : Controller
    {
        private readonly QlthuvienContext _context;

        public BookController(QlthuvienContext context)
        {
            _context = context;
        }
        [Route("sach")]
        [Route("books")]
        public async Task<IActionResult> Index(string keywords, string catalog, string category, string orderby, int page = 1)
        {
            // Load categories và authors cho dropdown tìm kiếm và sidebar
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Authors = await _context.Authors
                .Include(a => a.Books)
                .OrderBy(a => a.AuthorName)
                .ToListAsync();

            // Query sách với các điều kiện tìm kiếm
            var booksQuery = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.RentalPrices)
                .AsQueryable();

            // Tìm kiếm theo từ khóa
            if (!string.IsNullOrEmpty(keywords))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(keywords) ||
                    (b.Description != null && b.Description.Contains(keywords)) ||
                    (b.Isbn != null && b.Isbn.Contains(keywords)) ||
                    b.Authors.Any(a => a.AuthorName.Contains(keywords)) ||
                    (b.Category != null && b.Category.CategoryName.Contains(keywords)) ||
                    (b.Publisher != null && b.Publisher.PublisherName.Contains(keywords)));
            }

            // Lọc theo tác giả (catalog = tên tác giả)
            if (!string.IsNullOrEmpty(catalog) && catalog != "Tìm kiếm theo tác giả" && catalog != "Search the Catalog" && catalog.Trim() != "")
            {
                booksQuery = booksQuery.Where(b => b.Authors.Any(a => a.AuthorName == catalog));
            }

            // Lọc theo thể loại
            if (!string.IsNullOrEmpty(category) && category != "Tất cả thể loại" && category != "All Categories" && category.Trim() != "")
            {
                booksQuery = booksQuery.Where(b => b.Category != null && b.Category.CategoryName == category);
            }

            // Sắp xếp
            switch (orderby)
            {
                case "Sort by popularity":
                    // Có thể sắp xếp theo số lượt thuê hoặc view
                    booksQuery = booksQuery.OrderByDescending(b => b.BookId);
                    break;
                case "Sort by newness":
                    booksQuery = booksQuery.OrderByDescending(b => b.BookId);
                    break;
                case "Sort by price":
                    // Sắp xếp theo giá thuê
                    booksQuery = booksQuery.OrderBy(b => b.RentalPrices.OrderByDescending(rp => rp.EffectiveDate).FirstOrDefault().DailyRate);
                    break;
                default:
                    booksQuery = booksQuery.OrderByDescending(b => b.BookId);
                    break;
            }

            // Phân trang
            int pageSize = 12;
            var totalBooks = await booksQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var books = await booksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính số lượng có sẵn cho từng sách
            foreach (var book in books)
            {
                var rentedCount = await _context.RentalDetails
                    .Include(rd => rd.Rental)
                    .Where(rd => rd.BookId == book.BookId && rd.Rental.Status == "Đang thuê")
                    .SumAsync(rd => rd.Quantity ?? 0);

                var totalQuantity = book.Quantity ?? 0;
                var availableQuantity = totalQuantity - rentedCount;
                ViewBag.AvailableQuantities = ViewBag.AvailableQuantities ?? new Dictionary<int, int>();
                ((Dictionary<int, int>)ViewBag.AvailableQuantities)[book.BookId] = Math.Max(0, availableQuantity);
            }

            // ViewBag cho phân trang và filters
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBooks = totalBooks;
            ViewBag.Keywords = keywords;
            ViewBag.Catalog = catalog;
            ViewBag.Category = category;
            ViewBag.OrderBy = orderby;
            ViewBag.StartItem = (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalBooks);

            return View(books);
        }
        [Route("book/{alias}-{id}.html")]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            // 1. Lấy thông tin chi tiết sách với đầy đủ thông tin
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.RentalPrices)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // 2. Lấy sách liên quan (cùng thể loại)
            ViewBag.BooksRelated = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.Publisher)
                .Where(b => b.BookId != id && b.CategoryId == book.CategoryId)
                .OrderByDescending(b => b.BookId)
                .Take(6) // Lấy 6 cuốn
                .ToListAsync();

            // 3. Lấy bảng giá hiện tại (mới nhất)
            ViewBag.CurrentRentalPrice = book.RentalPrices
                .OrderByDescending(rp => rp.EffectiveDate)
                .FirstOrDefault();

            // 4. Tính số lượng sách đang được thuê (status = "Đang thuê")
            var rentedCount = await _context.RentalDetails
                .Include(rd => rd.Rental)
                .Where(rd => rd.BookId == id && rd.Rental.Status == "Đang thuê")
                .SumAsync(rd => rd.Quantity ?? 0);

            // 5. Tính số lượng có sẵn
            var totalQuantity = book.Quantity ?? 0;
            var availableQuantity = totalQuantity - rentedCount;
            ViewBag.AvailableQuantity = Math.Max(0, availableQuantity);
            ViewBag.RentedCount = rentedCount;

            // 6. Load categories và authors cho dropdown tìm kiếm
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();

            return View(book);
        }

    }
}
