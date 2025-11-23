using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers.ApiControllers
{
    [Route("api/shop")]
    [ApiController]
    public class ShopApiController : Controller
    {
        private readonly ShopService _shopService;
        private readonly UserService _userService;
        private readonly IWebHostEnvironment _env;

        public ShopApiController(ShopService shopService,UserService userService, IWebHostEnvironment env)
        {
            _shopService = shopService;
            _userService = userService;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterShop([FromForm] Shop model, IFormFile Avatar)
        {
            string userId = Request.Headers["UserId"];

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Thiếu UserId trong Header!" });

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { success = false, message = "Không tìm thấy user" });

            // Check user đã có shop
            var existed = await _shopService.GetShopByUserIdAsync(user.Id);
            if (existed != null)
                return BadRequest(new { success = false, message = "User đã có shop!" });

            // Upload ảnh
            string shopImage = "default-image-url";
            if (Avatar != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(Avatar.FileName);
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads/shop");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Avatar.CopyToAsync(stream);
                }

                shopImage = "/uploads/shop/" + fileName;
            }

            // Gán giá trị
            model.UserId = user.Id;
            model.ShopImage = shopImage;

            await _shopService.CreateShopAsync(model);

            // Thêm role seller
            if (!user.RoleId.Contains("seller"))
            {
                user.RoleId.Add("seller");
                await _userService.UpdateUserAsync(user);
            }

            return Ok(new { success = true, message = "Đăng ký shop thành công!", data = model });
        }
    }
}
