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
            var roles = HttpContext.Session.GetString("UserRoles");
            var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (!roleList.Contains("Seller"))
                roleList.Add("Seller");

            HttpContext.Session.SetString("UserRoles", string.Join(",", roleList));
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
            if (user.Phone == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Vui lòng cập nhật đủ thông tin cá nhân để tạo shop!"
                });
            }
            return Ok(new
            {
                success = true,
                email = user.Email,
                phone = user.Phone
            });
        }

        [HttpGet("my-shop")]
        public async Task<IActionResult> GetMyShop()
        {
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập!"
                });
            }

            try
            {
                var shop = await _shopService.GetShopByUserIdAsync(userId);

                if (shop == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Bạn chưa có shop. Vui lòng đăng ký shop!"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        id = shop.Id,
                        shopName = shop.ShopName,
                        description = shop.Description,
                        businessType = shop.BusinessType,
                        address = shop.Address,
                        email = shop.Email,
                        phone = shop.Phone,
                        shopImage = shop.ShopImage,
                        createdAt = shop.CreatedAt,
                        updatedAt = shop.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin shop: " + ex.Message
                });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateShop([FromForm] Shop model, IFormFile Avatar)
        {
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập!"
                });
            }

            try
            {
                var existingShop = await _shopService.GetShopByUserIdAsync(userId);

                if (existingShop == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy shop!"
                    });
                }

                // Cập nhật thông tin
                existingShop.ShopName = model.ShopName;
                existingShop.Description = model.Description;
                existingShop.BusinessType = model.BusinessType;
                existingShop.Address = model.Address;
                existingShop.Email = model.Email;
                existingShop.Phone = model.Phone;
                existingShop.UpdatedAt = DateTime.UtcNow;

                // Upload ảnh mới nếu có
                if (Avatar != null && Avatar.Length > 0)
                {
                    existingShop.ShopImage = await _cloudinaryService.UploadImageAsync(Avatar);
                }

                await _shopService.UpdateShopAsync(existingShop);

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật shop thành công!",
                    data = existingShop
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật shop: " + ex.Message
                });
            }
        }

    }
}
