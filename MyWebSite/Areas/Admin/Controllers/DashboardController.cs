using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;

namespace MyWebSite.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public List<RecentOrder> RecentOrders { get; set; } = new();

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get current and previous month dates for comparison
            var today = DateTime.Today;
            var firstDayCurrentMonth = new DateTime(today.Year, today.Month, 1);
            var firstDayPreviousMonth = firstDayCurrentMonth.AddMonths(-1);

            // Create dashboard viewmodel
            var viewModel = new DashboardViewModel();

            // Get total orders and calculate growth
            var totalOrders = await _context.Orders.CountAsync();
            var currentMonthOrders = await _context.Orders
                .CountAsync(o => o.OrderDate >= firstDayCurrentMonth);
            var previousMonthOrders = await _context.Orders
                .CountAsync(o => o.OrderDate >= firstDayPreviousMonth && o.OrderDate < firstDayCurrentMonth);

            viewModel.TotalOrders = totalOrders;
            viewModel.OrdersGrowth = previousMonthOrders > 0
                ? Math.Round(((decimal)currentMonthOrders / previousMonthOrders - 1) * 100, 1)
                : 100;

            // Get total revenue and calculate growth
            var totalRevenue = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .SumAsync(o => o.Amount);
            var currentMonthRevenue = await _context.Orders
                .Where(o => o.OrderDate >= firstDayCurrentMonth && o.Status != "Cancelled")
                .SumAsync(o => o.Amount);
            var previousMonthRevenue = await _context.Orders
                .Where(o => o.OrderDate >= firstDayPreviousMonth && o.OrderDate < firstDayCurrentMonth && o.Status != "Cancelled")
                .SumAsync(o => o.Amount);

            viewModel.TotalRevenue = totalRevenue;
            viewModel.RevenueGrowth = previousMonthRevenue > 0
                ? Math.Round((currentMonthRevenue / previousMonthRevenue - 1) * 100, 1)
                : 100;

            // Get total customers and growth
            var totalCustomers = await _context.Users.CountAsync();  // Accessing AspNetUsers table
            var newCustomers = await _context.Users
                .CountAsync(u => u.LockoutEnd >= firstDayCurrentMonth);  // Assuming you have a Created field or custom registration date field

            viewModel.TotalCustomers = totalCustomers;
            viewModel.CustomersGrowth = (totalCustomers > 0) ? Math.Round((decimal)newCustomers / totalCustomers * 100, 1) : 0;

            // Get product stats
            viewModel.TotalProducts = await _context.Products.CountAsync();
            viewModel.LowStockProducts = await _context.Products
                .CountAsync(p => p.ProductDetail.StockCount <= p.LowStockThreshold);

            // Get recent orders
            viewModel.RecentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrder
                {
                    OrderId = o.OrderId,
                    CustomerName = o.ApplicationUser.FullName, // Hoặc Email nếu chưa có FullName
                    OrderDate = o.OrderDate,
                    Amount = o.TotalPrice,
                    Status = o.Status
                })
                .ToListAsync();




            // Get top products
            viewModel.TopProducts = await _context.OrderDetails
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.Price })
                .Select(g => new Product
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    Price = g.Key.Price,
                    SalesCount = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(p => p.SalesCount)
                .Take(5)
                .ToListAsync();

            // Get recent activities
            viewModel.RecentActivities = await _context.Activities
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .Select(a => new Activities
                {
                    ActivityId = a.ActivityId,
                    Type = a.Type,
                    Description = a.Description,
                    Timestamp = a.Timestamp,
                    UserId = a.UserId,
                }).ToListAsync();

            return View(viewModel);  // Pass view model to Razor view
        }
    }
}
