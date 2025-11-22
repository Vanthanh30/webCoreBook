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
        private const int PageSize = 5; // Số lượng người dùng trên mỗi trang

        public Admin_userController(User_adminService useadminService)
        {
            _useradminService = useadminService;
        }

        // Hiển thị danh sách tất cả người dùng với phân trang
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                ViewBag.AdminName = adminName;

                // Lấy tất cả người dùng từ service
                var allUsers = await _useradminService.GetAllUsersAsync();

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
                TempData["ErrorMessage"] = "Lỗi khi tải . " + ex.Message;
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
                var user = await _useradminService.GetUserByIdAsync(id); // Gọi service để lấy thông tin đơn hàng
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                    return RedirectToAction("Index"); // Quay lại trang danh sách nếu đơn hàng không tìm thấy
                }

                return View(user);  // Trả về View chi tiết đơn hàng
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message;
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
                    return RedirectToAction("Index"); // Redirect đến trang Index
                }

                // Lấy người dùng theo ID
                var user = await _useradminService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                    return RedirectToAction("Index"); // Redirect đến trang Index
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
                return RedirectToAction("Index"); // Redirect đến trang Index để tải lại dữ liệu
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("Index"); // Redirect đến trang Index nếu có lỗi
            }
        }
    }
}