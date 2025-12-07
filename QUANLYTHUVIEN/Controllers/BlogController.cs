using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using QUANLYTHUVIEN.Utilities;
using System.Globalization;

namespace QUANLYTHUVIEN.Controllers
{
    public class BlogController : Controller
    {
        private readonly QlthuvienContext _context;

        public BlogController(QlthuvienContext context)
        {
            _context = context;
        }

        [Route("bai-viet")]
        [Route("blog")]
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            // Load categories và authors cho dropdown tìm kiếm
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Authors = await _context.Authors
                .OrderBy(a => a.AuthorName)
                .ToListAsync();

            // Query bài viết - chỉ lấy bài đã xuất bản
            var blogsQuery = _context.TbBlogs
                .Include(b => b.Author)
                .Include(b => b.TbBlogComments)
                .Where(b => b.IsPublished == true)
                .AsQueryable();

            // Tìm kiếm theo tiêu đề hoặc nội dung
            if (!string.IsNullOrEmpty(searchString))
            {
                blogsQuery = blogsQuery.Where(b => 
                    b.Title.Contains(searchString) || 
                    (b.Content != null && b.Content.Contains(searchString)));
            }

            // Phân trang
            int pageSize = 9;
            var totalBlogs = await blogsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);

            var blogs = await blogsQuery
                .OrderByDescending(b => b.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ViewBag cho phân trang và search
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBlogs = totalBlogs;
            ViewBag.SearchString = searchString;
            ViewBag.StartItem = (page - 1) * pageSize + 1;
            ViewBag.EndItem = Math.Min(page * pageSize, totalBlogs);

            return View(blogs);
        }

        [Route("bai-viet/{alias}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TbBlogs == null)
            {
                return NotFound();
            }

            // Lấy thông tin chi tiết bài viết
            var blog = await _context.TbBlogs
                .Include(b => b.Author)
                .Include(b => b.TbBlogComments)
                    .ThenInclude(c => c.Customer)
                .FirstOrDefaultAsync(m => m.BlogId == id);

            if (blog == null || !blog.IsPublished)
            {
                return NotFound();
            }

            // Tăng lượt xem
            blog.Views = (blog.Views ?? 0) + 1;
            await _context.SaveChangesAsync();

            // Lấy các bài viết liên quan (mới nhất)
            ViewBag.RelatedBlogs = await _context.TbBlogs
                .Include(b => b.Author)
                .Include(b => b.TbBlogComments)
                .Where(b => b.BlogId != id && b.IsPublished == true)
                .OrderByDescending(b => b.CreatedDate)
                .Take(6)
                .ToListAsync();

            // Load categories và authors cho dropdown tìm kiếm
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();

            return View(blog);
        }
    }
}

