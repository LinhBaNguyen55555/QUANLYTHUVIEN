using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYTHUVIEN.ViewComponents
{
    public class AuthorTestimonialViewComponent : ViewComponent
    {
        private readonly QlthuvienContext _context;

        public AuthorTestimonialViewComponent(QlthuvienContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy các tác giả CÓ tiểu sử (!IsNullOrEmpty)
            // Lấy ngẫu nhiên 6 người
            var authorsWithBio = await _context.Authors
                                        .Where(a => !string.IsNullOrEmpty(a.Biography))
                                        .OrderBy(a => Guid.NewGuid()) // Lấy ngẫu nhiên
                                        .Take(8) // Lấy 8 người
                                        .AsNoTracking()
                                        .ToListAsync();

            return View(authorsWithBio); // Trả về file Default.cshtml
        }
    }
}