using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<BlogController> _logger;
        private readonly IWebHostEnvironment _environment;

        public BlogController(QlthuvienContext context, ILogger<BlogController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // GET: Admin/Blog
        public async Task<IActionResult> Index(string searchString, string status)
        {
            var blogsQuery = _context.TbBlogs
                .Include(b => b.Author)
                .AsQueryable();

            // Tìm kiếm theo tiêu đề
            if (!string.IsNullOrEmpty(searchString))
            {
                blogsQuery = blogsQuery.Where(b => b.Title.Contains(searchString));
            }

            // Lọc theo trạng thái xuất bản
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Published")
                {
                    blogsQuery = blogsQuery.Where(b => b.IsPublished);
                }
                else if (status == "Draft")
                {
                    blogsQuery = blogsQuery.Where(b => !b.IsPublished);
                }
            }

            var blogs = await blogsQuery
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            ViewBag.SelectedStatus = status;

            return View(blogs);
        }

        // GET: Admin/Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var blog = await _context.TbBlogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.BlogId == id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        // GET: Admin/Blog/Create
        public IActionResult Create()
        {
            ViewBag.Authors = _context.Users.OrderBy(u => u.Username).ToList();
            return View();
        }

        // POST: Admin/Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TbBlog blog)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Lưu đường dẫn ảnh đã chọn từ wwwroot
                    // Image đã được set từ input khi người dùng chọn từ browser
                    // Không cần xử lý upload nữa

                    blog.CreatedDate = DateTime.Now;

                    _context.Add(blog);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Bài viết '{blog.Title}' đã được tạo thành công!";

                    // Chuyển hướng về trang nguồn
                    string referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer) && referer.Contains("/Details"))
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = blog.BlogId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo bài viết: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bài viết.");
                }
            }
            ViewBag.Authors = _context.Users.OrderBy(u => u.Username).ToList();
            return View(blog);
        }

        // GET: Admin/Blog/Edit/5
        public async Task<IActionResult> Edit(int? id, string fromDetails)
        {
            if (id == null) return NotFound();
            var blog = await _context.TbBlogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.BlogId == id);
            if (blog == null) return NotFound();

            ViewBag.Authors = _context.Users.OrderBy(u => u.Username).ToList();
            ViewBag.FromDetails = !string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true";
            return View(blog);
        }

        // POST: Admin/Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TbBlog blog, string fromDetails)
        {
            if (id != blog.BlogId) return NotFound();

            // Xóa lỗi validation của fromDetails (không phải là trường của model)
            ModelState.Remove("fromDetails");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBlog = await _context.TbBlogs.FindAsync(id);
                    if (existingBlog == null)
                    {
                        return NotFound();
                    }

                    // Lưu đường dẫn ảnh đã chọn từ wwwroot
                    // Image đã được set từ input khi người dùng chọn từ browser
                    // Không cần xử lý upload hay xóa ảnh cũ nữa vì ảnh đã có sẵn trong wwwroot

                    // Cập nhật các trường
                    existingBlog.Title = blog.Title;
                    existingBlog.Alias = blog.Alias;
                    existingBlog.Content = blog.Content;
                    existingBlog.AuthorId = blog.AuthorId;
                    existingBlog.Image = blog.Image; // Lưu đường dẫn ảnh đã chọn
                    existingBlog.IsPublished = blog.IsPublished;
                    existingBlog.ModifiedDate = DateTime.Now;

                    _context.Update(existingBlog);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Bài viết '{blog.Title}' đã được cập nhật thành công!";

                    // Chuyển hướng về trang nguồn
                    if (!string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true")
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = blog.BlogId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật bài viết: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật bài viết.");
                }
            }
            else
            {
                _logger.LogWarning($"ModelState không hợp lệ khi cập nhật bài viết ID {id}");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        _logger.LogWarning($"  - Field '{key}': {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                    }
                }
            }
            ViewBag.Authors = _context.Users.OrderBy(u => u.Username).ToList();
            return View(blog);
        }

        // GET: Admin/Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var blog = await _context.TbBlogs.Include(b => b.Author).FirstOrDefaultAsync(b => b.BlogId == id);
            if (blog == null) return NotFound();
            return View(blog);
        }

        // POST: Admin/Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.TbBlogs.FindAsync(id);
            if (blog == null)
            {
                TempData["Error"] = "Bài viết không tồn tại.";
                return RedirectToAction(nameof(Index), new { area = "Admin" });
            }

            _context.TbBlogs.Remove(blog);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Bài viết '{blog.Title}' đã được xóa thành công!";
            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }
    }
}
