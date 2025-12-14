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
            // Chỉ lấy menu cấp 1 (ParentId == null) và menu con của chúng
            var items = await _context.TbMenus
                .Where(m => m.IsActive == true && m.ParentId == null)  // Chỉ lấy menu cấp 1
                .Include(m => m.InverseParent.Where(c => c.IsActive == true).OrderBy(c => c.Position))  // Load menu con đang active
                .OrderBy(m => m.Position)
                .ToListAsync();
            return View(items);
        }
    }
}
