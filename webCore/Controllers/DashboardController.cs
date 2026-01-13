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

        public async Task<ActionResult> Index()
        {
            var token = HttpContext.Session.GetString("AdminToken");
            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.Token = token;

            var allUsers = await _useradminService.GetAllUsersAsync();
            var totalUsers = allUsers.Count;

            var allCategories = await _categoryProductCollection.GetCategory();
            var totalCategories = allCategories.Count;

            var allProducts = await _categoryProductCollection.GetProduct();
            var totalProducts = allProducts.Count;

            var pendingProducts = allProducts
                .Where(p => p.Status != null &&
                            p.Status.Equals("Chờ duyệt", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToList();

            var categoryDictionary = new Dictionary<string, string>();
            foreach (var cat in allCategories)
            {
                if (!categoryDictionary.ContainsKey(cat.Id))
                {
                    categoryDictionary.Add(cat.Id, cat.Title);
                }
            }

            foreach (var product in pendingProducts)
            {
                if (!string.IsNullOrEmpty(product.CategoryId) && categoryDictionary.ContainsKey(product.CategoryId))
                {
                    product.CategoryTitle = categoryDictionary[product.CategoryId];
                }
            }

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PendingProducts = pendingProducts;

            return View();
        }
    }
}