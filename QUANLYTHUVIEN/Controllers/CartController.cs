using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QUANLYTHUVIEN.Models;
using System.Text.Json;
using System.Linq;

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

       
        private List<CartItem> GetCart()
        {
            if (HttpContext.Session == null)
            {
                return new List<CartItem>();
            }
            
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson, options) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        
        private void SaveCart(List<CartItem> cart)
        {
            if (HttpContext.Session == null || cart == null)
            {
                return;
            }
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = false
                };
                var cartJson = JsonSerializer.Serialize(cart, options);
                HttpContext.Session.SetString(CartSessionKey, cartJson);
            }
            catch
            {
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            try
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
                    var currentPrice = book.RentalPrices?.OrderByDescending(rp => rp.EffectiveDate).FirstOrDefault();
                    var authorNames = book.Authors != null && book.Authors.Any() 
                        ? string.Join(", ", book.Authors.Select(a => a.AuthorName))
                        : "Updating...";
                    
                    var cartItem = new CartItem
                    {
                        BookId = book.BookId,
                        Title = book.Title ?? "Không có tiêu đề",
                        AuthorNames = authorNames,
                        CoverImageUrl = book.CoverImageUrl ?? "~/images/books-media/gird-view/book-media-grid-01.jpg",
                        DailyRate = currentPrice?.DailyRate ?? 0,
                        Quantity = 1
                    };
                    cart.Add(cartItem);
                }

                SaveCart(cart);

                var cartCount = cart.Sum(c => c.Quantity);
                return Json(new { success = true, cartCount = cartCount, message = "Đã thêm vào giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

       
        public IActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalAmount = cart?.Sum(c => c.TotalPrice) ?? 0;
            return View(cart ?? new List<CartItem>());
        }

     
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            try
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(c => c.BookId == bookId);
                if (item != null)
                {
                    cart.Remove(item);
                    SaveCart(cart);
                }

                var cartCount = cart?.Sum(c => c.Quantity) ?? 0;
                var totalAmount = cart?.Sum(c => c.TotalPrice) ?? 0;
                return Json(new { success = true, cartCount = cartCount, totalAmount = totalAmount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        
        [HttpPost]
        public IActionResult UpdateQuantity(int bookId, int quantity)
        {
            try
            {
               
                if (quantity < 1)
                {
                    quantity = 1;
                }

                var cart = GetCart();
                var item = cart?.FirstOrDefault(c => c.BookId == bookId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    SaveCart(cart);
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy sách trong giỏ hàng." });
                }

                var cartCount = cart?.Sum(c => c.Quantity) ?? 0;
                var totalAmount = cart?.Sum(c => c.TotalPrice) ?? 0;
                var itemTotal = item?.TotalPrice ?? 0;
                return Json(new { success = true, cartCount = cartCount, totalAmount = totalAmount, itemTotal = itemTotal });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCart();
            var cartCount = cart?.Sum(c => c.Quantity) ?? 0;
            return Json(new { cartCount = cartCount });
        }

       
        [HttpGet]
        public IActionResult GetCartItems()
        {
            try
            {
                var cart = GetCart();
                var cartJson = HttpContext.Session?.GetString(CartSessionKey);
                
                
                var debugInfo = new
                {
                    sessionExists = HttpContext.Session != null,
                    cartJsonExists = !string.IsNullOrEmpty(cartJson),
                    cartCount = cart?.Count ?? 0,
                    cartJsonLength = cartJson?.Length ?? 0
                };
                
                var totalAmount = cart?.Sum(c => c.TotalPrice) ?? 0;
                
                
                var items = new List<object>();
                if (cart != null && cart.Any())
                {
                    foreach (var c in cart)
                    {
                        items.Add(new
                        {
                            bookId = c.BookId,
                            title = c.Title ?? "Không có tiêu đề",
                            authorNames = c.AuthorNames ?? "Updating...",
                            coverImageUrl = c.CoverImageUrl ?? "~/images/books-media/gird-view/book-media-grid-01.jpg",
                            dailyRate = c.DailyRate,
                            quantity = c.Quantity,
                            totalPrice = c.TotalPrice
                        });
                    }
                }
                
                return Json(new { 
                    items = items, 
                    totalAmount = totalAmount,
                    debug = debugInfo
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    items = new List<object>(), 
                    totalAmount = 0, 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}

