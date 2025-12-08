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

        // Trang chủ
        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> Index()
        {
            // Kiểm tra trạng thái đăng nhập
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;
            ViewBag.IsLoggedIn = isLoggedIn;

            // Lấy danh mục hoạt động từ MongoDB
            var categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            // Lấy danh sách sản phẩm nhóm theo trạng thái Featured
            var groupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            // Lấy danh sách sản phẩm nổi bật
            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            ViewBag.FeaturedProducts = featuredProducts;

            // Lấy danh sách sản phẩm bán chạy
            var bestsellerProducts = await _productService.GetBestsellerProductsAsync();
            ViewBag.BestsellerProducts = bestsellerProducts;

            return View(); // Trả về View Index.cshtml
        }

        // Lấy danh sách sản phẩm theo danh mục (AJAX) - ĐÃ SỬA
        public async Task<IActionResult> GetProductsByCategoryId(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return BadRequest("Category ID is required.");
            }

            // --- BẮT ĐẦU SỬA ĐỔI ---

            // 1. Lấy tất cả danh mục để kiểm tra quan hệ cha-con
            var allCategories = await _categoryService.GetCategoriesAsync();

            // 2. Tìm danh sách các ID danh mục con của categoryId hiện tại
            var childCategoryIds = allCategories
                .Where(c => c.ParentId == categoryId)
                .Select(c => c._id)
                .ToList();

            List<Product_admin> products = new List<Product_admin>();

            // 3. Logic: Nếu có con (là Cha) thì lấy cả cha lẫn con. Nếu không (là Con) thì chỉ lấy nó.
            if (childCategoryIds.Any())
            {
                // A. Trường hợp chọn Danh mục Cha:

                var parentProducts = await _productService.GetProductsByCategoryIdAsync(categoryId);
                products.AddRange(parentProducts);

                foreach (var childId in childCategoryIds)
                {
                    var childProducts = await _productService.GetProductsByCategoryIdAsync(childId);
                    products.AddRange(childProducts);
                }
            }
            else
            {

                products = await _productService.GetProductsByCategoryIdAsync(categoryId);
            }

            products = products.GroupBy(p => p.Id).Select(g => g.First()).ToList();

            var groupedProducts = new Dictionary<string, List<Product_admin>>();

            var featuredProducts = products.Where(p => p.DiscountPercentage > 0).ToList();
            var newProducts = products.Where(p => p.DiscountPercentage == 0 && p.Price > 0).Take(10).ToList();
            var suggestedProducts = products.Except(featuredProducts).Except(newProducts).ToList();

            if (featuredProducts.Any()) groupedProducts.Add("Nổi bật", featuredProducts);
            if (newProducts.Any()) groupedProducts.Add("Mới", newProducts);
            if (suggestedProducts.Any()) groupedProducts.Add("Gợi ý", suggestedProducts);

            var orderedGroupedProducts = groupedProducts
                .OrderBy(group =>
                    group.Key == "Nổi bật" ? 0 :
                    group.Key == "Mới" ? 1 :
                    group.Key == "Gợi ý" ? 2 : int.MaxValue)
                .ToList();

            return PartialView("_BookListPartial", orderedGroupedProducts);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            // Lấy tất cả dữ liệu như trang chủ
            var categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            var groupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            var featuredProducts = await _productService.GetFeaturedProductsAsync();
            ViewBag.FeaturedProducts = featuredProducts;

            var bestsellerProducts = await _productService.GetBestsellerProductsAsync();
            ViewBag.BestsellerProducts = bestsellerProducts;

            // === PHẦN TÌM KIẾM ===
            ViewBag.SearchQuery = q?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchResults = await _productService.SearchProductsAsync(q);

                // Tạo nhóm giống hệt trang chủ
                var grouped = new List<KeyValuePair<string, List<Product_admin>>>();

                var flash = searchResults.Where(p => p.DiscountPercentage > 0).Take(20).ToList();
                var newest = searchResults.Where(p => p.DiscountPercentage == 0).Take(20).ToList();
                var suggest = searchResults.Except(flash).Except(newest).Take(20).ToList();

                if (flash.Any()) grouped.Add(new("Nổi bật", flash));
                if (newest.Any()) grouped.Add(new("Mới", newest));
                if (suggest.Any()) grouped.Add(new("Gợi ý", suggest));

                ViewBag.SearchResults = grouped; // Dùng để hiển thị thay thế
                ViewBag.IsSearching = true;
                ViewBag.ResultCount = searchResults.Count;
            }
            else
            {
                ViewBag.IsSearching = false;
            }

            // TRẢ VỀ LUÔN TRANG CHỦ (Index.cshtml)
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
    }
}