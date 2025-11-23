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
    public class HomeController : BaseController
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

        // Lấy danh sách sản phẩm theo danh mục (AJAX)
        public async Task<IActionResult> GetProductsByCategoryId(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return BadRequest("Category ID is required.");
            }

            var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
            return PartialView("_BookListPartial", products);
        }

        // Phương thức tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return PartialView("_ProductList", new List<Product_admin>());
            }

            // Tìm kiếm sản phẩm từ MongoDB
            var allProducts = await _productService.GetProductsAsync();
            var searchResults = allProducts
                .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_ProductList", searchResults);
        }

        // Xử lý đăng nhập và lưu thông tin vào session
        [HttpPost]
        public async Task<IActionResult> Sign_in(User loginUser)
        {
            if (!ModelState.IsValid)
            {
                return View(loginUser);
            }

            // Kiểm tra tài khoản trong MongoDB
            var user = await _userService.GetAccountByEmailAsync(loginUser.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(loginUser);
            }

            // Kiểm tra mật khẩu
            if (loginUser.Password != user.Password)
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(loginUser);
            }

            // Lưu thông tin đăng nhập vào session
            HttpContext.Session.SetString("UserToken", user.Token);
            HttpContext.Session.SetString("UserName", user.Name);

            return RedirectToAction("Index", "Home");
        }

        // Xử lý đăng xuất và xóa thông tin session
        [HttpPost]
        public IActionResult Sign_out()
        {
            HttpContext.Session.Remove("UserToken");
            HttpContext.Session.Remove("UserName");

            return RedirectToAction("Index", "Home");
        }

        // API lấy breadcrumb theo CategoryId
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