using DnsClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers.Attributes;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    [AuthorizeRole("Seller")]
    public class SellerProductController : BaseController
    {
        private readonly ProductService _productService;
        private readonly ReviewService _reviewService;

        public SellerProductController(ProductService productService, ReviewService reviewService)
        {
            _productService = productService;
            _reviewService = reviewService;
        }
        public IActionResult ProductManagement()
        {
            return View();
        }
        public IActionResult EditProduct()
        {
            return View();
        }
        public IActionResult AddProduct()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ProductReviews(string productId, int? rating, int page = 1)
        {
            int pageSize = 1;
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null) return NotFound("Sản phẩm không tồn tại");

            var allReviews = await _reviewService.GetByProductIdAsync(productId);

            var totalReviews = allReviews.Count;
            double averageRating = totalReviews > 0 ? allReviews.Average(r => r.QualityRating) : 0;

            var starCounts = new Dictionary<int, int>
            {
                { 5, allReviews.Count(r => r.QualityRating == 5) },
                { 4, allReviews.Count(r => r.QualityRating == 4) },
                { 3, allReviews.Count(r => r.QualityRating == 3) },
                { 2, allReviews.Count(r => r.QualityRating == 2) },
                { 1, allReviews.Count(r => r.QualityRating == 1) }
            };

            var filteredReviews = allReviews; // Dùng biến trung gian
            if (rating.HasValue && rating.Value > 0)
            {
                filteredReviews = allReviews.Where(r => r.QualityRating == rating.Value).ToList();
            }

            int totalItems = filteredReviews.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            page = Math.Max(1, page);
            if (totalPages > 0) page = Math.Min(page, totalPages);
            var pagedReviews = filteredReviews
                        .OrderByDescending(r => r.CreatedAt) 
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            ViewBag.Product = product;
            ViewBag.AverageRating = Math.Round(averageRating, 1); 
            ViewBag.TotalReviews = totalReviews;
            ViewBag.StarCounts = starCounts;
            ViewBag.CurrentRatingFilter = rating ?? 0;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedReviews);
        }
    }
}
