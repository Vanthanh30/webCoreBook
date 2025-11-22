using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using webCore.MongoHelper;
using System.Linq;

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class Admin_categoryController : Controller
    {
        private readonly CategoryProduct_adminService _CategoryProductCollection;
        private readonly ILogger<Admin_categoryController> _logger;

        public Admin_categoryController(CategoryProduct_adminService Category_adminService, ILogger<Admin_categoryController> logger)
        {
            _CategoryProductCollection = Category_adminService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string filter = "parent", int page = 1)
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            var categoryName = HttpContext.Session.GetString("CategoryName");
            ViewBag.CategoryName = categoryName;

            try
            {
                var categories = await _CategoryProductCollection.GetCategory();
                foreach (var category in categories)
                {
                    if (!string.IsNullOrEmpty(category.ParentId))
                    {
                        var parentCategory = categories.FirstOrDefault(c => c.Id == category.ParentId);
                        category.ParentTitle = parentCategory?.Title;
                    }
                }

                // Sort categories based on position
                var sortedCategories = categories.OrderBy(c => c.Position).ToList();

                // Filter categories based on filter parameter
                List<Category_admin> filteredCategories;
                if (filter == "parent")
                {
                    // Chỉ lấy danh mục cha (không có ParentId hoặc ParentId rỗng)
                    filteredCategories = sortedCategories.Where(c => string.IsNullOrEmpty(c.ParentId)).ToList();
                }
                else if (filter == "child")
                {
                    // Chỉ lấy danh mục con (có ParentId)
                    filteredCategories = sortedCategories.Where(c => !string.IsNullOrEmpty(c.ParentId)).ToList();
                }
                else
                {
                    // Lấy tất cả
                    filteredCategories = sortedCategories;
                }

                // Pagination logic
                int itemsPerPage = 5;
                int skip = (page - 1) * itemsPerPage;
                var pagedCategories = filteredCategories.Skip(skip).Take(itemsPerPage).ToList();

                // Total number of categories to calculate total pages
                int totalCategories = filteredCategories.Count();
                int totalPages = (int)Math.Ceiling(totalCategories / (double)itemsPerPage);

                // Pass pagination and filter information to the view
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.FilterType = filter;

                return View(pagedCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories from MongoDB.");
                return View("Error");
            }
        }


        public IActionResult Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            var categoryName = HttpContext.Session.GetString("CategoryName");
            ViewBag.CategoryName = categoryName;
            ViewBag.Categories = _CategoryProductCollection.GetCategory().Result;
            var hierarchicalCategories = GetHierarchicalCategories(ViewBag.Categories);
            ViewBag.Categories = hierarchicalCategories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category_admin category, string parentId)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = (await _CategoryProductCollection.GetCategory())
                    .FirstOrDefault(a => a.Title == category.Title);

                if (existingCategory != null)
                {
                    ModelState.AddModelError("Tên danh mục", "Đã có danh mục này.");
                    return View(category);
                }

                category.Id = Guid.NewGuid().ToString();
                category.ParentId = parentId;

                var categories = await _CategoryProductCollection.GetCategory();
                int maxPosition = categories.Any() ? categories.Max(c => c.Position) : 0;
                category.Position = maxPosition + 1;

                try
                {
                    // Lưu danh mục vào cơ sở dữ liệu
                    await _CategoryProductCollection.SaveCatelogyAsync(category);

                    // Cập nhật ParentTitle
                    if (!string.IsNullOrEmpty(parentId))
                    {
                        var parentCategory = categories.FirstOrDefault(c => c.Id == parentId);
                        category.ParentTitle = parentCategory?.Title;

                        // Cập nhật lại danh mục đã lưu với ParentTitle
                        await _CategoryProductCollection.UpdateCategoryAsync(category);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving category to MongoDB.");
                    ModelState.AddModelError("", "Could not save category to database. Please try again.");
                    return View(category);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _CategoryProductCollection.GetCategory();
            return View(category);
        }

        public async Task<IActionResult> Update(string id)
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            var category = await _CategoryProductCollection.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var categoryName = HttpContext.Session.GetString("CategoryName");
            ViewBag.CategoryName = categoryName;
            ViewBag.Categories = await _CategoryProductCollection.GetCategory();
            var hierarchicalCategories = GetHierarchicalCategories(ViewBag.Categories);
            ViewBag.Categories = hierarchicalCategories;
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Category_admin category, IFormFile Avatar, string parentId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing category from the database to retain its Position
                    var existingCategory = await _CategoryProductCollection.GetCategoryByIdAsync(category.Id);
                    if (existingCategory == null)
                    {
                        return NotFound(); // If the category doesn't exist, return 404
                    }

                    // Preserve the Position from the existing category
                    category.Position = existingCategory.Position;

                    // Update the category in the database
                    await _CategoryProductCollection.UpdateCategoryAsync(category);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating category in MongoDB.");
                    ModelState.AddModelError("", "Could not update category in database. Please try again.");
                    return View(category);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _CategoryProductCollection.GetCategory();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("", "Category ID is invalid.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Lấy danh mục cần xóa
                var categoryToDelete = await _CategoryProductCollection.GetCategoryByIdAsync(id);
                if (categoryToDelete == null)
                {
                    return NotFound(); // Nếu danh mục không tồn tại
                }

                // Lấy tất cả danh mục từ DB
                var allCategories = await _CategoryProductCollection.GetCategory();

                // Tìm danh mục con của danh mục cần xóa
                var categoriesToDelete = GetAllChildCategories(allCategories, id);
                categoriesToDelete.Add(categoryToDelete); // Bao gồm danh mục cha cần xóa

                // Xóa tất cả danh mục đã tìm được
                foreach (var category in categoriesToDelete)
                {
                    await _CategoryProductCollection.DeleteCategoryAsync(category.Id);
                }

                // Lấy danh sách còn lại sau khi xóa
                var remainingCategories = allCategories
                    .Where(c => !categoriesToDelete.Any(d => d.Id == c.Id))
                    .OrderBy(c => c.Position)
                    .ToList();

                // Cập nhật lại vị trí của các danh mục còn lại
                for (int i = 0; i < remainingCategories.Count; i++)
                {
                    remainingCategories[i].Position = i + 1; // Bắt đầu từ vị trí 1
                    await _CategoryProductCollection.UpdateCategoryAsync(remainingCategories[i]);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Category not found or already deleted.");
                ModelState.AddModelError("", "The category was not found or has already been deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category from MongoDB.");
                ModelState.AddModelError("", "Could not delete category from database. Please try again.");
            }

            return RedirectToAction(nameof(Index)); // Trả về danh sách sau khi xử lý
        }

        // Hàm hỗ trợ lấy tất cả danh mục con của một danh mục
        private List<Category_admin> GetAllChildCategories(List<Category_admin> categories, string parentId)
        {
            var result = new List<Category_admin>();

            foreach (var category in categories.Where(c => c.ParentId == parentId))
            {
                result.Add(category);
                // Đệ quy để lấy danh mục con của danh mục này
                result.AddRange(GetAllChildCategories(categories, category.Id));
            }

            return result;
        }

        //phân cấp bậc
        //phân cấp bậc
        private List<Category_admin> GetHierarchicalCategories(List<Category_admin> categories, string parentId = null, int level = 0)
        {
            var result = new List<Category_admin>();

            foreach (var category in categories.Where(c => c.ParentId == parentId))
            {
                // Thêm dấu gạch ngang để thể hiện cấp bậc
                category.Title = new string('-', level * 2) + " " + category.Title;
                result.Add(category);
                // Đệ quy để lấy danh mục con
                result.AddRange(GetHierarchicalCategories(categories, category.Id, level + 1));
            }

            return result;
        }
    }
}