using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PublisherController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<PublisherController> _logger;

        public PublisherController(QlthuvienContext context, ILogger<PublisherController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Publisher
        public async Task<IActionResult> Index(string searchString)
        {
            var publishersQuery = _context.Publishers
                .Include(p => p.Books)
                .AsQueryable();

            
            if (!string.IsNullOrEmpty(searchString))
            {
                publishersQuery = publishersQuery.Where(p => p.PublisherName.Contains(searchString));
            }

            var publishers = await publishersQuery
                .OrderBy(p => p.PublisherName)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            return View(publishers);
        }

        // GET: Admin/Publisher/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                    .ThenInclude(b => b.Authors)
                .Include(p => p.Books)
                    .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(p => p.PublisherId == id);
            if (publisher == null) return NotFound();
            return View(publisher);
        }

        // GET: Admin/Publisher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Publisher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Publishers.AnyAsync(p => p.PublisherName.ToLower() == publisher.PublisherName.ToLower()))
                    {
                        ModelState.AddModelError("PublisherName", "Tên nhà xuất bản đã tồn tại.");
                        return View(publisher);
                    }

                    _context.Add(publisher);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Nhà xuất bản '{publisher.PublisherName}' đã được tạo thành công!";

                    
                    string referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer) && referer.Contains("/Details"))
                    {
                        return RedirectToAction("Details", "Publisher", new { area = "Admin", id = publisher.PublisherId });
                    }
                    return RedirectToAction("Index", "Publisher", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo nhà xuất bản: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo nhà xuất bản.");
                }
            }
            return View(publisher);
        }

        // GET: Admin/Publisher/Edit/5
        public async Task<IActionResult> Edit(int? id, string fromDetails)
        {
            if (id == null) return NotFound();
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null) return NotFound();

            ViewBag.FromDetails = !string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true";
            return View(publisher);
        }

        // POST: Admin/Publisher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publisher publisher, string fromDetails)
        {
            if (id != publisher.PublisherId) return NotFound();

            
            ModelState.Remove("fromDetails");

            if (ModelState.IsValid)
            {
                try
                {
                    if (await _context.Publishers.AnyAsync(p => p.PublisherName.ToLower() == publisher.PublisherName.ToLower() && p.PublisherId != id))
                    {
                        ModelState.AddModelError("PublisherName", "Tên nhà xuất bản đã tồn tại.");
                        return View(publisher);
                    }

                    _context.Update(publisher);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Nhà xuất bản '{publisher.PublisherName}' đã được cập nhật thành công!";

                    
                    if (!string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true")
                    {
                        return RedirectToAction("Details", "Publisher", new { area = "Admin", id = publisher.PublisherId });
                    }
                    return RedirectToAction("Index", "Publisher", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật nhà xuất bản: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật nhà xuất bản.");
                }
            }
            return View(publisher);
        }

        // GET: Admin/Publisher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var publisher = await _context.Publishers.Include(p => p.Books).FirstOrDefaultAsync(p => p.PublisherId == id);
            if (publisher == null) return NotFound();
            return View(publisher);
        }

        // POST: Admin/Publisher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisher = await _context.Publishers.Include(p => p.Books).FirstOrDefaultAsync(p => p.PublisherId == id);
            if (publisher == null)
            {
                TempData["Error"] = "Nhà xuất bản không tồn tại.";
                return RedirectToAction("Index", "Publisher", new { area = "Admin" });
            }

            if (publisher.Books.Any())
            {
                TempData["Error"] = $"Không thể xóa nhà xuất bản '{publisher.PublisherName}' vì có {publisher.Books.Count} sách đang sử dụng.";
                return RedirectToAction("Index", "Publisher", new { area = "Admin" });
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Nhà xuất bản '{publisher.PublisherName}' đã được xóa thành công!";
            return RedirectToAction("Index", "Publisher", new { area = "Admin" });
        }
    }
}
