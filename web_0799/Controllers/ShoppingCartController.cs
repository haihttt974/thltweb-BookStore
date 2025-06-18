using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using web_0799.Models;
using web_0799.ViewModels;

namespace web_0799.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ProductDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ProductDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.GetUserAsync(User);

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == user.Id)
                .ToListAsync();

            var cart = new ShoppingCart
            {
                Items = cartItems.Select(ci => new CartItem
                {
                    ProductId = ci.ProductId,
                    Name = ci.Product.Name,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity
                }).ToList()
            };

            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                var returnUrl = Url.Action("AddToCart", "ShoppingCart", new { productId, quantity });
                return Redirect($"~/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }

            var user = await _userManager.GetUserAsync(User);
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItemDb
                {
                    UserId = user.Id,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm {product.Name} vào giỏ hàng.";
            return RedirectToAction("Index", "Home");
        }

        // Cập nhật số lượng sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateCart(int productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            var item = await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.UserId == user.Id && i.ProductId == productId);

            if (item == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(item);
                TempData["Success"] = $"Đã xóa {item.Product.Name} khỏi giỏ hàng.";
            }
            else
            {
                item.Quantity = quantity;
                TempData["Success"] = $"Đã cập nhật số lượng cho {item.Product.Name}.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Xóa một sản phẩm khỏi giỏ hàng
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            var item = await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.UserId == user.Id && i.ProductId == productId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                TempData["Success"] = $"Đã xóa {item.Product.Name} khỏi giỏ hàng.";
            }
            else
            {
                TempData["Success"] = "Sản phẩm không tồn tại trong giỏ hàng.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Xóa toàn bộ giỏ hàng
        public async Task<IActionResult> ClearCart()
        {
            var user = await _userManager.GetUserAsync(User);
            var items = _context.CartItems.Where(i => i.UserId == user.Id);

            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa toàn bộ sản phẩm khỏi giỏ hàng.";

            return RedirectToAction("Index");
        }

        // Trang thanh toán (GET)
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(i => i.Product)
                .Where(i => i.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index");
            }

            var cart = new ShoppingCart
            {
                Items = cartItems.Select(i => new CartItem
                {
                    ProductId = i.ProductId,
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return View(new CheckoutViewModel
            {
                Cart = cart,
                Order = new Order()
            });
        }

        // Xử lý đặt hàng (POST)
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            var cartItems = await _context.CartItems
                .Include(i => i.Product)
                .Where(i => i.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index");
            }

            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cartItems.Sum(i => i.Product.Price * i.Quantity);
            order.OrderDetails = cartItems.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Product.Price
            }).ToList();

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return View("OrderCompleted", order.Id);
        }
    }
}
