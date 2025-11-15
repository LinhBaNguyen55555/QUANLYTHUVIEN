using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models; // Đảm bảo namespace này đúng
using System.Linq;
using System.Threading.Tasks;

namespace QUANLYTHUVIEN.ViewComponents
{
    // 1. Phải kế thừa từ ViewComponent
    public class CategoryFilterViewComponent : ViewComponent
    {
        private readonly QlthuvienContext _context;

        // 2. Phải có constructor để nhận DbContext (Dependency Injection)
        public CategoryFilterViewComponent(QlthuvienContext context)
        {
            _context = context;
        }

        // 3. Phải có phương thức InvokeAsync để chạy logic
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy tất cả danh mục từ CSDL
            var categories = await _context.Categories
                                           .AsNoTracking()
                                           .OrderBy(c => c.CategoryName) // Sắp xếp theo tên cho đẹp
                                           .ToListAsync();

            // Trả về View mặc định (sẽ là file Default.cshtml)
            // và truyền 'categories' làm model
            return View(categories);
        }
    }
}
