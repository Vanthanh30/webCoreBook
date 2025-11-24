using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers.Attributes;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers.ApiControllers
{
    [ApiAuthorizeRoleAttribute("Buyer")]
    [Route("api/shop")]
    [ApiController]
    public class ShopApiController : ControllerBase
    {
        private readonly ShopService _shopService;
        private readonly UserService _userService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly RoleService _roleService;

        public ShopApiController(ShopService shopService,UserService userService, CloudinaryService cloudinaryService, RoleService roleService)
        {
            _shopService = shopService;
            _userService = userService;
            _cloudinaryService = cloudinaryService;
            _roleService = roleService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterShop([FromForm] Shop model, IFormFile Avatar)
        {
            // ⭐ Lấy UserId từ Session
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập!"
                });
            }

            // ⭐ Lấy user từ DB
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy user!"
                });
            }

            // ⭐ Kiểm tra đã có shop chưa
            var existedShop = await _shopService.GetShopByUserIdAsync(user.Id);
            if (existedShop != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "User đã có shop!"
                });
            }

            // ⭐ Upload avatar
            string shopImage = "default-image-url";
            if (Avatar != null)
            {
                shopImage = await _cloudinaryService.UploadImageAsync(Avatar);
            }

            // ⭐ Gán thông tin tự động từ user
            model.UserId = user.Id;
            model.Email = user.Email;
            model.Phone = user.Phone;
            model.ShopImage = shopImage;

            // ⭐ Tạo Shop
            await _shopService.CreateShopAsync(model);

            // ======================
            // ⭐ THÊM ROLE SELLER
            // ======================
            var sellerRole = await _roleService.GetRoleByNameAsync("Seller");
            if (sellerRole == null)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Role Seller chưa tồn tại!"
                });
            }

            // ⭐ Gọi UserService để thêm role
            await _userService.AddRoleToUserAsync(user.Id, sellerRole.Id);

            return Ok(new
            {
                success = true,
                message = "Đăng ký shop thành công! Bạn đã trở thành người bán.",
                data = model
            });
        }



        [HttpGet("info")]
        public async Task<IActionResult> GetUserInfo()
        {
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false });

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
                return NotFound(new { success = false });

            return Ok(new
            {
                success = true,
                email = user.Email,
                phone = user.Phone
            });
        }

    }
}
