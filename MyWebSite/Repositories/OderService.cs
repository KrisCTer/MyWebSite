using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;
using MyWebSite.Services;

namespace MyWebSite.Repositories
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddOrder(Order order)
        {
            _context.Orders.Add(order);
            await SaveChangesAsync();
        }

        public async Task AddOrderDetail(OrderDetail orderDetail)
        {
            _context.OrderDetails.Add(orderDetail);
            await SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderByIdAsync(string orderId)
        {

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId.ToString() == orderId);
            return order;
        }
        public Task<Order> CreateOrderAsync(Order order, List<CartItem> cartItems)
        {

            // Tạo đơn hàng mới
            var newOrder = new Order
            {
                UserId = order.UserId,
                OrderDate = DateTime.Now,
                Subtotal = cartItems.Sum(item => item.Price * item.Quantity),
                TotalPrice = order.TotalPrice,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,
                Status = "Pending"
            };
            // Lưu đơn hàng vào database
            _context.Orders.Add(newOrder);
            _context.SaveChanges();
            // Lưu chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = newOrder.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }
            _context.SaveChanges();
            return Task.FromResult(newOrder);
        }
    }

}
