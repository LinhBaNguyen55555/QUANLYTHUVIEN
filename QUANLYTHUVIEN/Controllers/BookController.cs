using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;

namespace QUANLYTHUVIEN.Controllers
{
    public class BookController : Controller
    {
        private readonly QlthuvienContext _context;

        public BookController(QlthuvienContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("book/{alias}-{id}.html")]
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            // 1. Lấy thông tin chi tiết sách
            // Lưu ý: Trong file Book.cs của bạn, quan hệ tên là "Category", không phải "CategoryBooks"
            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Authors) // Nên hiện cả tác giả nữa
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // 2. XÓA PHẦN BOOK REVIEW (Vì chưa có bảng này trong CSDL)
            // ViewBag.BooksReview = ... (Đã xóa)

            // 3. Sửa lại phần Sách liên quan (Related Books)
            // Logic: Lấy sách cùng CategoryId, trừ cuốn hiện tại ra
            ViewBag.BooksRelated = await _context.Books
                .Where(b => b.BookId != id && b.CategoryId == book.CategoryId)
                .OrderByDescending(b => b.BookId) // Lấy sách mới nhất
                .Take(8) // Lấy 8 cuốn
                .ToListAsync();

            return View(book);
        }

    }
}
