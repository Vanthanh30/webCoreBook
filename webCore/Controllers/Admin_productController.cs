using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using webCore.MongoHelper;
using System.Linq;
using CloudinaryDotNet;

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class Admin_productController : Controller
    {
        private readonly CategoryProduct_adminService _CategoryProductCollection;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<Admin_productController> _logger;

        public Admin_productController(CategoryProduct_adminService Category_adminService, CloudinaryService cloudinaryService, ILogger<Admin_productController> logger)
        {
            _CategoryProductCollection = Category_adminService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, string filter = "all")
        {
            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;
                var productName = HttpContext.Session.GetString("ProductName");
                ViewBag.ProductName = productName;

                const int pageSize = 5;

                var products = await _CategoryProductCollection.GetProduct();
                var categories = await _CategoryProductCollection.GetCategory();

                var categoryDictionary = new Dictionary<string, string>();
                foreach (var cat in categories)
                {
                    if (!categoryDictionary.ContainsKey(cat.Id))
                    {
                        categoryDictionary.Add(cat.Id, cat.Title);
                    }
                }

                foreach (var product in products)
                {
                    if (!string.IsNullOrEmpty(product.CategoryId) && categoryDictionary.ContainsKey(product.CategoryId))
                    {
                        product.CategoryTitle = categoryDictionary[product.CategoryId];
                    }
                }
                var validStatuses = new[] { "Hoạt động", "Chờ duyệt", "Không duyệt" };
                var filteredProducts = products
                    .Where(p => !string.IsNullOrEmpty(p.Status) &&
                                validStatuses.Contains(p.Status.Trim(), StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (filter == "active")
                {
                    filteredProducts = products.Where(p =>
                        p.Status != null &&
                        p.Status.Equals("Hoạt động", StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }
                else if (filter == "inactive")
                {
                    filteredProducts = products
                        .Where(p => p.Status != null &&
                                    p.Status.Equals("Không duyệt", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
                else if (filter == "pending")
                {
                    filteredProducts = products
                        .Where(p => p.Status != null &&
                                    p.Status.Equals("Chờ duyệt", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }


                var sortedProducts = filteredProducts.OrderBy(c => c.Position).ToList();

                var totalProducts = sortedProducts.Count;
                var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                var productsToDisplay = sortedProducts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentFilter = filter;

                return View(productsToDisplay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products from MongoDB.");
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách sản phẩm. " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;
                var productName = HttpContext.Session.GetString("ProductName");
                ViewBag.ProductName = productName;

                var product = await _CategoryProductCollection.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                    return RedirectToAction("Index");
                }

                if (!string.IsNullOrEmpty(product.CategoryId))
                {
                    var category = await _CategoryProductCollection.GetCategoryByIdAsync(product.CategoryId);
                    if (category != null)
                    {
                        product.CategoryTitle = category.Title;
                    }
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product details.");
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết sản phẩm: " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID sản phẩm không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var product = await _CategoryProductCollection.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }

                product.Status = "Hoạt động";
                product.UpdatedAt = DateTime.UtcNow;

                await _CategoryProductCollection.UpdateProductAsync(product);

                TempData["SuccessMessage"] = $"Đã duyệt sản phẩm '{product.Title}' thành công!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving product.");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi duyệt sản phẩm.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID sản phẩm không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var product = await _CategoryProductCollection.GetProductByIdAsync(id);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }

                product.Status = "Không duyệt";
                product.UpdatedAt = DateTime.UtcNow;

                await _CategoryProductCollection.UpdateProductAsync(product);

                TempData["SuccessMessage"] = $"Đã từ chối sản phẩm '{product.Title}'!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting product.");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi từ chối sản phẩm.";
            }

            return RedirectToAction(nameof(Index));
        }

        private List<Category_admin> GetHierarchicalCategories(List<Category_admin> categories, string parentId = null, int level = 0)
        {
            var result = new List<Category_admin>();

            foreach (var category in categories.Where(c => c.ParentId == parentId))
            {
                category.Title = new string('-', level * 2) + " " + category.Title;
                result.Add(category);
                result.AddRange(GetHierarchicalCategories(categories, category.Id, level + 1));
            }

            return result;
        }
    }
}