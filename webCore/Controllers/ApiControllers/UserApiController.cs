using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : Controller
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

            // lấy role Buyer từ DB
            var buyerRole = await _roleService.GetRoleByNameAsync("Buyer");
            if (buyerRole == null)
                return StatusCode(500, new { success = false, message = "Role Buyer chưa có trong DB" });

            // gán role Buyer
            user.RoleId.Add(buyerRole.Id);

            await _userService.SaveUserAsync(user);

            return Ok(new { success = true, message = "Đăng ký thành công" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User login)
        {
            var user = await _userService.GetAccountByEmailAsync(login.Email);
            if (user == null || user.Password != login.Password)
                return Unauthorized(new { success = false, message = "Sai email hoặc mật khẩu" });

            if (user.Status == 0)
                return Unauthorized(new { success = false, message = "Tài khoản bị khóa" });

            // lấy tên role
            var roleNames = new System.Collections.Generic.List<string>();
            foreach (var roleId in user.RoleId)
            {
                var r = await _roleService.GetRoleByIdAsync(roleId);
                if (r != null) roleNames.Add(r.Name);
            }

            // session optional
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

        [HttpPost("upgrade-seller")]
        public async Task<IActionResult> UpgradeSeller([FromBody] User req)
        {
            var user = await _userService.GetUserByIdAsync(req.Id);
            if (user == null)
                return BadRequest(new { success = false, message = "Không tìm thấy user" });

            var sellerRole = await _roleService.GetRoleByNameAsync("Seller");
            if (sellerRole == null)
                return StatusCode(500, new { success = false, message = "Role Seller chưa tồn tại trong DB" });

            if (!user.RoleId.Contains(sellerRole.Id))
            {
                user.RoleId.Add(sellerRole.Id);
                await _userService.UpdateUserAsync(user);
            }

            return Ok(new { success = true, message = "Bạn đã là người bán" });
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
