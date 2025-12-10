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

            // 7. Load đánh giá của sách
            List<BookReview> reviews = new List<BookReview>();
            try
            {
                reviews = await _context.BookReviews
                    .Include(r => r.Customer)
                    .Where(r => r.BookId == id)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();
            }
            catch
            {
                // Bảng BookReviews chưa tồn tại, sử dụng danh sách rỗng
                reviews = new List<BookReview>();
            }
            ViewBag.Reviews = reviews;
            ViewBag.ReviewCount = reviews.Count;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            // 8. Kiểm tra xem user đã trả sách này chưa (để cho phép đánh giá)
            var userId = HttpContext.Session?.GetString("UserId");
            bool canReview = false;
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
                if (user != null)
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => 
                            (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                            (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));
                    
                    if (customer != null)
                    {
                        // Kiểm tra xem customer đã thuê và trả sách này chưa
                        var hasReturnedBook = await _context.RentalDetails
                            .Include(rd => rd.Rental)
                            .AnyAsync(rd => rd.BookId == id && 
                                          rd.Rental.CustomerId == customer.CustomerId && 
                                          rd.Rental.Status == "Đã trả");
                        
                        // Kiểm tra xem đã đánh giá chưa
                        bool hasReviewed = false;
                        try
                        {
                            hasReviewed = await _context.BookReviews
                                .AnyAsync(r => r.BookId == id && r.CustomerId == customer.CustomerId);
                        }
                        catch
                        {
                            // Bảng BookReviews chưa tồn tại
                            hasReviewed = false;
                        }
                        
                        canReview = hasReturnedBook && !hasReviewed;
                    }
                }
            }
            ViewBag.CanReview = canReview;

            return View(book);
        }

        // POST: Book/SubmitReview - Gửi đánh giá
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(int bookId, int rating, string comment)
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá." });
            }

            if (!int.TryParse(userId, out int userIdInt))
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => 
                    (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                    (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
            }

            // Kiểm tra xem đã trả sách chưa
            var hasReturnedBook = await _context.RentalDetails
                .Include(rd => rd.Rental)
                .AnyAsync(rd => rd.BookId == bookId && 
                              rd.Rental.CustomerId == customer.CustomerId && 
                              rd.Rental.Status == "Đã trả");

            if (!hasReturnedBook)
            {
                return Json(new { success = false, message = "Bạn chỉ có thể đánh giá sách đã thuê và trả." });
            }

            // Kiểm tra xem đã đánh giá chưa
            BookReview? existingReview = null;
            try
            {
                existingReview = await _context.BookReviews
                    .FirstOrDefaultAsync(r => r.BookId == bookId && r.CustomerId == customer.CustomerId);
            }
            catch
            {
                // Bảng BookReviews chưa tồn tại, tiếp tục tạo review mới
            }

            if (existingReview != null)
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sách này rồi." });
            }

            // Validation
            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao." });
            }

            // Tạo review mới
            var review = new BookReview
            {
                BookId = bookId,
                CustomerId = customer.CustomerId,
                Rating = rating,
                Comment = comment?.Trim(),
                CreatedDate = DateTime.Now
            };

            try
            {
                _context.BookReviews.Add(review);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

    }
}
