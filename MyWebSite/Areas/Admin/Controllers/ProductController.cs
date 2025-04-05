using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;
using MyWebSite.Repositories;
using System.Threading.Tasks;

namespace MyWebSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly int _pageSize = 10;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index(string searchName, int? categoryId, string stockStatus,
                                              string sortOrder, int page = 1)
        {
            ViewData["CurrentFilter"] = searchName;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CategoryId = categoryId;
            ViewBag.StockStatus = stockStatus;
            ViewBag.CurrentPage = page;

            // Load all categories for the filter dropdown
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            ViewBag.Categories = categories;

            // Start with all products
            var products = await _productRepository.GetAllAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.Name.Contains(searchName)).ToList();
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId).ToList();
            }

            if (!string.IsNullOrEmpty(stockStatus))
            {
                switch (stockStatus.ToLower())
                {
                    case "instock":
                        products = products.Where(p => p.StockQuantity > p.LowStockThreshold).ToList();
                        break;
                    case "lowstock":
                        products = products.Where(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0).ToList();
                        break;
                    case "outofstock":
                        products = products.Where(p => p.StockQuantity == 0).ToList();
                        break;
                }
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name).ToList();
                    break;
                case "price":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                case "date":
                    products = products.OrderBy(p => p.CreatedAt).ToList();
                    break;
                case "date_desc":
                    products = products.OrderByDescending(p => p.CreatedAt).ToList();
                    break;
                default: // name ascending
                    products = products.OrderBy(p => p.Name).ToList();
                    break;
            }

            // Calculate pagination
            int totalItems = products.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            ViewBag.TotalPages = totalPages;

            // Apply pagination
            products = products.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

            return View(products);
        }

        // GET: Admin/Product/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id.ToString());

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Product/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.Id = Guid.NewGuid();
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;

                await _productRepository.AddAsync(product);

                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id.ToString());

            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productRepository.UpdateAsync(product);
                    TempData["SuccessMessage"] = "Product updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    if (!await ProductExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id.ToString());

            if (product == null)
            {
                return NotFound();
            }

            await _productRepository.DeleteAsync(id.ToString());
            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductExists(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id.ToString());
            return product != null;
        }
        // Trong AdminController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Sử dụng repository để thêm category
                    await _categoryRepository.AddAsync(category);

                    // Thông báo thành công
                    TempData["CategorySuccess"] = $"Category \"{category.Name}\" added successfully!";

                    // Redirect về trang Admin/Product/Add
                    return RedirectToAction("Add", "Product", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi
                    TempData["CategoryError"] = "Failed to add category. Please try again.";
                    // Log lỗi nếu cần
                    // _logger.LogError(ex, "Error creating category");
                }
            }
            else
            {
                TempData["CategoryError"] = "Invalid category data.";
            }

            // Trở về trang Admin/Product/Add nếu có lỗi
            return RedirectToAction("Add", "Product", new { area = "Admin" });
        }
    }
}
