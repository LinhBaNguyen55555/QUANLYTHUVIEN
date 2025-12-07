using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;

namespace QUANLYTHUVIEN.Controllers
{
    public class AuthorController : Controller
    {
        private readonly QlthuvienContext _context;

        public AuthorController(QlthuvienContext context)
        {
            _context = context;
        }

        [Route("tac-gia")]
        [Route("authors")]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            // Load categories và authors cho dropdown tìm kiếm
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Authors = await _context.Authors
                .OrderBy(a => a.AuthorName)
                .ToListAsync();

            // Query tác giả
            var authorsQuery = _context.Authors
                .Include(a => a.Books)
                    .ThenInclude(b => b.Category)
                .AsQueryable();

            // Tìm kiếm theo tên tác giả
            if (!string.IsNullOrEmpty(searchString))
            {
                authorsQuery = authorsQuery.Where(a => a.AuthorName.Contains(searchString));
            }

            // Phân trang
            int pageSize = 12;
            var totalAuthors = await authorsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalAuthors / (double)pageSize);

            var authors = await authorsQuery
                .OrderBy(a => a.AuthorName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewBag cho phân trang và search
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalAuthors = totalAuthors;
            ViewBag.SearchString = searchString;
            ViewBag.StartItem = (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalAuthors);

            return View(authors);
        }

        [Route("tac-gia/{alias}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Authors == null)
            {
                return NotFound();
            }

            // Lấy thông tin chi tiết tác giả
            var author = await _context.Authors
                .Include(a => a.Books)
                    .ThenInclude(b => b.Category)
                .Include(a => a.Books)
                    .ThenInclude(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            // Lấy các tác giả khác (để hiển thị liên quan)
            ViewBag.RelatedAuthors = await _context.Authors
                .Include(a => a.Books)
                .Where(a => a.AuthorId != id && a.Books.Any())
                .OrderByDescending(a => a.Books.Count)
                .Take(6)
                .ToListAsync();

            // Load categories và authors cho dropdown tìm kiếm
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();

            return View(author);
        }
    }
}






