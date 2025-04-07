using Microsoft.AspNetCore.Mvc;
using MyWebSite.Models;

namespace MyWebSite.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OrderDetail(int orderId)
        {
            var order = _context.OrderDetails
                .Where(o => o.OrderId == orderId)
                .ToList();

            if (order == null || order.Count == 0)
            {
                return NotFound();
            }

            ViewBag.OrderId = orderId;
            return View(order);
        }
    }
}
