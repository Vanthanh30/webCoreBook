using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.Controllers.ApiControllers
{
    [Route("api/product")]
    [ApiController]
    public class SellerApiProductController : ControllerBase
    {
        private readonly CategoryProduct_adminService _categoryProductCollection;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<SellerProductController> _logger;

        public SellerApiProductController(CategoryProduct_adminService category_adminService, CloudinaryService cloudinaryService, ILogger<SellerProductController> logger)
        {
            _categoryProductCollection = category_adminService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        [HttpGet("create-data")]
        public async Task<IActionResult> GetCreateData()
        {
            var categories = await _categoryProductCollection.GetCategory();
            var products = await _categoryProductCollection.GetProduct();

            return Ok(new
            {
                success = true,
                categories,
                products
            });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct(
            [FromForm] Product_admin product,
            IFormFile Image,
            [FromForm] string categoryId)
        {
            //  Lấy Seller ID từ Session
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập. Không thể tạo sản phẩm."
                });
            }

            // Gán SellerId vào sản phẩm
            product.SellerId = sellerId;

            // Kiểm tra trùng tên sản phẩm của CHÍNH seller đó
            var existingProduct = (await _categoryProductCollection.GetProduct())
                .FirstOrDefault(a => a.Title == product.Title && a.SellerId == sellerId);

            if (existingProduct != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Shop của bạn đã có sản phẩm này!"
                });
            }

            //  Generate ID
            product.Id = Guid.NewGuid().ToString();
            product.CategoryId = categoryId;

            //  Lấy Category Title
            var category = await _categoryProductCollection.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Danh mục không hợp lệ!"
                });
            }

            product.CategoryTitle = category.Title;

            //  Position
            var products = await _categoryProductCollection.GetProduct();
            int maxPosition = products.Any() ? products.Max(c => c.Position) : 0;
            product.Position = maxPosition + 1;

            //  Upload Image
            if (Image != null && Image.Length > 0)
            {
                try
                {
                    product.Image = await _cloudinaryService.UploadImageAsync(Image);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image");

                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thể upload ảnh. Vui lòng thử lại."
                    });
                }
            }

            //  Save product
            try
            {
                await _categoryProductCollection.SaveProductAsync(product);

                return Ok(new
                {
                    success = true,
                    message = "Tạo sản phẩm thành công!",
                    data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving product");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể lưu sản phẩm vào MongoDB."
                });
            }
        }

    }
}
