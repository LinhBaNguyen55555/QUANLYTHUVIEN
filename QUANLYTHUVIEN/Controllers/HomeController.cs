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
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();
            return View();
        }

        [Route("tim-kiem")]
        [Route("search")]
        public async Task<IActionResult> Search(string keywords, string catalog, string category, int page = 1)
        {
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();

 
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

           
            if (!string.IsNullOrEmpty(catalog) && catalog != "Tìm kiếm theo tác giả" && catalog != "Search the Catalog")
            {
                booksQuery = booksQuery.Where(b => b.Authors.Any(a => a.AuthorName == catalog));
            }

            
            if (!string.IsNullOrEmpty(category) && category != "Tất cả thể loại" && category != "All Categories")
            {
                booksQuery = booksQuery.Where(b => b.Category != null && b.Category.CategoryName == category);
            }

           
            int pageSize = 12;
            var totalBooks = await booksQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var books = await booksQuery
                .OrderByDescending(b => b.BookId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            
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

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("lien-he")]
        [Route("contact")]
        public async Task<IActionResult> Contact(string firstName, string lastName, string email, string phone, string message)
        {
            try
            {
                
                var first_name = Request.Form["first-name"].ToString();
                var last_name = Request.Form["last-name"].ToString();
                var emailValue = Request.Form["email"].ToString();
                var phoneValue = Request.Form["phone"].ToString();
                var messageValue = Request.Form["message"].ToString();

                
                if (string.IsNullOrWhiteSpace(first_name) && string.IsNullOrWhiteSpace(last_name))
                {
                    TempData["Error"] = "Vui lòng nhập họ hoặc tên.";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(emailValue))
                {
                    TempData["Error"] = "Vui lòng nhập email.";
                    return View();
                }

                if (string.IsNullOrWhiteSpace(messageValue))
                {
                    TempData["Error"] = "Vui lòng nhập nội dung tin nhắn.";
                    return View();
                }

                
                if (!emailValue.Contains("@") || !emailValue.Contains("."))
                {
                    TempData["Error"] = "Email không hợp lệ.";
                    return View();
                }

                
                var fullName = $"{first_name} {last_name}".Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    fullName = emailValue.Split('@')[0]; 
                }

               
                var contact = new Contact
                {
                    FullName = fullName,
                    Email = emailValue.Trim(),
                    Phone = string.IsNullOrWhiteSpace(phoneValue) ? null : phoneValue.Trim(),
                    Content = messageValue.Trim(),
                    CreatedDate = DateTime.Now
                };

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                
                await NotificationHelper.CreateNotificationForAdminsAsync(
                    _context,
                    "Tin nhắn liên hệ mới",
                    $"Bạn có tin nhắn liên hệ mới từ {fullName} ({emailValue}). Nội dung: {messageValue.Trim().Substring(0, Math.Min(100, messageValue.Trim().Length))}...",
                    "info"
                );

                TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi xảy ra khi gửi tin nhắn: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Contact Error: {ex.Message}\n{ex.StackTrace}");
                return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
