using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;

namespace MyWebSite.Repositories
{
    public class EFDiscountCodeRepository : IDiscountCodeRepositorycs
    {
        private readonly ApplicationDbContext _context;

        public EFDiscountCodeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Thêm mã giảm giá
        public async Task AddAsync(DiscountCode discountCode)
        {
            await _context.discountCodes.AddAsync(discountCode);
            await _context.SaveChangesAsync();
        }

        // Xóa mã giảm giá theo ID
        public async Task DeleteAsync(int id)
        {
            var discountCode = await _context.discountCodes.FindAsync(id);
            if (discountCode != null)
            {
                _context.discountCodes.Remove(discountCode);
                await _context.SaveChangesAsync();
            }
        }


        // Lấy danh sách tất cả mã giảm giá
        public async Task<IEnumerable<DiscountCode>> GetAllAsync()
        {
            return await _context.discountCodes.ToListAsync();
        }

        // Lấy mã giảm giá theo ID
        public async Task<DiscountCode> GetByIdAsync(int id)
        {
            return await _context.discountCodes.FindAsync(id);
        }

        public Task<int> GetDiscountPercentage(string voucherCode)
        {
            var discountCode = _context.discountCodes
                .FirstOrDefaultAsync(d => d.Code == voucherCode);
            if (discountCode == null)
                return Task.FromResult(0);
            return Task.FromResult(discountCode.Result.DiscountPercentage);
        }

        // Kiểm tra mã giảm giá đã hết hạn chưa
        public async Task<bool> IsCodeExpired(string code)
        {
            var discountCode = await _context.discountCodes
                .FirstOrDefaultAsync(d => d.Code == code);

            if (discountCode == null || discountCode.ValidTo == null)
                return false;

            return DateTime.UtcNow > discountCode.ValidTo;
        }

        // Kiểm tra mã giảm giá đã được sử dụng hết chưa
        public async Task<bool> IsCodeUsedUp(string code)
        {
            var discountCode = await _context.discountCodes
                .FirstOrDefaultAsync(d => d.Code == code);

            if (discountCode == null)
                return false;

            return discountCode.UsageCount >= discountCode.MaxUsage;
        }

        // Kiểm tra mã giảm giá hợp lệ
        public async Task<bool> IsValidCode(string code)
        {
            //var discountCode = await _context.discountCodes
            //    .FirstOrDefaultAsync(d => d.Code == code);

            //if (discountCode == null)
            //    return false;

            //return discountCode.Status &&
            //       discountCode.UsageCount < discountCode.MaxUsage &&
            //       (!discountCode.ValidTo|| DateTime.UtcNow <= discountCode.ValidTo);
            return true;
        }


        // Cập nhật mã giảm giá
        public async Task UpdateAsync(DiscountCode discountCode)
        {
            _context.discountCodes.Update(discountCode);
            await _context.SaveChangesAsync();
        }
    }
}
