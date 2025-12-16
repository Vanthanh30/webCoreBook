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
        private const int PageSize = 5; // Số lượng người dùng trên mỗi trang

        public Admin_userController(User_adminService useadminService, RoleService roleService)
        {
            _useradminService = useadminService;
            _roleService = roleService;
        }

        // Hiển thị danh sách tất cả người dùng với phân trang (không bao gồm Admin)
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;

                // Lấy danh sách RoleId của Admin để loại trừ
                var adminRoleIds = await _roleService.GetAdminRoleIdsAsync();

                // Lấy tất cả người dùng KHÔNG phải Admin từ service
                var allUsers = await _useradminService.GetAllUsersAsync(adminRoleIds);

                // Tính toán phân trang
                var totalUsers = allUsers.Count;
                var totalPages = (int)Math.Ceiling(totalUsers / (double)PageSize);

                // Đảm bảo page hợp lệ
                if (page < 1) page = 1;
                if (page > totalPages && totalPages > 0) page = totalPages;

                // Lấy dữ liệu cho trang hiện tại
                var usersOnPage = allUsers
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                // Truyền thông tin phân trang vào ViewBag
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                // Kiểm tra nếu không có người dùng nào
                if (totalUsers == 0)
                {
                    TempData["ErrorMessage"] = "Không có người dùng nào để hiển thị.";
                    return View(new List<User>());
                }

                // Trả về view với danh sách người dùng của trang hiện tại
                return View(usersOnPage);
            }
            catch (Exception ex)
            {
                // Xử lý khi có lỗi xảy ra
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách người dùng: " + ex.Message;
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound(); // Nếu id không hợp lệ, trả về lỗi 404
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
                // Kiểm tra nếu id rỗng hoặc null
                if (string.IsNullOrEmpty(id))
                {
                    TempData["ErrorMessage"] = "ID không hợp lệ.";
                    return RedirectToAction("Index");
                }

                // Lấy người dùng theo ID
                var user = await _useradminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    return RedirectToAction("Index");
                }

                // Xác định trạng thái mới: nếu đang là 1 thì đổi thành 0
                var newStatus = (user.Status == 1) ? 0 : 1;

                // Cập nhật trạng thái trong MongoDB
                var isUpdated = await _useradminService.UpdateUserStatusAsync(id, newStatus);

                if (isUpdated)
                {
                    TempData["SuccessMessage"] = "Trạng thái người dùng đã được cập nhật thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật trạng thái.";
                }

                // Tải lại trang Index
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

