using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SupplierController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(QlthuvienContext context, ILogger<SupplierController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Supplier
        public async Task<IActionResult> Index(string searchString)
        {
            var suppliersQuery = _context.Suppliers.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                suppliersQuery = suppliersQuery.Where(s =>
                    s.SupplierName.Contains(searchString) ||
                    (s.Email != null && s.Email.Contains(searchString)) ||
                    (s.Phone != null && s.Phone.Contains(searchString)));
            }

            var suppliers = await suppliersQuery
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            ViewBag.SearchString = searchString;
            return View(suppliers);
        }

        // GET: Admin/Supplier/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(m => m.SupplierId == id);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // GET: Admin/Supplier/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên nhà cung cấp trùng
                    if (await _context.Suppliers.AnyAsync(s => s.SupplierName.ToLower() == supplier.SupplierName.ToLower()))
                    {
                        ModelState.AddModelError("SupplierName", "Tên nhà cung cấp đã tồn tại.");
                        return View(supplier);
                    }

                    // Kiểm tra email trùng
                    if (!string.IsNullOrEmpty(supplier.Email) &&
                        await _context.Suppliers.AnyAsync(s => s.Email.ToLower() == supplier.Email.ToLower()))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        return View(supplier);
                    }

                    _context.Add(supplier);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Nhà cung cấp '{supplier.SupplierName}' đã được tạo thành công bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Nhà cung cấp '{supplier.SupplierName}' đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo nhà cung cấp: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo nhà cung cấp: {ex.Message}. Vui lòng thử lại.");
                }
            }

            return View(supplier);
        }

        // GET: Admin/Supplier/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Admin/Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.SupplierId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSupplier = await _context.Suppliers.FindAsync(id);
                    if (existingSupplier == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra tên nhà cung cấp trùng (trừ chính nó)
                    if (await _context.Suppliers.AnyAsync(s => s.SupplierName.ToLower() == supplier.SupplierName.ToLower() && s.SupplierId != id))
                    {
                        ModelState.AddModelError("SupplierName", "Tên nhà cung cấp đã tồn tại.");
                        return View(supplier);
                    }

                    // Kiểm tra email trùng (trừ chính nó)
                    if (!string.IsNullOrEmpty(supplier.Email) &&
                        await _context.Suppliers.AnyAsync(s => s.Email.ToLower() == supplier.Email.ToLower() && s.SupplierId != id))
                    {
                        ModelState.AddModelError("Email", "Email đã tồn tại.");
                        return View(supplier);
                    }

                    // Cập nhật thông tin
                    existingSupplier.SupplierName = supplier.SupplierName;
                    existingSupplier.Address = supplier.Address;
                    existingSupplier.Phone = supplier.Phone;
                    existingSupplier.Email = supplier.Email;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Nhà cung cấp '{supplier.SupplierName}' (ID: {supplier.SupplierId}) đã được cập nhật bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Nhà cung cấp '{supplier.SupplierName}' đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật nhà cung cấp ID {id}: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật nhà cung cấp: {ex.Message}. Vui lòng thử lại.");
                }
            }

            return View(supplier);
        }

        // GET: Admin/Supplier/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(m => m.SupplierId == id);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Admin/Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .Include(s => s.Orders)
                    .FirstOrDefaultAsync(s => s.SupplierId == id);

                if (supplier == null)
                {
                    _logger.LogWarning($"Không tìm thấy nhà cung cấp để xóa: ID {id}");
                    TempData["Error"] = "Nhà cung cấp không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra xem nhà cung cấp có đang được sử dụng không
                if (supplier.Orders.Any())
                {
                    _logger.LogWarning($"Không thể xóa nhà cung cấp '{supplier.SupplierName}' vì có đơn hàng liên quan");
                    TempData["Error"] = $"Không thể xóa nhà cung cấp '{supplier.SupplierName}' vì có {supplier.Orders.Count} đơn hàng liên quan.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Nhà cung cấp '{supplier.SupplierName}' (ID: {supplier.SupplierId}) đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Nhà cung cấp '{supplier.SupplierName}' đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa nhà cung cấp ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa nhà cung cấp. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}


