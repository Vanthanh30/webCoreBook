using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class HomeController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly UserService _userService;

        public HomeController(MongoDBService mongoDBService, ProductService productService, CategoryService categoryService, UserService userService)
        {
            _mongoDBService = mongoDBService;
            _productService = productService;
            _categoryService = categoryService;
            _userService = userService;
        }

        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> Index()
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;
            ViewBag.IsLoggedIn = isLoggedIn;

            var categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            var groupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            ViewBag.FeaturedProducts = featuredProducts;

            var bestsellerProducts = await _productService.GetBestsellerProductsAsync();
            ViewBag.BestsellerProducts = bestsellerProducts;

            return View();
        }

        public async Task<IActionResult> GetProductsByCategoryId(List<string> categoryId)
        {
            if (categoryId == null || !categoryId.Any())
            {
                var homeGroupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
                var orderedHomeProducts = homeGroupedProducts
                    .Where(g => g.Key == "Nổi bật" || g.Key == "Mới" || g.Key == "Gợi ý" || g.Key == "Bán chạy")
                    .OrderBy(g => g.Key == "Nổi bật" ? 0 : g.Key == "Mới" ? 1 : g.Key == "Gợi ý" ? 2 : 3)
                    .ToList();
                return PartialView("_BookListPartial", orderedHomeProducts);
            }

            var allCategories = await _categoryService.GetCategoriesAsync();
                
            var finalCategoryIds = new HashSet<string>();
            foreach (var id in categoryId)
            {
                var childIds = allCategories
                    .Where(c => c.ParentId == id)
                    .Select(c => c._id)
                    .ToList();
                if (childIds.Any())
                {
                    foreach (var childId in childIds)
                    {
                        finalCategoryIds.Add(childId);
                    }
                }
                else
                {
                    finalCategoryIds.Add(id);
                }
            }

            var products = new List<Product_admin>();
            foreach (var catId in finalCategoryIds)
            {
                var catProducts = await _productService.GetProductsByCategoryIdAsync(catId);
                products.AddRange(catProducts);
            }

            products = products.GroupBy(p => p.Id).Select(g => g.First()).ToList();

            var groupedProducts = ClassifyProductsByFeatured(products);

            var orderedGroupedProducts = groupedProducts
                .OrderBy(group => group.Key == "Nổi bật" ? 0 : group.Key == "Mới" ? 1 : group.Key == "Gợi ý" ? 2 : int.MaxValue)
                .ToList();

            return PartialView("_BookListPartial", orderedGroupedProducts);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            var categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            var groupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            ViewBag.FeaturedProducts = featuredProducts;

            var bestsellerProducts = await _productService.GetBestsellerProductsAsync();
            ViewBag.BestsellerProducts = bestsellerProducts;

            ViewBag.SearchQuery = q?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchResults = await _productService.SearchProductsAsync(q);

                var grouped = ClassifyProductsByFeatured(searchResults);

                ViewBag.SearchResults = grouped;
                ViewBag.IsSearching = true;
                ViewBag.ResultCount = searchResults.Count;
            }
            else
            {
                ViewBag.IsSearching = false;
            }

            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Sign_in(User loginUser)
        {
            if (!ModelState.IsValid)
            {
                return View(loginUser);
            }

            var user = await _userService.GetAccountByEmailAsync(loginUser.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(loginUser);
            }

            if (loginUser.Password != user.Password)
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(loginUser);
            }

            HttpContext.Session.SetString("UserToken", user.Token);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Sign_out()
        {
            HttpContext.Session.Remove("UserToken");
            HttpContext.Session.Remove("UserName");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("api/breadcrumbs/{categoryId}")]
        public async Task<IActionResult> GetBreadcrumbs(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return BadRequest("Category ID is required.");
            }

            var breadcrumbs = new List<Category>();
            string currentCategoryId = categoryId;

            while (!string.IsNullOrEmpty(currentCategoryId))
            {
                var category = await _categoryService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                if (category != null)
                {
                    breadcrumbs.Insert(0, category);
                    currentCategoryId = category.ParentId;
                }
                else
                {
                    break;
                }
            }

            return Ok(breadcrumbs);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByPrice(decimal min, decimal max)
        {
            var products = await _productService.GetProductsByFinalPriceRangeAsync(min, max);

            var groupedProducts = ClassifyProductsByFeatured(products);

            return PartialView("_BookListPartial", groupedProducts.ToList());
        }

        private List<KeyValuePair<string, List<Product_admin>>> ClassifyProductsByFeatured(List<Product_admin> products)
        {
            var grouped = new List<KeyValuePair<string, List<Product_admin>>>();

            var highlightedProducts = products
                .Where(p => p.Featured == 1)
                .OrderByDescending(p => p.Position)
                .ThenByDescending(p => p.CreatedAt)
                .Take(20)
                .ToList();

            var newProducts = products
                .Where(p => p.Featured == 2)
                .OrderByDescending(p => p.Position)
                .ThenByDescending(p => p.CreatedAt)
.Take(20)
                .ToList();

            var suggestedProducts = products
                .Where(p => p.Featured == 3)
                .OrderByDescending(p => p.Position)
                .ThenByDescending(p => p.CreatedAt)
                .Take(20)
                .ToList();

            var bestsellerProducts = products
                .Where(p => p.DiscountPercentage > 0 && p.Featured != 1 && p.Featured != 2 && p.Featured != 3)
                .OrderByDescending(p => p.DiscountPercentage)
                .ThenByDescending(p => p.Position)
                .Take(20)
                .ToList();

            if (highlightedProducts.Any())
                grouped.Add(new KeyValuePair<string, List<Product_admin>>("Nổi bật", highlightedProducts));

            if (newProducts.Any())
                grouped.Add(new KeyValuePair<string, List<Product_admin>>("Mới", newProducts));

            if (suggestedProducts.Any())
                grouped.Add(new KeyValuePair<string, List<Product_admin>>("Gợi ý", suggestedProducts));

            if (bestsellerProducts.Any())
                grouped.Add(new KeyValuePair<string, List<Product_admin>>("Bán chạy", bestsellerProducts));

            return grouped;
        }
    }
}
