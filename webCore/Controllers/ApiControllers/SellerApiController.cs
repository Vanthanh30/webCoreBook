using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    [Route("api/seller")]
    [ApiController]
    public class SellerApiController : ControllerBase
    {
        private readonly ShopService _shopService;
        public SellerApiController(ShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet("check-shop")]
        public async Task<IActionResult> CheckShop()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Chưa đăng nhập" });

            var shop = await _shopService.GetShopByUserIdAsync(userId);

            return Ok(new
            {
                success = true,
                hasShop = shop != null,
                shopId = shop?.Id
            });
        }

    }
}
