using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers;
using webCore.MongoHelper;

namespace webCore.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/adminSignin")]
    public class Admin_signinApiController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly RoleService _roleService;
        public Admin_signinApiController(AccountService accountService, RoleService roleService)
        {
            _accountService = accountService;
            _roleService = roleService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var account = await _accountService.GetAccountByEmailAsync(request.Username);

            if (account == null)
                return Unauthorized(new { success = false, message = "Sai tài khoản hoặc mật khẩu." });

            if (!PasswordHasher.VerifyPassword(request.Password, account.Password))
                return Unauthorized(new { success = false, message = "Sai tài khoản hoặc mật khẩu." });

            if (account.Status != 1)
                return Unauthorized(new { success = false, message = "Tài khoản đã bị khóa." });

            // ⭐ KIỂM TRA ROLE ADMIN
            bool isAdmin = false;
            List<string> roleNames = new();

            foreach (var roleId in account.RoleId)
            {
                var role = await _roleService.GetRoleByIdAsync(roleId);
                if (role != null)
                {
                    roleNames.Add(role.Name);

                    if (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                        isAdmin = true;
                }
            }

            if (!isAdmin)
                return Unauthorized(new { success = false, message = "Bạn không có quyền truy cập trang quản trị." });

            // SAVE SESSION
            HttpContext.Session.SetString("AdminId", account.Id);
            HttpContext.Session.SetString("AdminToken", account.Token);
            HttpContext.Session.SetString("AdminName", account.Name);
            HttpContext.Session.SetString("RoleId", string.Join(",", account.RoleId));
            HttpContext.Session.SetString("RoleName", string.Join(",", roleNames));


            return Ok(new
            {
                success = true,
                adminId = account.Id,
                token = account.Token,
                name = account.Name,
                avatar = account.ProfileImage,
                roleId = account.RoleId
            });
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetAdminInfo([FromQuery] string adminId)
        {
            if (string.IsNullOrEmpty(adminId))
                return BadRequest(new { success = false, message = "adminId rỗng." });

            var admin = await _accountService.GetAccountByIdAsync(adminId);

            if (admin == null)
                return NotFound(new { success = false, message = "Không tìm thấy admin." });

            return Ok(new
            {
                success = true,
                name = admin.Name,
                avatar = admin.ProfileImage == "default-image-url" ? null : admin.ProfileImage,
                email = admin.Email,
                roleId = admin.RoleId
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true, message = "Đã đăng xuất." });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
