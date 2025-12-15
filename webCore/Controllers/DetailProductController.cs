
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
        private readonly ReviewService _reviewService;

        public DetailProductController(
            MongoDBService mongoDBService,
            DetailProductService detailProductService,
            ProductService productService,
            CategoryService categoryService,
            ReviewService reviewService)
        {
            _mongoDBService = mongoDBService;
            _detailProductService = detailProductService;
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
        }

        // Phương thức tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return PartialView("_ProductList", new List<Product_admin>());
            }

            var allProducts = await _productService.GetProductsAsync();
            var searchResults = allProducts
                .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_ProductList", searchResults);
        }

        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> DetailProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound("Không tìm thấy sản phẩm – ID trống.");
            }

            var product = await _detailProductService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound("Sản phẩm bạn đang tìm không tồn tại hoặc đã bị xóa.");
            }

            var userToken = HttpContext.Session.GetString("UserToken");
            var userName = HttpContext.Session.GetString("UserName");

            ViewBag.IsLoggedIn = !string.IsNullOrEmpty(userToken);
            ViewBag.UserName = userName;
            ViewBag.UserToken = userToken;

            var similarProducts = await GetSimilarProducts(product);
            ViewBag.SimilarProducts = similarProducts ?? new List<Product_admin>();

            var breadcrumbs = new List<Category>
            {
                new Category { Title = "Trang chủ", _id = "home" }
            };

            string currentCategoryId = product.CategoryId;

            if (!string.IsNullOrEmpty(currentCategoryId))
            {
                while (!string.IsNullOrEmpty(currentCategoryId))
                {
                    var category = await _categoryService.GetCategoryBreadcrumbByIdAsync(currentCategoryId);
                    if (category == null) break;

                    breadcrumbs.Insert(1, category);
                    currentCategoryId = category.ParentId;
                }
            }

            ViewBag.Breadcrumbs = breadcrumbs;

            return View(product);
        }

        [HttpGet("api/reviews/product/{productId}")]
        public async Task<IActionResult> GetProductReviews(string productId)
        {
            Console.WriteLine($"=== GetProductReviews API Called ===");
            Console.WriteLine($"ProductId received: '{productId}'");

            if (string.IsNullOrEmpty(productId))
            {
                Console.WriteLine("ERROR: ProductId is null or empty");
                return BadRequest(new { message = "Product ID is required." });
            }

            try
            {
                var reviews = await _reviewService.GetByProductIdAsync(productId);
                Console.WriteLine($"Found {reviews.Count} reviews");

                if (reviews.Count > 0)
                {
                    Console.WriteLine($"First review - QualityRating: {reviews[0].QualityRating}, ServiceRating: {reviews[0].ServiceRating}");
                }
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Lỗi khi tải đánh giá", error = ex.Message });
            }
        }

        // ===== API LẤY THỐNG KÊ ĐÁNH GIÁ =====
        [HttpGet("api/reviews/stats/{productId}")]
        public async Task<IActionResult> GetReviewStats(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return BadRequest(new { message = "Product ID is required." });
            }

            try
            {
                var reviews = await _reviewService.GetByProductIdAsync(productId);

                if (reviews.Count == 0)
                {
                    return Ok(new
                    {
                        totalReviews = 0,
                        averageRating = 0.0,
                        ratingBreakdown = new { star5 = 0, star4 = 0, star3 = 0, star2 = 0, star1 = 0 }
                    });
                }

                // Tính toán thống kê
                var avgQuality = reviews.Average(r => r.QualityRating);
                var avgService = reviews.Average(r => r.ServiceRating);
                var overallAvg = (avgQuality + avgService) / 2;

                // Đếm số lượng mỗi mức sao
                var ratingCounts = new int[5];
                foreach (var review in reviews)
                {
                    var avgRating = (int)Math.Round((review.QualityRating + review.ServiceRating) / 2.0);
                    if (avgRating >= 1 && avgRating <= 5)
                    {
                        ratingCounts[avgRating - 1]++;
                    }
                }

                return Ok(new
                {
                    totalReviews = reviews.Count,
                    averageRating = Math.Round(overallAvg, 1),
                    ratingBreakdown = new
                    {
                        star5 = ratingCounts[4],
                        star4 = ratingCounts[3],
                        star3 = ratingCounts[2],
                        star2 = ratingCounts[1],
                        star1 = ratingCounts[0]
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi khi tải thống kê",
                    error = ex.Message
                });
            }
        }

        [HttpGet("api/product/breadcrumbs/{productId}")]
        public async Task<IActionResult> GetProductBreadcrumbs(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return BadRequest("Product ID is required.");
            }

            var product = await _detailProductService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var breadcrumbs = new List<Category>();
            string currentCategoryId = product.CategoryId;

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

        private async Task<List<Product_admin>> GetSimilarProducts(Product_admin product)
        {
            var similarProducts = await _detailProductService.GetProductsByCategoryAsync(product.CategoryId);
            similarProducts = similarProducts.Where(p => p.Id != product.Id).ToList();
            return similarProducts.Take(10).ToList();
        }
    }
}