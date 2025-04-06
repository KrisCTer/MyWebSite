using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyWebSite.Extensions;
using MyWebSite.Models;
using MyWebSite.Repositories;

namespace MyWebSite.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CartSessionKey = "ShoppingCart";

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        public IActionResult UpdateQuantity(Guid productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();

            var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cart.Items.Remove(cartItem);
                }

                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            }

            return Json(new
            {
                success = true,
                totalItems = cart.TotalItems,
                totalAmount = cart.TotalAmount,
                itemSubtotal = cartItem?.Subtotal ?? 0
            });
        }

        // ADD: An item to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity = 1)
        {
            var product = await GetProductFromDatabase(productId); // Lấy sản phẩm từ cơ sở dữ liệu
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();

            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return Json(new { success = true, totalItems = cart.TotalItems });
        }


        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>(CartSessionKey) ?? new ShoppingCart();
            return View(cart);
        }
        // Các actions khác...
        private async Task<Product> GetProductFromDatabase(Guid productId)
        {
            // Truy vấn có số dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

        public IActionResult RemoveFromCart(Guid productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);
                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CheckOut(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.Amount = order.TotalPrice; // Make sure Amount is also set since it's required
            order.Notes = order.Notes ?? "No notes";
            order.ShippingAddress = order.ShippingAddress ?? "No address";
            order.Status = "Pending";

            // Create the order first
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Now create order details with the generated OrderId
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId, // Use the generated OrderId
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price * item.Quantity
                };
                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");
            return View("CheckOut", order);
        }
        // Hiển thị form Checkout
        [HttpGet]
        public IActionResult CheckOut()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var order = new Order();
            
            order.Amount = cart.Items.Sum(i => i.Price * i.Quantity); // để binding vào view
            return View(order); // view Checkout.cshtml
        }

    }
}