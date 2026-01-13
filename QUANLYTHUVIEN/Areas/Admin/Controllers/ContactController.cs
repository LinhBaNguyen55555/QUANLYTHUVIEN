using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<ContactController> _logger;

        public ContactController(QlthuvienContext context, ILogger<ContactController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Contact
        public async Task<IActionResult> Index(string searchString)
        {
            var contactsQuery = _context.Contacts.AsQueryable();

            
            if (!string.IsNullOrEmpty(searchString))
            {
                contactsQuery = contactsQuery.Where(c =>
                    c.FullName.Contains(searchString) ||
                    (c.Email != null && c.Email.Contains(searchString)) ||
                    (c.Phone != null && c.Phone.Contains(searchString)) ||
                    (c.Content != null && c.Content.Contains(searchString)));
            }

            var contacts = await contactsQuery
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            return View(contacts);
        }

        // GET: Admin/Contact/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactID == id);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Admin/Contact/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactID == id);

            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Admin/Contact/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var contact = await _context.Contacts.FindAsync(id);

                if (contact == null)
                {
                    _logger.LogWarning($"Không tìm thấy tin nhắn liên hệ để xóa: ID {id}");
                    TempData["Error"] = "Tin nhắn liên hệ không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }

                var contactName = contact.FullName;
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Tin nhắn liên hệ từ '{contactName}' (ID: {contact.ContactID}) đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Tin nhắn liên hệ từ '{contactName}' đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa tin nhắn liên hệ ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa tin nhắn liên hệ. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }
    }
}


