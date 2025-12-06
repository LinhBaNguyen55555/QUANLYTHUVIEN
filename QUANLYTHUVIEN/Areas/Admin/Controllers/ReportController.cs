using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<ReportController> _logger;

        public ReportController(QlthuvienContext context, ILogger<ReportController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Report/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboardData = new DashboardViewModel();

            // Thống kê tổng quan
            dashboardData.TotalBooks = await _context.Books.CountAsync();
            dashboardData.TotalCustomers = await _context.Customers.CountAsync();
            dashboardData.TotalRentals = await _context.Rentals.CountAsync();
            dashboardData.TotalOrders = await _context.Orders.CountAsync();
            dashboardData.TotalSuppliers = await _context.Suppliers.CountAsync();
            dashboardData.TotalUsers = await _context.Users.CountAsync();

            // Thống kê sách đang được thuê
            dashboardData.ActiveRentals = await _context.Rentals.CountAsync(r => r.Status == "Active");

            // Thống kê doanh thu tháng này
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            dashboardData.MonthlyRevenue = await _context.Rentals
                .Where(r => r.RentalDate.Value.Month == currentMonth && r.RentalDate.Value.Year == currentYear)
                .SumAsync(r => r.TotalAmount ?? 0);

            // Thống kê doanh thu năm nay
            dashboardData.YearlyRevenue = await _context.Rentals
                .Where(r => r.RentalDate.Value.Year == currentYear)
                .SumAsync(r => r.TotalAmount ?? 0);

            // Thống kê sách theo thể loại
            dashboardData.BooksByCategory = await _context.Categories
                .Select(c => new CategoryStat
                {
                    CategoryName = c.CategoryName,
                    BookCount = c.Books.Count
                })
                .OrderByDescending(c => c.BookCount)
                .Take(10)
                .ToListAsync();

            // Thống kê sách được thuê nhiều nhất
            dashboardData.TopRentedBooks = await _context.Books
                .Select(b => new BookRentalStat
                {
                    BookTitle = b.Title,
                    AuthorName = b.AuthorNames,
                    RentalCount = b.RentalDetails.Sum(rd => rd.Quantity ?? 0),
                    TotalRevenue = b.RentalDetails.Sum(rd => (rd.Quantity ?? 0) * (rd.PricePerDay ?? 0))
                })
                .OrderByDescending(b => b.RentalCount)
                .Take(10)
                .ToListAsync();

            // Thống kê khách hàng hoạt động nhất
            dashboardData.TopCustomers = await _context.Customers
                .Select(c => new CustomerStat
                {
                    CustomerName = c.FullName,
                    RentalCount = c.Rentals.Count,
                    TotalSpent = c.Rentals.Sum(r => r.TotalAmount ?? 0)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToListAsync();

            // Thống kê doanh thu theo tháng (6 tháng gần nhất)
            dashboardData.MonthlyRevenueChart = await GetMonthlyRevenueDataInternal();

            // Thống kê trạng thái phiếu thuê
            dashboardData.RentalStatusChart = await GetRentalStatusDataInternal();

            return View(dashboardData);
        }

        // GET: Admin/Report/Revenue
        public async Task<IActionResult> Revenue(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;

            var revenueData = new RevenueReportViewModel
            {
                SelectedYear = selectedYear,
                MonthlyRevenue = await GetMonthlyRevenueByYear(selectedYear),
                TotalRevenue = await _context.Rentals
                    .Where(r => r.RentalDate.Value.Year == selectedYear)
                    .SumAsync(r => r.TotalAmount ?? 0)
            };

            return View(revenueData);
        }

        // GET: Admin/Report/Books
        public async Task<IActionResult> Books()
        {
            var booksReport = new BooksReportViewModel
            {
                BooksByCategory = await _context.Categories
                    .Select(c => new CategoryStat
                    {
                        CategoryName = c.CategoryName,
                        BookCount = c.Books.Count
                    })
                    .OrderByDescending(c => c.BookCount)
                    .ToListAsync(),

                BooksByLanguage = await _context.Languages
                    .Select(l => new LanguageStat
                    {
                        LanguageName = l.LanguageName,
                        BookCount = l.Books.Count
                    })
                    .OrderByDescending(l => l.BookCount)
                    .ToListAsync(),

                BooksByPublisher = await _context.Publishers
                    .Select(p => new PublisherStat
                    {
                        PublisherName = p.PublisherName,
                        BookCount = p.Books.Count
                    })
                    .OrderByDescending(p => p.BookCount)
                    .Take(20)
                    .ToListAsync(),

                TopRentedBooks = await _context.Books
                    .Select(b => new BookRentalStat
                    {
                        BookTitle = b.Title,
                        AuthorName = b.AuthorNames,
                        RentalCount = b.RentalDetails.Sum(rd => rd.Quantity ?? 0),
                        TotalRevenue = b.RentalDetails.Sum(rd => (rd.Quantity ?? 0) * (rd.PricePerDay ?? 0))
                    })
                    .OrderByDescending(b => b.RentalCount)
                    .Take(20)
                    .ToListAsync()
            };

            return View(booksReport);
        }

        // API endpoints cho AJAX
        [HttpGet]
        public async Task<IActionResult> GetMonthlyRevenueData()
        {
            return Json(await GetMonthlyRevenueDataInternal());
        }

        [HttpGet]
        public async Task<IActionResult> GetRentalStatusData()
        {
            return Json(await GetRentalStatusDataInternal());
        }

        private async Task<List<MonthlyRevenueData>> GetMonthlyRevenueDataInternal()
        {
            var currentYear = DateTime.Now.Year;
            var monthlyData = new List<MonthlyRevenueData>();

            for (int month = 1; month <= 12; month++)
            {
                var revenue = await _context.Rentals
                    .Where(r => r.RentalDate.Value.Month == month && r.RentalDate.Value.Year == currentYear)
                    .SumAsync(r => r.TotalAmount ?? 0);

                monthlyData.Add(new MonthlyRevenueData
                {
                    Month = month,
                    MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
                    Revenue = revenue
                });
            }

            return monthlyData;
        }

        private async Task<List<RentalStatusData>> GetRentalStatusDataInternal()
        {
            var statusData = await _context.Rentals
                .GroupBy(r => r.Status)
                .Select(g => new RentalStatusData
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count()
                })
                .ToListAsync();

            return statusData;
        }

        private async Task<List<MonthlyRevenueData>> GetMonthlyRevenueByYear(int year)
        {
            var monthlyData = new List<MonthlyRevenueData>();

            for (int month = 1; month <= 12; month++)
            {
                var revenue = await _context.Rentals
                    .Where(r => r.RentalDate.Value.Month == month && r.RentalDate.Value.Year == year)
                    .SumAsync(r => r.TotalAmount ?? 0);

                monthlyData.Add(new MonthlyRevenueData
                {
                    Month = month,
                    MonthName = new DateTime(year, month, 1).ToString("MMM yyyy"),
                    Revenue = revenue
                });
            }

            return monthlyData;
        }
    }

    // ViewModels
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalRentals { get; set; }
        public int TotalOrders { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveRentals { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }

        public List<CategoryStat> BooksByCategory { get; set; } = new();
        public List<BookRentalStat> TopRentedBooks { get; set; } = new();
        public List<CustomerStat> TopCustomers { get; set; } = new();
        public List<MonthlyRevenueData> MonthlyRevenueChart { get; set; } = new();
        public List<RentalStatusData> RentalStatusChart { get; set; } = new();
    }

    public class CategoryStat
    {
        public string CategoryName { get; set; } = "";
        public int BookCount { get; set; }
    }

    public class BookRentalStat
    {
        public string BookTitle { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public int RentalCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CustomerStat
    {
        public string CustomerName { get; set; } = "";
        public int RentalCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class MonthlyRevenueData
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = "";
        public decimal Revenue { get; set; }
    }

    public class RentalStatusData
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

    public class RevenueReportViewModel
    {
        public int SelectedYear { get; set; }
        public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
        public decimal TotalRevenue { get; set; }
    }

    public class BooksReportViewModel
    {
        public List<CategoryStat> BooksByCategory { get; set; } = new();
        public List<LanguageStat> BooksByLanguage { get; set; } = new();
        public List<PublisherStat> BooksByPublisher { get; set; } = new();
        public List<BookRentalStat> TopRentedBooks { get; set; } = new();
    }

    public class LanguageStat
    {
        public string LanguageName { get; set; } = "";
        public int BookCount { get; set; }
    }

    public class PublisherStat
    {
        public string PublisherName { get; set; } = "";
        public int BookCount { get; set; }
    }
}
