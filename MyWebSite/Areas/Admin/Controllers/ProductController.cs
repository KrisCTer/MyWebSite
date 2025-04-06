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
        private readonly ApplicationDbContext _context;
        private readonly int _pageSize = 10;

        public ProductController(ApplicationDbContext context,IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _context = context;
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
            var product = await _context.Products
                .Include(p => p.ProductDetail)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
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

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl, List<IFormFile> imageUrls)
        {
            if (ModelState.IsValid)
            {
                // Trường hợp chỉ có một hình ảnh chính

                product.ImageUrl = await SaveImage(imageUrl);

                product.ProductDetail.Price = product.Price;
                if (imageUrls != null && imageUrls.Count > 0)
                {
                    var imageUrlList = new List<string>();
                    foreach (var image in imageUrls)
                    {
                        imageUrlList.Add(await SaveImage(image));
                    }
                    // Nếu chưa có hình ảnh chính, lấy hình đầu tiên từ danh sách
                    if (string.IsNullOrEmpty(product.ImageUrl))
                    {
                        product.ImageUrl = imageUrlList.FirstOrDefault();
                    }
                    // Nên lưu danh sách hình ảnh vào đâu đó trong product
                    // Ví dụ: product.AdditionalImages = imageUrlList;

                }
                // Thêm sản phẩm vào cơ sở dữ liệu
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, trả về form với dữ liệu đã nhập
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }
        // GET: Admin/Product/Edit/5
        // Admin/ProductController.cs
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.ProductDetail)  // Bao gồm ProductDetail nếu cần
                .Include(p => p.Category)       // Bao gồm Category nếu cần
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Load các danh mục để chọn
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(product);
        }


        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Product product)
        {
            if (id != product.ProductId)
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
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            await _productRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductExists(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
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
                    return RedirectToAction("Create", "Product", new { area = "Admin" });
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
            return RedirectToAction("Create", "Product", new { area = "Admin" });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStock(Guid id, int stockQuantity, int lowStockThreshold)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.StockQuantity = stockQuantity;
            product.LowStockThreshold = lowStockThreshold;

            await _productRepository.UpdateAsync(product);

            return RedirectToAction("Display", new { id = id });
        }

    }
}
