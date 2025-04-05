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
            var discountCodes = await _discountCodeRepository.GetAllAsync();
            return View(discountCodes);
        }
        // form code giam gia moi
        public IActionResult Create()
        {
            return View();
        }
        // them ma giam gia
        [HttpPost]
        public async Task<IActionResult> Create(DiscountCode discountCode)
        {
            discountCode.UsageCount = 0;
            discountCode.Status = true;

            if (!ModelState.IsValid)
            {
                return View(discountCode);
            }
            if (discountCode.ValidFrom >= discountCode.ValidTo)
            {
                ModelState.AddModelError("ValidFrom", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
                return View(discountCode);
            }

            await _discountCodeRepository.AddAsync(discountCode);
            return RedirectToAction(nameof(Index));
        }

        // xem chi tiet ma giam gia
        public async Task<IActionResult> Details(int id)
        {
            var discountCode = await _discountCodeRepository.GetByIdAsync(id);
            return discountCode == null ? NotFound() : View(discountCode);
        }

        // cap nhat ma giam gia
        public async Task<IActionResult> Edit(int id)
        {
            var discountCode = await _discountCodeRepository.GetByIdAsync(id);
            return discountCode == null ? NotFound() : View(discountCode);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, DiscountCode discountCode)
        {
            if (id != discountCode.Id)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View(discountCode);
            }
            if (discountCode.ValidFrom >= discountCode.ValidTo)
            {
                ModelState.AddModelError("ValidFrom", "Ngày bắt đầu phải nhỏ hơn ngày kết thúc");
                return View(discountCode);
            }
            await _discountCodeRepository.UpdateAsync(discountCode);
            return RedirectToAction(nameof(Index));
        }

        // xoa ma giam gia
        public async Task<IActionResult> Delete(int id)
        {
            var discountCode = await _discountCodeRepository.GetByIdAsync(id);
            return discountCode == null ? NotFound() : View(discountCode);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
