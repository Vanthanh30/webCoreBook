
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
        public async Task<IActionResult> Search(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return PartialView("_ProductList", new List<Product_admin>());
            }

            var allProducts = await _productService.GetProductsAsync();
            var searchResults = allProducts
                .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_ProductList", searchResults);
        }
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
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            ViewBag.IsLoggedIn = isLoggedIn;

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                return View(new List<CartItem>());
            }

            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");

            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");

            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId.ToString())).ToList();

            decimal totalAmount = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);
            totalAmount = Math.Round(totalAmount, 2); 
            decimal discountAmount = 0;

            if (!string.IsNullOrEmpty(voucherDiscount))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmount * (discountValue / 100);
            }


            decimal finalAmount = totalAmount - discountAmount;

            ViewData["VoucherDiscount"] = voucherDiscount; 
            ViewData["TotalAmount"] = totalAmount;           
            ViewData["FinalAmount"] = finalAmount;          
            ViewData["SelectedProductIds"] = selectedProductIds; 

            return View(cart.Items); 
        }


        [HttpPost]
        public IActionResult SaveSelectedProducts([FromBody] List<string> selectedProductIds)
        {
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                HttpContext.Session.Remove("SelectedProductIds");
                return Json(new { success = false, message = "No products selected" });
            }

            HttpContext.Session.SetString("CheckoutMode", "cart");
            HttpContext.Session.SetString("SelectedProductIds", JsonConvert.SerializeObject(selectedProductIds));

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string productId)
        {
            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            var itemToRemove = cart.Items.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
            }
            else
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            await _cartService.AddOrUpdateCartAsync(cart);

            return Json(new { success = true });
        }



        [HttpPost]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<string> productIds)
        {
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
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(string productId, int quantity)
        {

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Giỏ hàng không tồn tại." });
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                cart.UpdatedAt = DateTime.UtcNow;

                await _cartService.AddOrUpdateCartAsync(cart);

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
            HttpContext.Session.SetString("FinalAmount", finalAmount.ToString());
            HttpContext.Session.SetString("TotalAmount", totalAmount.ToString());
            HttpContext.Session.SetString("DiscountAmount", discountAmount.ToString());

            HttpContext.Session.SetString("CheckoutItems",JsonConvert.SerializeObject(selectedItems));

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


            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            ViewBag.IsLoggedIn = isLoggedIn;

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            Console.WriteLine($"Product ID: {productId}");
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {
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
            var voucherDiscount = "0";

            decimal totalAmount = product.Price * quantity * (1 - product.DiscountPercentage / 100);
            decimal finalAmount = totalAmount;

            ViewData["VoucherDiscount"] = voucherDiscount;  
            ViewData["TotalAmount"] = totalAmount;          
            ViewData["FinalAmount"] = finalAmount;          
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

            HttpContext.Session.SetString("CheckoutMode", "buynow");
            HttpContext.Session.SetString("BuyNowItem", JsonConvert.SerializeObject(tempItem));

            return RedirectToAction("Checkout");
        }

    }
}
