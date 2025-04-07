using Microsoft.AspNetCore.Mvc;
using MyWebSite.Models;

namespace MyWebSite.Repositories
{
    public interface IDiscountCodeRepositorycs
    {
        Task<IEnumerable<DiscountCode>> GetAllAsync();
        Task<DiscountCode> GetByIdAsync(int id);
        Task<DiscountCode> GetByCodeAsync(string code);
        Task AddAsync(DiscountCode discountCode);
        Task UpdateAsync(DiscountCode discountCode);
        Task DeleteAsync(int id);
        Task<int> GetDiscountPercentage(string code);  // Add this method to fetch the discount percentage.
        Task ApplyVoucher(string code);
        Task<bool> IsValidCode(string code);
        Task<bool> IsCodeExpired(string code);
        Task<bool> IsCodeUsedUp(string code);
        Task<int> GetDiscountPercentage(string voucherCode);
    }
}
