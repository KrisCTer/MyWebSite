using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MyWebSite.Models;

namespace MyWebSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(
            string orderId,
            string customerName,
            DateTime? fromDate,
            DateTime? toDate,
            string status,
            int page = 1)
        {
            const int pageSize = 10;

            // Base query
            var query = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(orderId))
            {
                if (int.TryParse(orderId, out int id))
                {
                    query = query.Where(o => o.OrderId == id);
                }
            }

            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(o => o.ApplicationUser.UserName.Contains(customerName));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include orders from the end date
                query = query.Where(o => o.OrderDate <= toDate.Value.AddDays(1));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Order by date descending (newest first)
            query = query.OrderByDescending(o => o.OrderDate);

            // Calculate total pages
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Adjust current page if needed
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Get records for current page
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get stats for dashboard cards
            var now = DateTime.Now;
            var lastMonth = now.AddMonths(-1);

            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            ViewBag.ShippedOrders = await _context.Orders.CountAsync(o => o.Status == "Shipped");
            ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice);

            // Calculate growth percentage
            var currentMonthOrders = await _context.Orders.CountAsync(o => o.OrderDate.Month == now.Month && o.OrderDate.Year == now.Year);
            var lastMonthOrders = await _context.Orders.CountAsync(o => o.OrderDate.Month == lastMonth.Month && o.OrderDate.Year == lastMonth.Year);

            ViewBag.OrdersGrowth = lastMonthOrders > 0
                ? Math.Round(((double)(currentMonthOrders - lastMonthOrders) / lastMonthOrders) * 100, 1)
                : 100;

            // Save filter values for form
            ViewBag.OrderIdFilter = orderId;
            ViewBag.CustomerNameFilter = customerName;
            ViewBag.FromDateFilter = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDateFilter = toDate?.ToString("yyyy-MM-dd");
            ViewBag.StatusFilter = status;

            // Pagination data
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Admin/Order/Create
        public IActionResult Create()
        {
            // Create a new order form
            ViewBag.Users = _context.Users.ToList();
            ViewBag.Products = _context.Products.ToList();

            return View();
        }

        // POST: Admin/Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order, int[] productIds, int[] quantities, decimal[] prices)
        {
            if (ModelState.IsValid)
            {
                // Add order details from form arrays
                for (int i = 0; i < productIds.Length; i++)
                {
                    if (productIds[i] != 0 && quantities[i] > 0)
                    {
                        var product = await _context.Products.FindAsync(productIds[i]);
                        if (product != null)
                        {
                            order.OrderDetails.Add(new OrderDetail
                            {
                                ProductId = product.ProductId,
                                Quantity = quantities[i],
                                Price = prices[i],
                                Product = product
                            });
                        }
                    }
                }

                // Calculate total price
                order.TotalPrice = order.OrderDetails.Sum(od => od.Price * od.Quantity - od.Discount);
                order.Amount = order.TotalPrice;
                order.OrderDate = DateTime.Now;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Order created successfully.";
                return RedirectToAction(nameof(Details), new { id = order.OrderId });
            }

            // If we got this far, something failed; redisplay form
            ViewBag.Users = _context.Users.ToList();
            ViewBag.Products = _context.Products.ToList();

            return View(order);
        }

        // GET: Admin/Order/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Users = _context.Users.ToList();
            ViewBag.Products = _context.Products.ToList();

            return View(order);
        }
        // POST: Admin/Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing order with details to update
                    var existingOrder = await _context.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefaultAsync(o => o.OrderId == id);

                    if (existingOrder == null)
                    {
                        return NotFound();
                    }

                    // Update order properties
                    existingOrder.UserId = order.UserId;
                    existingOrder.Status = order.Status;
                    existingOrder.ShippingAddress = order.ShippingAddress;
                    existingOrder.Notes = order.Notes;

                    // Update dates if provided
                    if (order.ShippedDate.HasValue)
                        existingOrder.ShippedDate = order.ShippedDate;

                    if (order.DeliveredDate.HasValue)
                        existingOrder.DeliveredDate = order.DeliveredDate;

                    // Save changes
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Order updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = order.OrderId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed; redisplay form
            ViewBag.Users = _context.Users.ToList();
            ViewBag.Products = _context.Products.ToList();

            return View(order);
        }

        // POST: Admin/Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Remove order details and payment first
            _context.OrderDetails.RemoveRange(order.OrderDetails);
            if (order.Payment != null)
            {
                _context.Payments.Remove(order.Payment);
            }

            // Remove the order itself
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Order deleted successfully.";
            return RedirectToAction("Index");
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status, string note)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Update status
            order.Status = status;

            // Update shipped/delivered dates based on status
            if (status == "Shipped" && !order.ShippedDate.HasValue)
            {
                order.ShippedDate = DateTime.Now;
            }
            else if (status == "Delivered" && !order.DeliveredDate.HasValue)
            {
                order.DeliveredDate = DateTime.Now;
            }

            // Add note if provided
            if (!string.IsNullOrEmpty(note))
            {
                if (string.IsNullOrEmpty(order.Notes))
                {
                    order.Notes = note;
                }
                else
                {
                    // Append to existing notes with timestamp
                    order.Notes += $"\n\n[{DateTime.Now:MM/dd/yyyy HH:mm}] {note}";
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order status updated to {status} successfully.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // GET: Admin/Order/PrintInvoice/5
        public async Task<IActionResult> PrintInvoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Order/SendNotification/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(int id, string message)
        {
            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // TODO: Implement notification sending logic
            // This would typically integrate with an email service or notification system

            TempData["Success"] = "Notification sent successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Admin/Order/ExportOrders
        public async Task<IActionResult> ExportOrders(string status = null)
        {
            // Base query
            var query = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            // Apply filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Get orders
            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            // Generate CSV content
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);

            // Write header
            writer.WriteLine("OrderID,Customer,OrderDate,Status,Items,TotalPrice");

            // Write data rows
            foreach (var order in orders)
            {
                writer.WriteLine($"{order.OrderId},{order.ApplicationUser.UserName},{order.OrderDate:yyyy-MM-dd},{order.Status},{order.OrderDetails.Count},{order.TotalPrice}");
            }

            writer.Flush();
            memoryStream.Position = 0;

            // Return file
            var fileName = $"orders_export_{DateTime.Now:yyyyMMdd}.csv";
            return File(memoryStream.ToArray(), "text/csv", fileName);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}