
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
        public async Task<IActionResult> AddToCart(string productId, string title, decimal price, decimal discountpercentage, int quantity, string image)
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, trả về thông báo lỗi
                return Json(new { success = false, message = "Bạn cần đăng nhập để thêm vào giỏ hàng." });
            }

            // Lấy giỏ hàng từ dịch vụ hoặc tạo mới nếu chưa tồn tại
            var cart = await _cartService.GetCartByUserIdAsync(userId) ?? new Cart { UserId = userId };
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ, tăng số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                // Nếu chưa có, thêm mới sản phẩm vào giỏ
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Title = title,
                    Price = price,
                    DiscountPercentage = discountpercentage,
                    Quantity = quantity,
                    Image = image
                });
            }

            // Cập nhật thời gian sửa đổi giỏ hàng
            cart.UpdatedAt = DateTime.UtcNow;

            // Lưu giỏ hàng vào MongoDB
            await _cartService.AddOrUpdateCartAsync(cart);
            int itemCount = cart.Items.Count;
            // Trả về kết quả thành công
            return Json(new { success = true, itemCount = itemCount, message = "Sản phẩm đã được thêm vào giỏ hàng!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            // Lấy userId từ session hoặc claim
            var userId = HttpContext.Session.GetString("UserToken");  // Giả sử bạn lưu token trong Session sau khi đăng nhập

            // Nếu userId không tồn tại (người dùng chưa đăng nhập)
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { itemCount = 0 });
            }

            // Lấy giỏ hàng của người dùng từ database
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            // Kiểm tra giỏ hàng và lấy số lượng sản phẩm
            int itemCount = cart?.Items.Count ?? 0; // Nếu không có giỏ hàng thì trả về 0

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
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

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

            // In ra danh sách các sản phẩm đã chọn (logging)
            Console.WriteLine("Products selected: " + string.Join(", ", selectedProductIds));

            // Lưu danh sách sản phẩm đã chọn vào session
            HttpContext.Session.SetString("SelectedProductIds", JsonConvert.SerializeObject(selectedProductIds));

            // Trả về JSON xác nhận thành công
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, trả về thông báo lỗi
                return Json(new { success = false, message = "Bạn cần đăng nhập để xóa sản phẩm." });
            }

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
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, trả về thông báo lỗi
                return Json(new { success = false, message = "Bạn cần đăng nhập để cập nhật số lượng." });
            }

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
            HttpContext.Session.Remove("CartItem");
            // Kiểm tra trạng thái đăng nhập từ session
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;
            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Sign_in", "User");
            }

            // Lấy giỏ hàng của người dùng từ dịch vụ
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                // Nếu giỏ hàng rỗng, trả về trang giỏ hàng
                return RedirectToAction("Cart");
            }

            // Lấy thông tin voucher từ session
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");

            // Lấy danh sách các sản phẩm đã chọn từ session
            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");

            // Lọc ra các sản phẩm đã chọn trong giỏ hàng để tính tổng tiền
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId)).ToList();

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

            // Trả về view thanh toán và truyền thông tin vào view
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

            // Lấy UserId từ session
            var userId = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userId))
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

            // Kiểm tra thông tin voucher từ session
            var voucherDiscount = "0";

            // Tính toán tổng tiền cho sản phẩm đã chọn
            decimal totalAmount = product.Price * quantity * (1 - product.DiscountPercentage / 100);
            decimal finalAmount = totalAmount;

            // Cập nhật các giá trị cần hiển thị vào ViewData
            ViewData["VoucherDiscount"] = voucherDiscount;  // Voucher giảm giá
            ViewData["TotalAmount"] = totalAmount;           // Tổng tiền trước giảm giá
            ViewData["FinalAmount"] = finalAmount;           // Tổng tiền sau giảm giá

            // Chuyển sản phẩm từ Product_admin sang CartItem
            var cartItem = new CartItem
            {
                ProductId = product.Id,
                Title = product.Title,
                Price = product.Price,
                DiscountPercentage = product.DiscountPercentage,
                Quantity = quantity, // Giả sử mua 1 sản phẩm
                Image = product.Image // Nếu có
            };

            // Lưu thông tin vào session để chuyển sang trang thanh toán
            HttpContext.Session.SetString("CartItem", JsonConvert.SerializeObject(cartItem));

            // Trả về view thanh toán và truyền thông tin vào view
            return View("Checkout", new CheckoutViewModel
            {
                // Dùng List<CartItem> với 1 item duy nhất
                Items = new List<CartItem> { cartItem },  // Danh sách CartItem với một sản phẩm được chọn
                TotalAmount = totalAmount,
                DiscountAmount = 0,  // Giảm giá (nếu có)
                FinalAmount = finalAmount,
                VoucherDiscount = voucherDiscount
            });
        }

    }
}
