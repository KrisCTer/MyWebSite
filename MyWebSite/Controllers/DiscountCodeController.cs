using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Models;
using MyWebSite.Repositories;

namespace MyWebSite.Controllers
{
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee + "," + SD.Role_Company)]
    public class DiscountCodeController : Controller
    {
        private readonly IDiscountCodeRepositorycs _discountCodeRepository;
        public DiscountCodeController(IDiscountCodeRepositorycs discountCodeRepository)
        {
            _discountCodeRepository = discountCodeRepository;
        }

        // hien thi danh sach ma giam gia
        public async Task<IActionResult> Index()
        {
            var discountCodes = await _discountCodeRepository.GetAllAsync();
            return View(discountCodes);
        }
    
        public async Task<IActionResult> Details(int id)
        {
            var discountCode = await _discountCodeRepository.GetByIdAsync(id);
            return discountCode == null ? NotFound() : View(discountCode);
        }
        // kiem tra han su dung ma giam gia
        public async Task<IActionResult> IsExpried(string code)
        {
            bool isExpried = await _discountCodeRepository.IsCodeExpired(code);
            return Json(new { isExpried });
        }
        // kiem tra da dung het luot su dung chua
        public async Task<IActionResult> IsUsedUp(string code)
        {
            bool isUsedUp = await _discountCodeRepository.IsCodeUsedUp(code);
            return Json(new { isUsedUp });
        }
        // kiem tra hop le
        public async Task<IActionResult> IsValidCode(string code)
        {
            bool isValid = await _discountCodeRepository.IsValidCode(code);
            return Json(new { isValid });
        }
    }
}
