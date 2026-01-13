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
            var recentPosts = await _context.TbBlogs
                                .Include(b => b.Author)
                                .Include(b => b.TbBlogComments) 
                                .OrderByDescending(b => b.CreatedDate)
                                .Take(6) 
                                .ToListAsync();

            return View(recentPosts);
        }
    }
}