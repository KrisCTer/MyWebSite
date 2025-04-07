using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;
using MyWebSite.Services;

namespace MyWebSite.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IOrderService _orderService;

        private readonly ICartService _cartService;

        public CheckoutController(IOrderService orderService,

            ICartService cartService)
        {
            _orderService = orderService;

            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            // Get cart items and prepare checkout view model
            var cart = await _cartService.GetCurrentCartAsync();
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new Checkout
            {
                Subtotal = cart.Subtotal,
                Discount = cart.Discount,
                ShippingCost = 30000,
                
                Tax = CalculateTax(cart)
            };

            viewModel.Total = viewModel.Subtotal + 30000;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(Checkout model)
        {
            var cart = await _cartService.GetCurrentCartAsync();
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            // Lấy UserId hiện tại
            var userId = User.FindFirst("sub")?.Value;

            // Tạo đơn hàng
            var newOrder = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Subtotal = cart.Subtotal,
                TotalPrice = model.Total,
                ShippingAddress = model.Address,
                Notes = model.Notes,
                Status = "Pending"
            };

            // Lưu đơn hàng
            _orderService.AddOrder(newOrder);
            await _orderService.SaveChangesAsync(); // Lưu đơn hàng vào database

            // Lưu chi tiết đơn hàng
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    ProductId = item.ProductId,
                    //ProductName = item.Name,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _orderService.AddOrderDetail(orderDetail);
            }

            await _orderService.SaveChangesAsync(); // Lưu chi tiết đơn hàng

            // Xóa giỏ hàng sau khi đặt hàng
            await _cartService.ClearCartAsync();

            // Redirect đến trang chi tiết đơn hàng
            return RedirectToAction("OrderDetail", "Order", new { orderId = newOrder.OrderId });
        }

        public class PaymentResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        public async Task<IActionResult> Confirmation(string orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

        private decimal CalculateShippingCost(Cart cart)
        {

            return cart.Subtotal > 100 ? 0 : 10;
        }

        private decimal CalculateTax(Cart cart)
        {

            return Math.Round(cart.Subtotal * 0.1m, 2);
        }

        private string GetCurrentUserId()
        {

            return User.FindFirst("sub")?.Value;
        }

        private async Task<PaymentResult> ProcessPayment(Checkout model, Order order)
        {

            return await Task.FromResult(new PaymentResult { Success = true });
        }


        public class ApplyCouponRequest
        {
            public string CouponCode { get; set; }
        }


        

    }
}