using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Text.Json;

namespace QUANLYTHUVIEN.Controllers
{
    public class CartController : Controller
    {
        private readonly QlthuvienContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(QlthuvienContext context)
        {
            _context = context;
        }

        // Lấy danh sách giỏ hàng từ session
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        // Lưu giỏ hàng vào session
        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        // Thêm sách vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var book = await _context.Books
                .Include(b => b.Authors)
                .Include(b => b.RentalPrices)
                .FirstOrDefaultAsync(b => b.BookId == bookId);

            if (book == null)
            {
                return Json(new { success = false, message = "Sách không tồn tại" });
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.BookId == bookId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var currentPrice = book.RentalPrices.OrderByDescending(rp => rp.EffectiveDate).FirstOrDefault();
                var cartItem = new CartItem
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    AuthorNames = book.AuthorNames,
                    CoverImageUrl = book.CoverImageUrl,
                    DailyRate = currentPrice?.DailyRate ?? 0,
                    Quantity = 1
                };
                cart.Add(cartItem);
            }

            SaveCart(cart);

            var cartCount = cart.Sum(c => c.Quantity);
            return Json(new { success = true, cartCount = cartCount, message = "Đã thêm vào giỏ hàng" });
        }

        // Xem giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalAmount = cart.Sum(c => c.TotalPrice);
            return View(cart);
        }

        // Xóa sách khỏi giỏ hàng
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.BookId == bookId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }

            var cartCount = cart.Sum(c => c.Quantity);
            var totalAmount = cart.Sum(c => c.TotalPrice);
            return Json(new { success = true, cartCount = cartCount, totalAmount = totalAmount });
        }

        // Cập nhật số lượng
        [HttpPost]
        public IActionResult UpdateQuantity(int bookId, int quantity)
        {
            if (quantity <= 0)
            {
                return RemoveFromCart(bookId);
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.BookId == bookId);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }

            var cartCount = cart.Sum(c => c.Quantity);
            var totalAmount = cart.Sum(c => c.TotalPrice);
            var itemTotal = item?.TotalPrice ?? 0;
            return Json(new { success = true, cartCount = cartCount, totalAmount = totalAmount, itemTotal = itemTotal });
        }

        // Lấy số lượng giỏ hàng (dùng cho AJAX)
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            var cartCount = cart.Sum(c => c.Quantity);
            return Json(new { cartCount = cartCount });
        }

        // Lấy danh sách items trong giỏ hàng (dùng cho dropdown)
        [HttpGet]
        public IActionResult GetCartItems()
        {
            var cart = GetCart();
            var totalAmount = cart.Sum(c => c.TotalPrice);
            return Json(new { items = cart, totalAmount = totalAmount });
        }
    }
}

