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
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập. Không thể tạo sản phẩm."
                });
            }

            product.SellerId = sellerId;

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

            product.Id = Guid.NewGuid().ToString();
            product.CategoryId = categoryId;

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

            var products = await _categoryProductCollection.GetProduct();
            int maxPosition = products.Any() ? products.Max(c => c.Position) : 0;
            product.Position = maxPosition + 1;

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

        [HttpGet("list")]
        public async Task<IActionResult> GetSellerProducts()
        {
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập."
                });
            }

            try
            {
                var allProducts = await _categoryProductCollection.GetProduct();

                var sellerProducts = allProducts
                    .Where(p => p.SellerId == sellerId && !p.Deleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        id = p.Id,
                        title = p.Title,
                        image = p.Image,
                        categoryTitle = p.CategoryTitle,
                        price = p.Price,
                        discountPercentage = p.DiscountPercentage,
                        stock = p.Stock,
                        status = p.Status,
                        featured = p.Featured,
                        position = p.Position,
                        createdAt = p.CreatedAt,
                        updatedAt = p.UpdatedAt
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = sellerProducts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seller products");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách sản phẩm."
                });
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> GetProductForEdit(string id)
        {
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập."
                });
            }

            try
            {
                var product = await _categoryProductCollection.GetProductByIdAsync(id);

                if (product == null || product.SellerId != sellerId)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm hoặc bạn không có quyền chỉnh sửa."
                    });
                }

                var categories = await _categoryProductCollection.GetCategory();

                return Ok(new
                {
                    success = true,
                    product = new
                    {
                        id = product.Id,
                        title = product.Title,
                        description = product.Description,
                        image = product.Image,
                        categoryId = product.CategoryId,
                        categoryTitle = product.CategoryTitle,
                        price = product.Price,
                        discountPercentage = product.DiscountPercentage,
                        stock = product.Stock,
                        status = product.Status,
                        featured = product.Featured
                    },
                    categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product for edit");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thông tin sản phẩm."
                });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProduct(
            string id,
            [FromForm] Product_admin product,
            IFormFile Image,
            [FromForm] string categoryId)
        {
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập."
                });
            }

            try
            {
                var existingProduct = await _categoryProductCollection.GetProductByIdAsync(id);

                if (existingProduct == null || existingProduct.SellerId != sellerId)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm hoặc bạn không có quyền chỉnh sửa."
                    });
                }

                var duplicateProduct = (await _categoryProductCollection.GetProduct())
                    .FirstOrDefault(p => p.Title == product.Title
                        && p.SellerId == sellerId
                        && p.Id != id);

                if (duplicateProduct != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Shop của bạn đã có sản phẩm với tên này!"
                    });
                }

                existingProduct.Title = product.Title;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.DiscountPercentage = product.DiscountPercentage;
                existingProduct.Stock = product.Stock;
                existingProduct.Featured = product.Featured;
                existingProduct.Status = product.Status;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(categoryId) && categoryId != existingProduct.CategoryId)
                {
                    var category = await _categoryProductCollection.GetCategoryByIdAsync(categoryId);
                    if (category == null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Danh mục không hợp lệ!"
                        });
                    }
                    existingProduct.CategoryId = categoryId;
                    existingProduct.CategoryTitle = category.Title;
                }

                if (Image != null && Image.Length > 0)
                {
                    try
                    {

                        existingProduct.Image = await _cloudinaryService.UploadImageAsync(Image);
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

                await _categoryProductCollection.UpdateProductAsync(existingProduct);

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật sản phẩm thành công!",
                    data = existingProduct
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật sản phẩm."
                });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập."
                });
            }

            try
            {
                var product = (await _categoryProductCollection.GetProduct())
                    .FirstOrDefault(p => p.Id == id && p.SellerId == sellerId);

                if (product == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm hoặc bạn không có quyền xóa."
                    });
                }

                product.Deleted = true;
                product.UpdatedAt = DateTime.UtcNow;
                await _categoryProductCollection.UpdateProductAsync(product);

                return Ok(new
                {
                    success = true,
                    message = "Đã xóa sản phẩm thành công!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa sản phẩm."
                });
            }
        }
    }
}
