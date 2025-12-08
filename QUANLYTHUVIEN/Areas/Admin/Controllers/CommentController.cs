using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CommentController : Controller
    {
        private readonly QlthuvienContext _context;

        public CommentController(QlthuvienContext context)
        {
            _context = context;
        }

        // Danh sách bình luận
        public IActionResult Index(string? search, bool? status)
        {
            var query = _context.TbBlogComments
                .Include(c => c.Blog)
                .Include(c => c.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(c =>
                    (c.CommentText != null && c.CommentText.ToLower().Contains(keyword)) ||
                    (c.Blog != null && c.Blog.Title.ToLower().Contains(keyword)) ||
                    (c.Customer != null && c.Customer.FullName != null && c.Customer.FullName.ToLower().Contains(keyword)));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.IsApproved == status.Value);
            }

            var comments = query
                .OrderByDescending(c => c.CreatedDate ?? DateTime.MinValue)
                .ToList();

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleApprove(int id)
        {
            var comment = _context.TbBlogComments.FirstOrDefault(c => c.CommentId == id);
            if (comment == null)
            {
                TempData["Error"] = "Không tìm thấy bình luận.";
                return RedirectToAction(nameof(Index));
            }

            comment.IsApproved = !comment.IsApproved;
            _context.SaveChanges();

            TempData["Success"] = comment.IsApproved ? "Đã duyệt bình luận." : "Đã chuyển bình luận về trạng thái chờ duyệt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var comment = _context.TbBlogComments.FirstOrDefault(c => c.CommentId == id);
            if (comment == null)
            {
                TempData["Error"] = "Không tìm thấy bình luận cần xóa.";
                return RedirectToAction(nameof(Index));
            }

            _context.TbBlogComments.Remove(comment);
            _context.SaveChanges();

            TempData["Success"] = "Đã xóa bình luận.";
            return RedirectToAction(nameof(Index));
        }
    }
}







