using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;

namespace QUANLYTHUVIEN.ViewComponents
{
    public class BookViewComponent : ViewComponent
    {
        private readonly QlthuvienContext _context;

        public BookViewComponent(QlthuvienContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var books = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Publisher)
                .Include(b => b.Language)
                .Include(b => b.Authors)
                .OrderByDescending(b => b.BookId)
                .Take(8)
                .ToListAsync();

            return View(books);
        }
    }
}