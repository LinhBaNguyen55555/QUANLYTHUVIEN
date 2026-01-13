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
            
            ViewBag.Categories = await _context.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Authors = await _context.Authors
                .Include(a => a.Books)
                .OrderBy(a => a.AuthorName)
                .ToListAsync();

            
            var booksQuery = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.RentalPrices)
                .AsQueryable();

            
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

            
            if (!string.IsNullOrEmpty(catalog) && catalog != "Tìm kiếm theo tác giả" && catalog != "Search the Catalog" && catalog.Trim() != "")
            {
                booksQuery = booksQuery.Where(b => b.Authors.Any(a => a.AuthorName == catalog));
            }

            
            if (!string.IsNullOrEmpty(category) && category != "Tất cả thể loại" && category != "All Categories" && category.Trim() != "")
            {
                booksQuery = booksQuery.Where(b => b.Category != null && b.Category.CategoryName == category);
            }

            
            switch (orderby)
            {
                case "Sort by popularity":
                    
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

            
            int pageSize = 12;
            var totalBooks = await booksQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var books = await booksQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

           
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

            
            ViewBag.BooksRelated = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors)
                .Include(b => b.Publisher)
                .Where(b => b.BookId != id && b.CategoryId == book.CategoryId)
                .OrderByDescending(b => b.BookId)
                .Take(6) 
                .ToListAsync();

            
            ViewBag.CurrentRentalPrice = book.RentalPrices
                .OrderByDescending(rp => rp.EffectiveDate)
                .FirstOrDefault();

            
            var rentedCount = await _context.RentalDetails
                .Include(rd => rd.Rental)
                .Where(rd => rd.BookId == id && rd.Rental.Status == "Đang thuê")
                .SumAsync(rd => rd.Quantity ?? 0);

            
            var totalQuantity = book.Quantity ?? 0;
            var availableQuantity = totalQuantity - rentedCount;
            ViewBag.AvailableQuantity = Math.Max(0, availableQuantity);
            ViewBag.RentedCount = rentedCount;

            
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();

            
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
                reviews = new List<BookReview>();
            }
            ViewBag.Reviews = reviews;
            ViewBag.ReviewCount = reviews.Count;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            
            var userId = HttpContext.Session?.GetString("UserId");
            bool canReview = false;
            Customer? customer = null;
            int? currentCustomerId = null;
            
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userIdInt);
                if (user != null)
                {
                    customer = await _context.Customers
                        .FirstOrDefaultAsync(c => 
                            (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                            (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));
                    
                    if (customer != null)
                    {
                        currentCustomerId = customer.CustomerId;
                        
                        var hasReturnedBook = await _context.RentalDetails
                            .Include(rd => rd.Rental)
                            .AnyAsync(rd => rd.BookId == id && 
                                          rd.Rental.CustomerId == customer.CustomerId && 
                                          rd.Rental.Status == "Đã trả");
                        
                        
                        bool hasReviewed = false;
                        try
                        {
                            hasReviewed = await _context.BookReviews
                                .AnyAsync(r => r.BookId == id && r.CustomerId == customer.CustomerId);
                        }
                        catch
                        {
                            hasReviewed = false;
                        }
                        
                        canReview = hasReturnedBook && !hasReviewed;
                    }
                }
            }
            ViewBag.CanReview = canReview;
            ViewBag.CurrentCustomerId = currentCustomerId;

            return View(book);
        }

        // POST: Book/SubmitReview
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

            var hasReturnedBook = await _context.RentalDetails
                .Include(rd => rd.Rental)
                .AnyAsync(rd => rd.BookId == bookId && 
                              rd.Rental.CustomerId == customer.CustomerId && 
                              rd.Rental.Status == "Đã trả");

            if (!hasReturnedBook)
            {
                return Json(new { success = false, message = "Bạn chỉ có thể đánh giá sách đã thuê và trả." });
            }

            BookReview? existingReview = null;
            try
            {
                existingReview = await _context.BookReviews
                    .FirstOrDefaultAsync(r => r.BookId == bookId && r.CustomerId == customer.CustomerId);
            }
            catch
            {
            }

            if (existingReview != null)
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sách này rồi." });
            }

            
            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao." });
            }

            
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

        // POST: Book/DeleteReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để xóa đánh giá." });
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

           
            var review = await _context.BookReviews.FindAsync(reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại." });
            }

            
            if (review.CustomerId != customer.CustomerId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa đánh giá này." });
            }

            try
            {
                _context.BookReviews.Remove(review);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đánh giá đã được xóa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Book/UpdateReview 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReview(int reviewId, int rating, string comment)
        {
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để chỉnh sửa đánh giá." });
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

            
            var review = await _context.BookReviews.FindAsync(reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại." });
            }

            
            if (review.CustomerId != customer.CustomerId)
            {
                return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa đánh giá này." });
            }

            
            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao." });
            }

            if (comment != null && comment.Length > 1000)
            {
                return Json(new { success = false, message = "Bình luận không được vượt quá 1000 ký tự." });
            }

            try
            {
                review.Rating = rating;
                review.Comment = comment?.Trim();
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đánh giá đã được cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

    }
}
