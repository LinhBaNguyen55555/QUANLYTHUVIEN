using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace QUANLYTHUVIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly QlthuvienContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(QlthuvienContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index(string searchString, int? supplierId)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .AsQueryable();

            // Tìm kiếm theo nhà cung cấp
            if (!string.IsNullOrEmpty(searchString))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Supplier.SupplierName.Contains(searchString) ||
                    (o.User != null && o.User.FullName.Contains(searchString)));
            }

            // Lọc theo nhà cung cấp
            if (supplierId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.SupplierId == supplierId.Value);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Dữ liệu cho dropdown filters
            ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.SupplierName).ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedSupplierId = supplierId;

            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Order/Create
        public IActionResult Create()
        {
            ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View();
        }

        // POST: Admin/Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] selectedBooks, int[] quantities, decimal[] unitPrices)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    order.OrderDate = DateTime.Now;

                    // Tính tổng tiền và tạo chi tiết đơn hàng
                    decimal totalAmount = 0;
                    for (int i = 0; i < selectedBooks.Length; i++)
                    {
                        var bookId = selectedBooks[i];
                        var quantity = quantities[i];
                        var unitPrice = unitPrices[i];

                        var orderDetail = new OrderDetail
                        {
                            BookId = bookId,
                            Quantity = quantity,
                            UnitPrice = unitPrice
                        };
                        order.OrderDetails.Add(orderDetail);
                        totalAmount += unitPrice * quantity;
                    }

                    order.TotalAmount = totalAmount;

                    _context.Add(order);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Đơn hàng mới đã được tạo với nhà cung cấp '{order.Supplier?.SupplierName}' bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Đơn hàng đã được tạo thành công với nhà cung cấp '{order.Supplier?.SupplierName}'!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi tạo đơn hàng: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo đơn hàng: {ex.Message}. Vui lòng thử lại.");
                }
            }

            ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View(order);
        }

        // GET: Admin/Order/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View(order);
        }

        // POST: Admin/Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingOrder = await _context.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật thông tin cơ bản
                    existingOrder.SupplierId = order.SupplierId;
                    existingOrder.UserId = order.UserId;

                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Đơn hàng ID {id} đã được cập nhật bởi {User.Identity?.Name ?? "Admin"}");
                    TempData["Success"] = $"Đơn hàng đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi cập nhật đơn hàng ID {id}: {ex.Message}");
                    ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật đơn hàng: {ex.Message}. Vui lòng thử lại.");
                }
            }

            ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.SupplierName).ToList();
            ViewBag.Users = _context.Users.OrderBy(u => u.FullName).ToList();
            ViewBag.Books = _context.Books.Where(b => b.Quantity > 0).OrderBy(b => b.Title).ToList();
            return View(order);
        }

        // GET: Admin/Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Supplier)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    _logger.LogWarning($"Không tìm thấy đơn hàng để xóa: ID {id}");
                    TempData["Error"] = "Đơn hàng không tồn tại hoặc đã bị xóa.";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa chi tiết đơn hàng trước
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đơn hàng ID {id} đã được xóa bởi {User.Identity?.Name ?? "Admin"}");
                TempData["Success"] = $"Đơn hàng đã được xóa thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa đơn hàng ID {id}: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa đơn hàng. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

