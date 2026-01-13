using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoDBService _mongoDBService;
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        public CategoryController(MongoDBService mongoDBService, ProductService productService, CategoryService categoryService)
        {
            _mongoDBService = mongoDBService;
            _productService = productService;
            _categoryService = categoryService;
        }
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
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            ViewBag.Categories = categories;

            var groupedProducts = await _productService.GetProductsGroupedByFeaturedAsync();
            ViewBag.GroupedProducts = groupedProducts;

            return View();
        }
    }
}
