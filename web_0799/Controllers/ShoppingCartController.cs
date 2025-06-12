using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using web_0799.Extensions;
using web_0799.Models;
using web_0799.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using web_0799.ViewModels;

namespace web_0799.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ProductDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ProductDBContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Làm sạch TempData khi vào trang để tránh hiển thị thông báo cũ
            TempData.Remove("Message");
            TempData.Remove("MessageType");

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            // Log giỏ hàng để kiểm tra sản phẩm "Lặng"
            Console.WriteLine("Cart contents:");
            foreach (var item in cart.Items)
            {
                Console.WriteLine($"ProductId: {item.ProductId}, Name: {item.Name}, Quantity: {item.Quantity}");
            }
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                TempData["MessageType"] = "Error";
                return RedirectToAction("Index");
            }

            Console.WriteLine($"Updating product: {item.Name}, ProductId: {productId}, Quantity: {quantity}");

            if (quantity <= 0)
            {
                cart.RemoveItem(productId);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                TempData["Message"] = $"Đã xóa {item.Name} khỏi giỏ hàng.";
                TempData["MessageType"] = "Success";
            }
            else
            {
                item.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                TempData["Message"] = $"Đã cập nhật số lượng cho {item.Name}.";
                TempData["MessageType"] = "Success";
            }

            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    Console.WriteLine($"Removing product: {item.Name}, ProductId: {productId}");
                    cart.RemoveItem(productId);
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                    TempData["Message"] = $"Đã xóa {item.Name} khỏi giỏ hàng.";
                    TempData["MessageType"] = "Success";
                }
                else
                {
                    TempData["Message"] = "Sản phẩm không tồn tại trong giỏ hàng.";
                    TempData["MessageType"] = "Error";
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");

            TempData["Message"] = "Đã xóa toàn bộ sản phẩm khỏi giỏ hàng.";
            TempData["MessageType"] = "Success";

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Message"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                TempData["MessageType"] = "Error";
                return RedirectToAction("Index");
            }

            var model = new CheckoutViewModel
            {
                Order = new Order(),
                Cart = cart
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Message"] = "Giỏ hàng trống.";
                TempData["MessageType"] = "Error";
                return RedirectToAction("Index");
            }
            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");
            return View("OrderCompleted", order.Id);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Kiểm tra trạng thái đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                var returnUrl = Url.Action("AddToCart", "ShoppingCart", new { productId, quantity });
                var loginUrl = Url.Content($"~/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                return Redirect(loginUrl);
            }

            var product = await GetProductFromDatabase(productId);
            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại.";
                TempData["MessageType"] = "Error";
                return RedirectToAction("Index");
            }

            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["Message"] = $"Đã thêm {product.Name} vào giỏ hàng.";
            TempData["MessageType"] = "Success";
            return RedirectToAction("Index", "Home");
        }

        private async Task<Product> GetProductFromDatabase(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }
    }
}