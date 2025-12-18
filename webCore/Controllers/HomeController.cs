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
                var flashSaleProducts = await _productService.GetTopDiscountProductsAsync(3);
                ViewBag.FlashSaleProducts = flashSaleProducts;

                return View(); // Trả về View Index.cshtml
            }
            public async Task<IActionResult> GetProductsByCategoryId(string categoryId)
            {
                if (string.IsNullOrEmpty(categoryId))
                {
                    return BadRequest("Category ID is required.");
                }

                var allCategories = await _categoryService.GetCategoriesAsync();
                var childCategoryIds = allCategories
                    .Where(c => c.ParentId == categoryId)
                    .Select(c => c._id)
                    .ToList();

                List<Product_admin> products = new List<Product_admin>();
                if (childCategoryIds.Any())
                {
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

                // ✅ BAO GỒM CẢ Featured = 0 (Không nổi bật)
                var featuredGroups = products
                    .GroupBy(p => p.Featured switch
                    {
                        1 => "Nổi bật",
                        2 => "Mới",
                        3 => "Gợi ý",
                        0 => "Không nổi bật",
                        _ => "Không nổi bật"
                    })
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Thêm vào dictionary theo thứ tự ưu tiên
                if (featuredGroups.ContainsKey("Nổi bật"))
                    groupedProducts.Add("Nổi bật", featuredGroups["Nổi bật"]);

                if (featuredGroups.ContainsKey("Mới"))
                    groupedProducts.Add("Mới", featuredGroups["Mới"]);

                if (featuredGroups.ContainsKey("Gợi ý"))
                    groupedProducts.Add("Gợi ý", featuredGroups["Gợi ý"]);

                if (featuredGroups.ContainsKey("Không nổi bật"))
                    groupedProducts.Add("Không nổi bật", featuredGroups["Không nổi bật"]);

                // Chuyển sang List với thứ tự đã định
                var orderedGroupedProducts = groupedProducts
                    .Select(kvp => new KeyValuePair<string, List<Product_admin>>(kvp.Key, kvp.Value))
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

                var flashSaleProducts = await _productService.GetTopDiscountProductsAsync(3);
                ViewBag.FlashSaleProducts = flashSaleProducts;

                ViewBag.SearchQuery = q?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var searchResults = await _productService.SearchProductsAsync(q);

                    // ✅ BAO GỒM CẢ Featured = 0 (Không nổi bật)
                    var grouped = new List<KeyValuePair<string, List<Product_admin>>>();

                    var featuredGroups = searchResults
                        .GroupBy(p => p.Featured switch
                        {
                            1 => "Nổi bật",
                            2 => "Mới",
                            3 => "Gợi ý",
                            0 => "Không nổi bật",
                            _ => "Không nổi bật"
                        })
                        .ToDictionary(g => g.Key, g => g.ToList());

                    if (featuredGroups.ContainsKey("Nổi bật"))
                        grouped.Add(new KeyValuePair<string, List<Product_admin>>("Nổi bật", featuredGroups["Nổi bật"]));

                    if (featuredGroups.ContainsKey("Mới"))
                        grouped.Add(new KeyValuePair<string, List<Product_admin>>("Mới", featuredGroups["Mới"]));

                    if (featuredGroups.ContainsKey("Gợi ý"))
                        grouped.Add(new KeyValuePair<string, List<Product_admin>>("Gợi ý", featuredGroups["Gợi ý"]));

                    if (featuredGroups.ContainsKey("Không nổi bật"))
                        grouped.Add(new KeyValuePair<string, List<Product_admin>>("Không nổi bật", featuredGroups["Không nổi bật"]));

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
            [HttpGet]
            public async Task<IActionResult> GetProductsByPrice(decimal min, decimal max)
            {
                var products = await _productService.GetProductsByFinalPriceRangeAsync(min, max);

                var groupedProducts = new Dictionary<string, List<Product_admin>>();

                var featuredGroups = products
                    .GroupBy(p => p.Featured switch
                    {
                        1 => "Nổi bật",
                        2 => "Mới",
                        3 => "Gợi ý",
                        0 => "Không nổi bật",
                        _ => "Không nổi bật"
                    })
                    .ToDictionary(g => g.Key, g => g.ToList());

                if (featuredGroups.ContainsKey("Nổi bật"))
                    groupedProducts.Add("Nổi bật", featuredGroups["Nổi bật"]);

                if (featuredGroups.ContainsKey("Mới"))
                    groupedProducts.Add("Mới", featuredGroups["Mới"]);

                if (featuredGroups.ContainsKey("Gợi ý"))
                    groupedProducts.Add("Gợi ý", featuredGroups["Gợi ý"]);

                if (featuredGroups.ContainsKey("Không nổi bật"))
                    groupedProducts.Add("Không nổi bật", featuredGroups["Không nổi bật"]);

                // ✅ QUAN TRỌNG: Chuyển sang List<KeyValuePair<>> giống các method khác
                var orderedGroupedProducts = groupedProducts
                    .Select(kvp => new KeyValuePair<string, List<Product_admin>>(kvp.Key, kvp.Value))
                    .ToList();

                return PartialView("_BookListPartial", orderedGroupedProducts);
            }
        }
    }