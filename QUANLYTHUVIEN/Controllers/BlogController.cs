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

            // Lấy thông tin customer hiện tại để kiểm tra quyền xóa bình luận
            int? currentCustomerId = null;
            var userId = HttpContext.Session?.GetString("UserId");
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
                        currentCustomerId = customer.CustomerId;
                    }
                }
            }
            ViewBag.CurrentCustomerId = currentCustomerId;

            return View(blog);
        }

        // POST: Blog/SubmitComment - Gửi bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComment(int blogId, string commentText)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để bình luận." });
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

            // Tìm customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => 
                    (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                    (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
            }

            // Kiểm tra blog có tồn tại không
            var blog = await _context.TbBlogs.FindAsync(blogId);
            if (blog == null || !blog.IsPublished)
            {
                return Json(new { success = false, message = "Bài viết không tồn tại hoặc đã bị xóa." });
            }

            // Validation
            if (string.IsNullOrWhiteSpace(commentText))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung bình luận." });
            }

            if (commentText.Length > 1000)
            {
                return Json(new { success = false, message = "Bình luận không được vượt quá 1000 ký tự." });
            }

            // Tạo comment mới
            var comment = new TbBlogComment
            {
                BlogId = blogId,
                CustomerId = customer.CustomerId,
                CommentText = commentText.Trim(),
                CreatedDate = DateTime.Now,
                IsApproved = true // Tự động duyệt (có thể thay đổi thành false nếu cần admin duyệt)
            };

            try
            {
                _context.TbBlogComments.Add(comment);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Bình luận của bạn đã được gửi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // POST: Blog/DeleteComment - Xóa bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session?.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để xóa bình luận." });
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

            // Tìm customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => 
                    (!string.IsNullOrEmpty(user.Email) && c.Email == user.Email) ||
                    (!string.IsNullOrEmpty(user.Phone) && c.Phone == user.Phone));

            if (customer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin khách hàng." });
            }

            // Tìm comment
            var comment = await _context.TbBlogComments
                .Include(c => c.Blog)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);

            if (comment == null)
            {
                return Json(new { success = false, message = "Bình luận không tồn tại." });
            }

            // Kiểm tra quyền: chỉ người đã đăng bình luận mới có thể xóa
            if (comment.CustomerId != customer.CustomerId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa bình luận này." });
            }

            try
            {
                _context.TbBlogComments.Remove(comment);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Bình luận đã được xóa thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}

