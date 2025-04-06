using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
        private readonly IDiscountCodeRepositorycs _discountCodeRepository;
        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
            // Già sứ bạn có phương thức lấy thông tin sản phẩm từ productId
            var product = await GetProductFromDatabase(productId);

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

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
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
            order.Notes = order.Notes ?? "No notes";
            order.ShippingAddress = order.ShippingAddress ?? "No address";
            order.Status = "Pending"; // Trạng thái chưa xác nhận

            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            // 👉 Sau khi lưu đơn hàng, kiểm tra nếu user đã có tổng đơn > 10 triệu (đã xác nhận)
            var totalConfirmedAmount = await _context.Orders
                .Where(o => o.UserId == user.Id && o.Status == "Pending")
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            if (totalConfirmedAmount > 10)
            {
                var isLoyal = await _context.LoyalCustomers.AnyAsync(lc => lc.UserId == user.Id);
                if (!isLoyal)
                {
                    _context.LoyalCustomers.Add(new LoyalCustomer
                    {
                        UserId = user.Id,
                        JoinedDate = DateTime.Now,
                        RewardPoints = 0
                    });
                    await _context.SaveChangesAsync();
                }
            }

            return View("OrderCompleted", order.OrderId);
        }
        [HttpPost]
        public async Task<IActionResult> ApplyDiscountCode(string voucherCode)
        {
            var isValidCode = await _discountCodeRepository.IsValidCode(voucherCode);
            if (!isValidCode)
            {
                return Json(new { success = false, message = "Invalid voucher code." });
            }

            // Assuming GetDiscountPercentage returns the discount percentage (e.g., 10% = 0.10)
            var discountPercentage = await _discountCodeRepository.GetDiscountPercentage(voucherCode);
            return Json(new { success = true, discountPercentage });
        }
    }
}