using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(QlthuvienContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Customer
        public async Task<IActionResult> Index(string searchString)
        {
            var customersQuery = _context.Customers.AsQueryable();

            
            if (!string.IsNullOrEmpty(searchString))
            {
                customersQuery = customersQuery.Where(c =>
                    c.FullName.Contains(searchString) ||
                    (c.Email != null && c.Email.Contains(searchString)) ||
                    (c.Phone != null && c.Phone.Contains(searchString)));
            }

            var customers = await customersQuery
                .OrderBy(c => c.CustomerId)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            return View(customers);
        }

        // GET: Admin/Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Rentals)
                .Include(c => c.TbBlogComments)
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Admin/Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (!string.IsNullOrEmpty(customer.Email) &&
                        await _context.Customers.AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower()))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        return View(customer);
                    }

                    customer.CreatedAt = DateTime.Now;

                    _context.Add(customer);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Khách hàng '{customer.FullName}' đã được tạo thành công bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Khách hàng '{customer.FullName}' đã được tạo thành công!";

                    
                    string referer = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(referer) && referer.Contains("/Details"))
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = customer.CustomerId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo khách hàng: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo khách hàng: {ex.Message}. Vui lòng thử lại.");
                }
            }

            return View(customer);
        }

        // GET: Admin/Customer/Edit/5
        public async Task<IActionResult> Edit(int? id, string fromDetails)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.FromDetails = !string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true";
            return View(customer);
        }

        // POST: Admin/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer, string fromDetails)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            
            ModelState.Remove("fromDetails");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    
                    if (!string.IsNullOrEmpty(customer.Email) &&
                        await _context.Customers.AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower() && c.CustomerId != id))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        return View(customer);
                    }

                    
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Address = customer.Address;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Khách hàng '{customer.FullName}' (ID: {customer.CustomerId}) đã được cập nhật bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Khách hàng '{customer.FullName}' đã được cập nhật thành công!";

                    
                    if (!string.IsNullOrEmpty(fromDetails) && fromDetails.ToLower() == "true")
                    {
                        return RedirectToAction(nameof(Details), new { area = "Admin", id = customer.CustomerId });
                    }
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật khách hàng ID {id}: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật khách hàng: {ex.Message}. Vui lòng thử lại.");
                }
            }

            return View(customer);
        }

        // GET: Admin/Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Rentals)
                .Include(c => c.TbBlogComments)
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Admin/Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Rentals)
                    .Include(c => c.TbBlogComments)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customer == null)
                {
                    _logger.LogWarning($"Không tìm thấy khách hàng để xóa: ID {id}");
                    TempData["Error"] = "Khách hàng không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }

                
                if (customer.Rentals.Any() || customer.TbBlogComments.Any())
                {
                    _logger.LogWarning($"Không thể xóa khách hàng '{customer.FullName}' vì có dữ liệu liên quan");
                    TempData["Error"] = $"Không thể xóa khách hàng '{customer.FullName}' vì có dữ liệu liên quan (phiếu thuê, bình luận).";
                    return RedirectToAction(nameof(Index), new { area = "Admin" });
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Khách hàng '{customer.FullName}' (ID: {customer.CustomerId}) đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Khách hàng '{customer.FullName}' đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa khách hàng ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa khách hàng. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index), new { area = "Admin" });
        }
    }
}


