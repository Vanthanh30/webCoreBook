using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly RoleService _roleService;

        public UserApiController(UserService userService, RoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            var existing = await _userService.GetAccountByEmailAsync(user.Email);
            if (existing != null)
                return BadRequest(new { success = false, message = "Email đã tồn tại" });

            if (string.IsNullOrEmpty(user.Password))
                return BadRequest(new { success = false, message = "Mật khẩu không được để trống" });

            user.Password = PasswordHasher.HashPassword(user.Password);

            var buyerRole = await _roleService.GetRoleByNameAsync("Buyer");
            if (buyerRole == null)
                return StatusCode(500, new { success = false, message = "Role Buyer chưa có trong DB" });

            user.RoleId.Add(buyerRole.Id);

            await _userService.SaveUserAsync(user);

            return Ok(new { success = true, message = "Đăng ký thành công" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User login)
        {
            var user = await _userService.GetAccountByEmailAsync(login.Email);
            if (user == null)
                return Unauthorized(new { success = false, message = "Sai email hoặc mật khẩu" });

            bool validPassword = PasswordHasher.VerifyPassword(login.Password, user.Password);

            if (!validPassword)
                return Unauthorized(new { success = false, message = "Sai email hoặc mật khẩu" });

            if (user.Status == 0)
                return Unauthorized(new { success = false, message = "Tài khoản bị khóa" });

            var roleNames = new List<string>();
            foreach (var roleId in user.RoleId)
            {
                var role = await _roleService.GetRoleByIdAsync(roleId);
                if (role != null)
                {
                    roleNames.Add(role.Name);

                    // Nếu là admin → không cho đăng nhập
                    if (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return Unauthorized(new
                        {
                            success = false,
                            message = "Tài khoản không có quyền truy cập trang này"
                        });
                    }
                }
            }


            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("UserToken", user.Token);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRoles", string.Join(",", roleNames));

            return Ok(new
            {
                success = true,
                name = user.Name,
                roles = roleNames,
                token = user.Token
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserRoles");
            HttpContext.Session.Remove("UserToken");

            return Ok(new { success = true, message = "Đăng xuất thành công" });
        }

    }
}
