using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYTHUVIEN.ViewComponents
{
    public class LatestBlogViewComponent : ViewComponent
    {
        private readonly QlthuvienContext _context;

        public LatestBlogViewComponent(QlthuvienContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy 6 bài blog mới nhất
            // Chúng ta .Include() cả Author và Comments để hiển thị thông tin
            var recentPosts = await _context.TbBlogs
                                .Include(b => b.Author)
                                .Include(b => b.TbBlogComments) // Để đếm số lượng comment
                                .OrderByDescending(b => b.CreatedDate)
                                .Take(6) // Lấy 6 bài mới nhất
                                .ToListAsync();

            return View(recentPosts);
        }
    }
}