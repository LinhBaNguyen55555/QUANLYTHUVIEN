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
            
            var authorsWithBio = await _context.Authors
                                        .Where(a => !string.IsNullOrEmpty(a.Biography))
                                        .OrderBy(a => Guid.NewGuid())
                                        .Take(8) 
                                        .AsNoTracking()
                                        .ToListAsync();

            return View(authorsWithBio);
        }
    }
}