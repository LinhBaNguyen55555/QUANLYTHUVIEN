using QUANLYTHUVIEN.Models;
using Microsoft.AspNetCore.Mvc;

namespace Harmic.ViewComponents
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
            var items = _context.TbMenus.Where(m => (bool)m.IsActive)
                .OrderBy(m => m.Position).ToList();
            return await Task.FromResult<IViewComponentResult>(View(items));
        }
    }
}
