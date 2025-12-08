using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using QUANLYTHUVIEN.Utilities;
using System.Linq;

namespace QUANLYTHUVIEN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlthuvienContext _context;

        public HomeController(ILogger<HomeController> logger, QlthuvienContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Load categories và authors cho dropdown tìm kiếm
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();
            return View();
        }

        [Route("tim-kiem")]
        [Route("search")]
        public async Task<IActionResult> Search(string keywords, string catalog, string category, int page = 1)
        {
            // Lấy danh sách thể loại và tác giả cho dropdown
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();

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
            if (!string.IsNullOrEmpty(catalog) && catalog != "Tìm kiếm theo tác giả" && catalog != "Search the Catalog")
            {
                booksQuery = booksQuery.Where(b => b.Authors.Any(a => a.AuthorName == catalog));
            }

            // Lọc theo thể loại
            if (!string.IsNullOrEmpty(category) && category != "Tất cả thể loại" && category != "All Categories")
            {
                booksQuery = booksQuery.Where(b => b.Category != null && b.Category.CategoryName == category);
            }

            // Phân trang
            int pageSize = 12;
            var totalBooks = await booksQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var books = await booksQuery
                .OrderByDescending(b => b.BookId)
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

                book.Quantity = book.Quantity ?? 0;
                var availableQuantity = (book.Quantity ?? 0) - rentedCount;
                ViewBag.AvailableQuantities = ViewBag.AvailableQuantities ?? new Dictionary<int, int>();
                ((Dictionary<int, int>)ViewBag.AvailableQuantities)[book.BookId] = Math.Max(0, availableQuantity);
            }

            // ViewBag cho phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBooks = totalBooks;
            ViewBag.Keywords = keywords;
            ViewBag.Catalog = catalog;
            ViewBag.Category = category;

            return View(books);
        }

        [Route("ve-chung-toi")]
        [Route("about")]
        public async Task<IActionResult> About()
        {
            // Load thống kê cho trang About
            var totalBooks = await _context.Books.CountAsync();
            var totalAuthors = await _context.Authors.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalCustomers = await _context.Customers.CountAsync();

            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalAuthors = totalAuthors;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalCustomers = totalCustomers;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("lien-he")]
        [Route("contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
