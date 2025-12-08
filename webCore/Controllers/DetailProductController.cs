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
    public class DetailProductController : Controller
    {
        private readonly MongoDBService _mongoDBService;


        private readonly DetailProductService _detailProductService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;

        public DetailProductController(MongoDBService mongoDBService, DetailProductService detailProductService, ProductService productService, CategoryService categoryService)
        {
            _mongoDBService = mongoDBService;
            _detailProductService = detailProductService;
            _productService = productService;
            _categoryService = categoryService;
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

        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> DetailProduct(string id)
        {
            // === 1. Kiểm tra ID hợp lệ ===
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy sản phẩm – ID trống.");
            }

            // === 2. Lấy sản phẩm ===
            var product = await _detailProductService.GetProductByIdAsync(id);

            // Nếu không tìm thấy sản phẩm → trả về 404 đẹp, không để null
            if (product == null)
            {
                return NotFound("Sản phẩm bạn đang tìm không tồn tại hoặc đã bị xóa.");
            }

            // === 3. Truyền trạng thái đăng nhập (giữ nguyên logic cũ) ===
            var userToken = HttpContext.Session.GetString("UserToken");
            var userName = HttpContext.Session.GetString("UserName");

            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(userToken);
            ViewBag.UserName = userName;
            ViewBag.UserToken = userToken;

            // === 4. Lấy sản phẩm tương tự (an toàn) ===
            var similarProducts = await GetSimilarProducts(product);
            ViewBag.SimilarProducts = similarProducts ?? new List<Product_admin>();

            // === 5. Xử lý Breadcrumbs – AN TOÀN HOÀN TOÀN ===
            var breadcrumbs = new List<Category>
    {
        new Category { Title = "Trang chủ", _id = "home" }
    };

            string currentCategoryId = product.CategoryId;

            // Nếu sản phẩm không có danh mục → vẫn cho vào trang chi tiết, chỉ không có breadcrumb
            if (!string.IsNullOrEmpty(currentCategoryId))
            {
                while (!string.IsNullOrEmpty(currentCategoryId))
                {
                    var category = await _categoryService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                    if (category == null) break; // thoát vòng lặp nếu không tìm thấy

                    breadcrumbs.Insert(1, category);
                    currentCategoryId = category.ParentId;
                }
            }

            ViewBag.Breadcrumbs = breadcrumbs;

            // === 6. Trả về view – Model chắc chắn không null ===
            return View(product);
        }


        [HttpGet("api/product/breadcrumbs/{productId}")]
        public async Task<IActionResult> GetProductBreadcrumbs(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return BadRequest("Product ID is required.");
            }

            // Lấy thông tin sản phẩm
            var product = await _detailProductService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Truy vấn danh mục breadcrumb từ categoryId của sản phẩm
            var breadcrumbs = new List<Category>();

            string currentCategoryId = product.CategoryId;

            // Lấy breadcrumb của các danh mục cha
            while (!string.IsNullOrEmpty(currentCategoryId))
            {
                var category = await _categoryService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                if (category != null)
                {
                    breadcrumbs.Insert(0, category); // Thêm vào **đầu danh sách**
                    currentCategoryId = category.ParentId; // Lấy danh mục cha
                }
                else
                {
                    break;
                }
            }

            return Ok(new
            {
                Product = new
                {
                    product.Id,
                    product.Title,
                    product.Description,
                    product.Price,
                    product.Image
                },
                Breadcrumbs = breadcrumbs
            });
        }
        // Phương thức để lấy các sản phẩm tương tự
        private async Task<List<Product_admin>> GetSimilarProducts(Product_admin product)
        {
            // Lấy tất cả sản phẩm cùng danh mục
            var similarProducts = await _detailProductService.GetProductsByCategoryAsync(product.CategoryId);

            // Loại bỏ sản phẩm hiện tại khỏi danh sách
            similarProducts = similarProducts.Where(p => p.Id != product.Id).ToList();

            // Lấy tối đa 10 sản phẩm tương tự
            return similarProducts.Take(10).ToList();
        }

    }
}
