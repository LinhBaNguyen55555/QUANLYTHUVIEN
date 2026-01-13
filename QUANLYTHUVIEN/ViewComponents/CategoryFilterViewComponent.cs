using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models; // Đảm bảo namespace này đúng
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYTHUVIEN.ViewComponents
{
    
    public class CategoryFilterViewComponent : ViewComponent
    {
        private readonly QlthuvienContext _context;

        
        public CategoryFilterViewComponent(QlthuvienContext context)
        {
            _context = context;
        }

        
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories
                                           .AsNoTracking()
                                           .OrderBy(c => c.CategoryName) 
                                           .ToListAsync();
            return View(categories);
        }
    }
}
