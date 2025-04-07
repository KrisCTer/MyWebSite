
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Extensions;
using MyWebSite.Models;
using MyWebSite.Repositories;
using static MyWebSite.Controllers.CheckoutController;

namespace MyWebSite.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository, IDiscountCodeRepositorycs discountCodeRepositorycs)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
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

        private async Task<Product> GetProductFromDatabase(Guid productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

        public IActionResult RemoveFromCart(Guid productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CheckOut(Order order, string voucherCode = null)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;

            // Tính tổng phụ (subtotal) của các mặt hàng trong giỏ hàng
            decimal subtotal = cart.Items.Sum(i => i.Price * i.Quantity);

            // Kiểm tra và áp dụng mã giảm giá nếu có
            if (!string.IsNullOrEmpty(voucherCode))
            {
                if (discountPercentage > 0)
                {
                    var discountAmount = subtotal * discountPercentage;
                    order.TotalPrice = subtotal - discountAmount;
                }
                else
                {
                    // Nếu mã giảm giá không hợp lệ, giữ nguyên tổng giá trị
                    order.TotalPrice = subtotal;
                }
            }
            else
            {
                // Nếu không có mã giảm giá, chỉ tính tổng phụ
                order.TotalPrice = subtotal;
            }

            order.Notes = order.Notes ?? "No notes";
            order.ShippingAddress = order.ShippingAddress ?? "No address";
            order.Status = "Pending";

            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

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