using MyWebSite.Models;


namespace MyWebSite.Services
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIdAsync(string orderId);
        Task<Order> CreateOrderAsync(Order order, List<CartItem> cartItems);
        Task AddOrder(Order order);
        Task AddOrderDetail(OrderDetail orderDetail);
        Task SaveChangesAsync(); // Nếu bạn sử dụng Entity Framework hoặc lưu thông qua DbContext
       
    }

   
    public interface ICartService
    {
        Task<Cart> GetCurrentCartAsync();
        Task ClearCartAsync();
    }
}
