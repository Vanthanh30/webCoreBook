using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.MongoHelper;
using webCore.Services;
using System.Linq;
using System.Collections.Generic;

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class DashboardController : Controller
    {
        private readonly User_adminService _useradminService;
        private readonly CategoryProduct_adminService _categoryProductCollection;

        public DashboardController(User_adminService useradminService, CategoryProduct_adminService categoryProductAdminService)
        {
            _useradminService = useradminService;
            _categoryProductCollection = categoryProductAdminService;
        }

        // GET: DashboardController
        public async Task<ActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.Token = token;

            // Lấy số liệu thống kê
            var allUsers = await _useradminService.GetAllUsersAsync();
            var totalUsers = allUsers.Count;

            var allCategories = await _categoryProductCollection.GetCategory();
            var totalCategories = allCategories.Count;

            var allProducts = await _categoryProductCollection.GetProduct();
            var totalProducts = allProducts.Count;

            // Lấy danh sách sản phẩm cần duyệt (Status khác "Hoạt động")
            var pendingProducts = allProducts
                .Where(p => string.IsNullOrEmpty(p.Status) ||
                           !p.Status.Equals("Hoạt động", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToList();

            // Tạo dictionary để map CategoryId -> CategoryTitle
            var categoryDictionary = new Dictionary<string, string>();
            foreach (var cat in allCategories)
            {
                if (!categoryDictionary.ContainsKey(cat.Id))
                {
                    categoryDictionary.Add(cat.Id, cat.Title);
                }
            }

            // Gán CategoryTitle cho mỗi sản phẩm cần duyệt
            foreach (var product in pendingProducts)
            {
                if (!string.IsNullOrEmpty(product.CategoryId) && categoryDictionary.ContainsKey(product.CategoryId))
                {
                    product.CategoryTitle = categoryDictionary[product.CategoryId];
                }
            }

            // Truyền dữ liệu vào ViewBag
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PendingProducts = pendingProducts;

            return View();
        }
    }
}