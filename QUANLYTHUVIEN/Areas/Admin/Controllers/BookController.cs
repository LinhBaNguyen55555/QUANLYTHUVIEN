using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<BookController> _logger;

        public BookController(QlthuvienContext context, ILogger<BookController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Book
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? authorId)
        {
            var booksQuery = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.Authors)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(searchString) ||
                    b.Isbn.Contains(searchString) ||
                    b.Authors.Any(a => a.AuthorName.Contains(searchString)) ||
                    b.Category.CategoryName.Contains(searchString) ||
                    b.Publisher.PublisherName.Contains(searchString));
            }

            // Lọc theo thể loại
            if (categoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId == categoryId.Value);
            }

            // Lọc theo tác giả
            if (authorId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.Authors.Any(a => a.AuthorId == authorId.Value));
            }

            var books = await booksQuery
                .OrderBy(b => b.BookId)
                .ToListAsync();

            // Dữ liệu cho dropdown filters
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedAuthorId = authorId;

            return View(books);
        }

        // GET: Admin/Book/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Admin/Book/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
            ViewBag.Publishers = _context.Publishers.OrderBy(p => p.PublisherName).ToList();
            ViewBag.Languages = _context.Languages.OrderBy(l => l.LanguageName).ToList();
            ViewBag.Authors = _context.Authors.OrderBy(a => a.AuthorName).ToList();
            return View();
        }

        // POST: Admin/Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, int[] selectedAuthors)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Thêm các tác giả được chọn
                    if (selectedAuthors != null && selectedAuthors.Length > 0)
                    {
                        foreach (var authorId in selectedAuthors)
                        {
                            var author = await _context.Authors.FindAsync(authorId);
                            if (author != null)
                            {
                                book.Authors.Add(author);
                            }
                        }
                    }

                    _context.Add(book);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Sách '{book.Title}' đã được tạo thành công bởi {User.Identity?.Name ?? "Admin"} với ID {book.BookId}");
                    TempData["Success"] = $"Sách '{book.Title}' đã được tạo thành công!";

                    // Chuyển hướng về trang nguồn
                    string referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer) && referer.Contains("/Details"))
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = book.BookId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo sách: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo sách: {ex.Message}. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo sách:");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        _logger.LogWarning($"  - Field '{key}': {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
            ViewBag.Publishers = _context.Publishers.OrderBy(p => p.PublisherName).ToList();
            ViewBag.Languages = _context.Languages.OrderBy(l => l.LanguageName).ToList();
            ViewBag.Authors = _context.Authors.OrderBy(a => a.AuthorName).ToList();
            return View(book);
        }

        // GET: Admin/Book/Edit/5
        public async Task<IActionResult> Edit(int? id, string fromDetails)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
            ViewBag.Publishers = _context.Publishers.OrderBy(p => p.PublisherName).ToList();
            ViewBag.Languages = _context.Languages.OrderBy(l => l.LanguageName).ToList();
            ViewBag.Authors = _context.Authors.OrderBy(a => a.AuthorName).ToList();
            ViewBag.SelectedAuthors = book.Authors.Select(a => a.AuthorId).ToArray();
            ViewBag.FromDetails = !string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true";

            return View(book);
        }

        // POST: Admin/Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, int[] selectedAuthors, string fromDetails)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            // Xóa lỗi validation của fromDetails (không phải là trường của model)
            ModelState.Remove("fromDetails");

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin sách
                    var existingBook = await _context.Books
                        .Include(b => b.Authors)
                        .FirstOrDefaultAsync(b => b.BookId == id);

                    if (existingBook == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các trường cơ bản
                    existingBook.Title = book.Title;
                    existingBook.Isbn = book.Isbn;
                    existingBook.CategoryId = book.CategoryId;
                    existingBook.PublisherId = book.PublisherId;
                    existingBook.LanguageId = book.LanguageId;
                    existingBook.PublishedYear = book.PublishedYear;
                    existingBook.Quantity = book.Quantity;
                    existingBook.Description = book.Description;
                    existingBook.CoverImage = book.CoverImage;

                    // Cập nhật danh sách tác giả
                    existingBook.Authors.Clear();
                    if (selectedAuthors != null && selectedAuthors.Length > 0)
                    {
                        foreach (var authorId in selectedAuthors)
                        {
                            var author = await _context.Authors.FindAsync(authorId);
                            if (author != null)
                            {
                                existingBook.Authors.Add(author);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Sách '{book.Title}' (ID: {book.BookId}) đã được cập nhật bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Sách '{book.Title}' đã được cập nhật thành công!";

                    // Chuyển hướng về trang nguồn
                    if (!string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true")
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = book.BookId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật sách ID {id}: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật sách: {ex.Message}. Vui lòng thử lại.");
                }
            }
            else
            {
                _logger.LogWarning($"ModelState không hợp lệ khi cập nhật sách ID {id}");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        _logger.LogWarning($"  - Field '{key}': {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }

            ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
            ViewBag.Publishers = _context.Publishers.OrderBy(p => p.PublisherName).ToList();
            ViewBag.Languages = _context.Languages.OrderBy(l => l.LanguageName).ToList();
            ViewBag.Authors = _context.Authors.OrderBy(a => a.AuthorName).ToList();
            ViewBag.SelectedAuthors = selectedAuthors ?? new int[0];

            return View(book);
        }

        // GET: Admin/Book/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Admin/Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var book = await _context.Books
                    .Include(b => b.Authors)
                    .FirstOrDefaultAsync(b => b.BookId == id);

                if (book == null)
                {
                    _logger.LogWarning($"Không tìm thấy sách để xóa: ID {id}");
                    TempData["Error"] = "Sách không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }

                // Kiểm tra xem sách có đang được sử dụng không
                var hasOrders = await _context.OrderDetails.AnyAsync(od => od.BookId == id);
                var hasRentals = await _context.RentalDetails.AnyAsync(rd => rd.BookId == id);
                var hasRentalPrices = await _context.RentalPrices.AnyAsync(rp => rp.BookId == id);

                if (hasOrders || hasRentals)
                {
                    _logger.LogWarning($"Không thể xóa sách '{book.Title}' vì có dữ liệu liên quan");
                    TempData["Error"] = $"Không thể xóa sách '{book.Title}' vì sách đang có trong đơn hàng hoặc phiếu thuê.";
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }

                // Xóa các bản ghi RentalPrice liên quan trước
                if (hasRentalPrices)
                {
                    var rentalPrices = await _context.RentalPrices
                        .Where(rp => rp.BookId == id)
                        .ToListAsync();
                    _context.RentalPrices.RemoveRange(rentalPrices);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Đã xóa {rentalPrices.Count} bản ghi giá thuê liên quan đến sách '{book.Title}'");
                }

                // Xóa quan hệ với tác giả
                book.Authors.Clear();

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Sách '{book.Title}' (ID: {book.BookId}) đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Sách '{book.Title}' đã được xóa thành công!";
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, $"Lỗi database khi xóa sách ID {id}: {dbEx.Message}");
                TempData["Error"] = "Không thể xóa sách vì có ràng buộc dữ liệu. Vui lòng kiểm tra lại các đơn hàng, phiếu thuê hoặc dữ liệu liên quan khác.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa sách ID {id}: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra khi xóa sách: {ex.Message}. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
}


