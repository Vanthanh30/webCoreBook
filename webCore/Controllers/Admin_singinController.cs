using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class Admin_singinController : Controller
    {
        private readonly AccountService _accountService;

        public Admin_singinController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // API endpoint để login và trả về JSON
        [HttpPost]
        [Route("Admin_singin/LoginApi")]
        public async Task<IActionResult> LoginApi([FromBody] LoginRequest request)
        {
            try
            {
                // Tìm kiếm tài khoản trong MongoDB
                var account = (await _accountService.GetAccounts())
                    .FirstOrDefault(a => a.Email == request.Username && a.Password == request.Password);

                if (account != null && account.Status == "Hoạt động")
                {
                    // Lưu thông tin admin vào session (backup)
                    HttpContext.Session.SetString("AdminId", account.Id);
                    HttpContext.Session.SetString("AdminToken", account.Token);
                    HttpContext.Session.SetString("AdminName", account.FullName);
                    HttpContext.Session.SetString("RoleId", account.RoleId);

                    // Trả về JSON với AdminId và Token để lưu vào localStorage
                    return Json(new
                    {
                        success = true,
                        adminId = account.Id,
                        token = account.Token,
                        name = account.FullName,
                        avatar = account.Avatar,
                        roleId = account.RoleId
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tên tài khoản hoặc mật khẩu không đúng. Vui lòng thử lại."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // API để get thông tin admin từ localStorage
        [HttpGet]
        [Route("Admin_singin/GetAdminInfo")]
        public async Task<IActionResult> GetAdminInfo(string adminId)
        {
            try
            {
                if (string.IsNullOrEmpty(adminId))
                {
                    return Json(new { success = false, message = "AdminId không hợp lệ" });
                }

                var admin = await _accountService.GetAccountByIdAsync(adminId);
                
                if (admin != null)
                {
                    return Json(new
                    {
                        success = true,
                        name = admin.FullName,
                        avatar = admin.Avatar,
                        email = admin.Email,
                        roleId = admin.RoleId
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy admin" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Đăng xuất
        [HttpPost]
        [HttpGet]
        public IActionResult Logout()
        {
            // Xóa tất cả thông tin khỏi session
            HttpContext.Session.Clear();
            
            return RedirectToAction("Index");
        }
    }

    // Model cho LoginRequest
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}