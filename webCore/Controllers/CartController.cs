
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly VoucherClientService _voucherService;
        private readonly ProductService _productService;

        public CartController(CartService cartService, VoucherClientService voucherService, ProductService productService)
        {
            _cartService = cartService;
            _voucherService = voucherService;
            _productService = productService;
        }
        // Phương thức tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return PartialView("_ProductList", new List<Product_admin>());
            }

            // Tìm kiếm sản phẩm từ MongoDB
            var allProducts = await _productService.GetProductsAsync();
            var searchResults = allProducts
                .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_ProductList", searchResults);
        }
        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(
            string productId,
            string title,
            decimal price,
            decimal discountpercentage,
            int quantity,
            string image)
        {
            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào giỏ hàng." });
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Không lấy được UserId. Vui lòng đăng nhập lại." });
            }

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
            }

            string sellerId = product.SellerId;
            if (product.SellerId == userId)
                return Json(new
                {
                    success = false,
                    message = "Bạn không thể thêm giỏ hàng sản phẩm của shop mình."
                });

            var cart = await _cartService.GetCartByUserIdAsync(userId)
                       ?? new Cart { UserId = userId };

            var existingItem = cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    SellerId = sellerId,
                    Title = title,
                    Price = price,
                    DiscountPercentage = discountpercentage,
                    Quantity = quantity,
                    Image = image
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;

            await _cartService.AddOrUpdateCartAsync(cart);

            return Json(new
            {
                success = true,
                itemCount = cart.Items.Count,
                message = "Sản phẩm đã được thêm vào giỏ hàng!"
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return Json(new { itemCount = 0 });
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { itemCount = 0 });
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);

            int itemCount = cart?.Items.Count ?? 0;

            return Json(new { itemCount = itemCount });
        }


        [ServiceFilter(typeof(SetLoginStatusFilter))]
        // Hiển thị giỏ hàng của người dùng
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            // Kiểm tra trạng thái đăng nhập từ session
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            // 2️⃣ LẤY USERID ĐỂ LOAD CART
            var userId = HttpContext.Session.GetString("UserId");

            // Lấy giỏ hàng của người dùng từ dịch vụ
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            // Kiểm tra xem giỏ hàng có tồn tại và có sản phẩm hay không
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                // Nếu giỏ hàng rỗng, trả về view với danh sách trống
                return View(new List<CartItem>());
            }

            // Lấy thông tin voucher từ session
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");

            // Lấy danh sách các sản phẩm đã chọn từ session (List<string>)
            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");

            // Lọc ra các sản phẩm đã chọn trong giỏ hàng để tính tổng tiền
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId.ToString())).ToList();

            // Tính toán tổng tiền cho các sản phẩm đã chọn
            decimal totalAmount = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);
            totalAmount = Math.Round(totalAmount, 2); // Làm tròn đến 2 chữ số sau dấu thập phân
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(voucherDiscount))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmount * (discountValue / 100);
            }


            decimal finalAmount = totalAmount - discountAmount;

            // Cập nhật các giá trị cần hiển thị vào ViewData
            ViewData["VoucherDiscount"] = voucherDiscount;  // Voucher giảm giá
            ViewData["TotalAmount"] = totalAmount;           // Tổng tiền trước giảm giá
            ViewData["FinalAmount"] = finalAmount;           // Tổng tiền sau giảm giá
            ViewData["SelectedProductIds"] = selectedProductIds; // Danh sách các sản phẩm đã chọn

            // Trả về danh sách tất cả các sản phẩm trong giỏ hàng
            return View(cart.Items); // Trả về tất cả các sản phẩm trong giỏ
        }


        [HttpPost]
        public IActionResult SaveSelectedProducts([FromBody] List<string> selectedProductIds)
        {
            // Kiểm tra xem dữ liệu có được nhận hay không
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                // Nếu không có sản phẩm nào được chọn, có thể ghi log hoặc trả về lỗi
                return Json(new { success = false, message = "No products selected" });
            }

            // Lưu danh sách sản phẩm đã chọn vào session
            HttpContext.Session.SetString("CheckoutMode", "cart");
            HttpContext.Session.SetString("SelectedProductIds", JsonConvert.SerializeObject(selectedProductIds));

            // Trả về JSON xác nhận thành công
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            // Lấy UserId từ session
            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");

            // Lấy giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            // Tìm và xóa sản phẩm trong giỏ hàng
            var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
            }
            else
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            // Cập nhật giỏ hàng vào MongoDB
            await _cartService.AddOrUpdateCartAsync(cart);

            // Trả về kết quả thành công
            return Json(new { success = true });
        }



        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<string> productIds)
        {
            // Lấy userId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để xóa sản phẩm." });
            }

            if (productIds == null || productIds.Count == 0)
            {
                return Json(new { success = false, message = "Chọn sản phẩm cần xóa." });
            }

            bool deleted = await _cartService.RemoveMutipleItemsFromCartAsync(userId, productIds);

            return Json(new { success = deleted });
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("UserId")?.Value;
        }
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        // Hàm cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(string productId, int quantity)
        {

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");
            // Lấy giỏ hàng của người dùng
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            // Tìm sản phẩm trong giỏ hàng
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                // Cập nhật số lượng
                item.Quantity = quantity;
                cart.UpdatedAt = DateTime.UtcNow;

                // Lưu giỏ hàng cập nhật vào MongoDB
                await _cartService.AddOrUpdateCartAsync(cart);

                // Trả về kết quả thành công
                return Json(new { success = true, message = "Số lượng sản phẩm đã được cập nhật." });
            }

            return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
        }
        [ServiceFilter(typeof(SetLoginStatusFilter))]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
                return RedirectToAction("Sign_in", "User");

            var userId = HttpContext.Session.GetString("UserId");

            var mode = HttpContext.Session.GetString("CheckoutMode");
            List<CartItem> selectedItems = new List<CartItem>();
            decimal totalAmount = 0;

            if (mode == "cart")
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);

                if (cart == null || cart.Items == null || cart.Items.Count == 0)
                    return RedirectToAction("Cart");

                // Lấy danh sách ID đã chọn
                var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(
                    HttpContext.Session.GetString("SelectedProductIds") ?? "[]"
                );

                selectedItems = cart.Items
                    .Where(i => selectedProductIds.Contains(i.ProductId))
                    .ToList();

                totalAmount = selectedItems.Sum(i =>
                    i.Price * (1 - i.DiscountPercentage / 100) * i.Quantity
                );
            }

            else if (mode == "buynow")
            {
                var json = HttpContext.Session.GetString("BuyNowItem");
                if (json == null)
                    return RedirectToAction("Cart");

                var tempItem = JsonConvert.DeserializeObject<CartItem>(json);
                selectedItems.Add(tempItem);

                totalAmount =
                    tempItem.Price *
                    (1 - tempItem.DiscountPercentage / 100) *
                    tempItem.Quantity;
            }

            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(voucherDiscount))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmount * (discountValue / 100);
            }

            decimal finalAmount = totalAmount - discountAmount;
            return View(new CheckoutViewModel
            {
                Items = selectedItems,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                VoucherDiscount = voucherDiscount
            });
        }

        [ServiceFilter(typeof(SetLoginStatusFilter))]

        [HttpGet]
        public async Task<IActionResult> BuyNow(string productId, int quantity)
        {
            HttpContext.Session.Remove("CartItem");

            // Kiểm tra trạng thái đăng nhập từ session
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }
            Console.WriteLine($"Product ID: {productId}");
            // Lấy sản phẩm từ dịch vụ theo productId
            var product = await _productService.GetProductByIdAsync(productId);

            // Kiểm tra xem sản phẩm có tồn tại không
            if (product == null)
            {
                // Nếu không có sản phẩm, chuyển hướng hoặc hiển thị thông báo lỗi
                return NotFound("Sản phẩm không tồn tại.");
            }
            var userId = HttpContext.Session.GetString("UserId");
            if (product.SellerId == userId)
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn không thể mua sản phẩm của shop mình."
                });
            }
            // Kiểm tra thông tin voucher từ session
            var voucherDiscount = "0";

            // Tính toán tổng tiền cho sản phẩm đã chọn
            decimal totalAmount = product.Price * quantity * (1 - product.DiscountPercentage / 100);
            decimal finalAmount = totalAmount;

            // Cập nhật các giá trị cần hiển thị vào ViewData
            ViewData["VoucherDiscount"] = voucherDiscount;  // Voucher giảm giá
            ViewData["TotalAmount"] = totalAmount;           // Tổng tiền trước giảm giá
            ViewData["FinalAmount"] = finalAmount;           // Tổng tiền sau giảm giá
            if (product.SellerId == userId)
                return Json(new
                {
                    success = false,
                    message = "Bạn không thể thêm giỏ hàng sản phẩm của shop mình."
                });

            var tempItem = new CartItem
            {
                ProductId = product.Id,
                SellerId = product.SellerId,
            Title = product.Title,
                Price = product.Price,
                DiscountPercentage = product.DiscountPercentage,
                Quantity = quantity,
                Image = product.Image
            };

            // Lưu vào session
            HttpContext.Session.SetString("CheckoutMode", "buynow");
            HttpContext.Session.SetString("BuyNowItem", JsonConvert.SerializeObject(tempItem));

            // Chuyển sang checkout
            return RedirectToAction("Checkout");
        }

    }
}
