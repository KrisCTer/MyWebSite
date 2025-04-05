using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;

namespace MyWebSite.Repositories
{
    public class EFLoyalCustomerRepository : ILoyalCustomerRepository
    {
        private readonly ApplicationDbContext _context;
        public EFLoyalCustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task CheckAndAddLoyalCustomerAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || user.Orders == null) return;

            // Tính tổng đơn hàng đã hoàn thành
            var total = user.Orders
                .Where(o => o.Status == "Delivered")
                .Sum(o => o.Amount);

            if (total >= 10000000)
            {
                // Kiểm tra xem đã là khách thân thiết chưa
                var exists = await _context.LoyalCustomers
                    .AnyAsync(lc => lc.UserId == userId);

                if (!exists)
                {
                    var loyal = new LoyalCustomer
                    {
                        UserId = userId,
                        JoinedDate = DateTime.Now,
                        RewardPoints = 0,
                    };
                    _context.LoyalCustomers.Add(loyal);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
