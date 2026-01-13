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

                var sortedCategories = categories.OrderBy(c => c.Position).ToList();

                List<Category_admin> filteredCategories;
                if (filter == "parent")
                {
                    filteredCategories = sortedCategories.Where(c => string.IsNullOrEmpty(c.ParentId)).ToList();
                }
                else if (filter == "child")
                {
                    filteredCategories = sortedCategories.Where(c => !string.IsNullOrEmpty(c.ParentId)).ToList();
                }
                else
                {
                    filteredCategories = sortedCategories;
                }

                int itemsPerPage = 5;
                int skip = (page - 1) * itemsPerPage;
                var pagedCategories = filteredCategories.Skip(skip).Take(itemsPerPage).ToList();

                int totalCategories = filteredCategories.Count();
                int totalPages = (int)Math.Ceiling(totalCategories / (double)itemsPerPage);

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
                    await _CategoryProductCollection.SaveCatelogyAsync(category);

                    if (!string.IsNullOrEmpty(parentId))
                    {
                        var parentCategory = categories.FirstOrDefault(c => c.Id == parentId);
                        category.ParentTitle = parentCategory?.Title;

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
                    var existingCategory = await _CategoryProductCollection.GetCategoryByIdAsync(category.Id);
                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    category.Position = existingCategory.Position;

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
                var categoryToDelete = await _CategoryProductCollection.GetCategoryByIdAsync(id);
                if (categoryToDelete == null)
                {
                    return NotFound();
                }

                var allCategories = await _CategoryProductCollection.GetCategory();

                var categoriesToDelete = GetAllChildCategories(allCategories, id);
                categoriesToDelete.Add(categoryToDelete); 
                foreach (var category in categoriesToDelete)
                {
                    await _CategoryProductCollection.DeleteCategoryAsync(category.Id);
                }

                var remainingCategories = allCategories
                    .Where(c => !categoriesToDelete.Any(d => d.Id == c.Id))
                    .OrderBy(c => c.Position)
                    .ToList();

                for (int i = 0; i < remainingCategories.Count; i++)
                {
                    remainingCategories[i].Position = i + 1; 
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

            return RedirectToAction(nameof(Index)); 
        }

        private List<Category_admin> GetAllChildCategories(List<Category_admin> categories, string parentId)
        {
            var result = new List<Category_admin>();

            foreach (var category in categories.Where(c => c.ParentId == parentId))
            {
                result.Add(category);
                result.AddRange(GetAllChildCategories(categories, category.Id));
            }

            return result;
        }

        private List<Category_admin> GetHierarchicalCategories(List<Category_admin> categories, string parentId = null, int level = 0)
        {
            var result = new List<Category_admin>();

            foreach (var category in categories.Where(c => c.ParentId == parentId))
            {
                category.Title = new string('-', level * 2) + " " + category.Title;
                result.Add(category);
                result.AddRange(GetHierarchicalCategories(categories, category.Id, level + 1));
            }

            return result;
        }
    }
}