using QUANLYTHUVIEN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QUANLYTHUVIEN.ViewComponents
{
    public class MenuTopViewComponent : ViewComponent
    {
        private readonly QlthuvienContext _context;

        public MenuTopViewComponent(QlthuvienContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _context.TbMenus
                .Where(m => m.IsActive == true)
                .Include(m => m.InverseParent.OrderBy(c => c.Position))  // Load menu con
                .OrderBy(m => m.Position)
                .ToListAsync();
            return View(items);
        }
    }
}
