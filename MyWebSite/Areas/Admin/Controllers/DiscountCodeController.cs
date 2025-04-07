using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Models;
using MyWebSite.Repositories;

namespace MyWebSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
            var discounts = await _discountCodeRepository.GetAllAsync();
            return View(discounts);
        }

        // POST: Admin/Discount/Create
        [HttpPost]
        public async Task<IActionResult> Create(DiscountCode discount)
        {
            if (ModelState.IsValid)
            {
                discount.UsageCount = 0;
                await _discountCodeRepository.AddAsync(discount);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Discount/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, DiscountCode discount)
        {
            if (id != discount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _discountCodeRepository.UpdateAsync(discount);
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Discount/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _discountCodeRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
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
