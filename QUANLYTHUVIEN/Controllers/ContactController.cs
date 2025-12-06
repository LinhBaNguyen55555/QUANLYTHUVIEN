using Microsoft.AspNetCore.Mvc;
using QUANLYTHUVIEN.Data;
using QUANLYTHUVIEN.Models;

namespace QUANLYTHUVIEN.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendContact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                _context.Contacts.Add(contact);
                _context.SaveChanges();
                return Json(new { success = true, message = "Gửi liên hệ thành công!" });
            }

            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin!" });
        }
    }
}