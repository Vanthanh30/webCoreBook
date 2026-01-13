using Microsoft.AspNetCore.Mvc;
using webCore.Services;
using webCore.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    public class Admin_userController : Controller
    {
        private readonly User_adminService _useradminService;
        private readonly RoleService _roleService;
        private const int PageSize = 5; 

        public Admin_userController(User_adminService useadminService, RoleService roleService)
        {
            _useradminService = useadminService;
            _roleService = roleService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;

                var adminRoleIds = await _roleService.GetAdminRoleIdsAsync();

                var allUsers = await _useradminService.GetAllUsersAsync(adminRoleIds);

                var totalUsers = allUsers.Count;
                var totalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                var usersOnPage = allUsers
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                if (totalUsers == 0)
                {
                    TempData["ErrorMessage"] = "Không có người dùng nào để hiển thị.";
                    return View(new List<User>());
                }

                return View(usersOnPage);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách người dùng: " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); 
            }

            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;
                var user = await _useradminService.GetUserByIdAsync(id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    return RedirectToAction("Index");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết người dùng: " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = "ID không hợp lệ.";
                    return RedirectToAction("Index");
                }

                var user = await _useradminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    return RedirectToAction("Index");
                }

                var newStatus = (user.Status == 1) ? 0 : 1;

                var isUpdated = await _useradminService.UpdateUserStatusAsync(id, newStatus);

                if (isUpdated)
                {
                    TempData["SuccessMessage"] = "Trạng thái người dùng đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}

