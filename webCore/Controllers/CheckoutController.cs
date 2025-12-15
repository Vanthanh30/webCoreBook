using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly VoucherClientService _voucherClientService;
        private readonly CategoryProduct_adminService _categoryProductAdminService;
        private readonly ReviewService _reviewService;
        private readonly UserService _userService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<CheckoutController> _logger;
        private readonly ShopService _shopService;

        public CheckoutController(CartService cartService, OrderService orderService,
    VoucherClientService voucherClientService, CategoryProduct_adminService categoryProduct_AdminService,
    ReviewService reviewService, UserService userService, CloudinaryService cloudinaryService,
    ILogger<CheckoutController> logger, ShopService shopService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _voucherClientService = voucherClientService;
            _categoryProductAdminService = categoryProduct_AdminService;
            _reviewService = reviewService;
            _userService = userService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
            _shopService = shopService;
        }


        [HttpGet]
        public async Task<IActionResult> PaymentInfo()
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;
            ViewBag.IsLoggedIn = isLoggedIn;

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");

            var cartItemSession = HttpContext.Session.GetString("CartItem");

            if (!string.IsNullOrEmpty(cartItemSession))
            {
                var cartItem = JsonConvert.DeserializeObject<CartItem>(cartItemSession);

                decimal totalAmount = cartItem.Price * cartItem.Quantity * (1 - cartItem.DiscountPercentage / 100);
                decimal finalAmount = totalAmount;

                return View(new PaymentInfoViewModel
                {
                    Items = new List<CartItem> { cartItem },
                    TotalAmount = totalAmount,
                    FinalAmount = finalAmount
                });
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId)).ToList();

            decimal totalAmountCheckout = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);
            decimal finalAmountCheckout = totalAmountCheckout;

            return View(new PaymentInfoViewModel
            {
                Items = selectedItems,
                TotalAmount = totalAmountCheckout,
                FinalAmount = finalAmountCheckout
            });
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentInfoViewModel model)
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;
            ViewBag.IsLoggedIn = isLoggedIn;

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");

            var cartItemSession = HttpContext.Session.GetString("CartItem");
            if (!string.IsNullOrEmpty(cartItemSession))
            {
                var cartItem = JsonConvert.DeserializeObject<CartItem>(cartItemSession);

                decimal totalAmount = cartItem.Price * cartItem.Quantity * (1 - cartItem.DiscountPercentage / 100);
                decimal finalAmount = totalAmount;

                var buyNowOrder = new Order
                {
                    UserId = userId,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Items = new List<CartItem> { cartItem },
                    TotalAmount = totalAmount,
                    DiscountAmount = 0,
                    FinalAmount = finalAmount,
                    Status = "Chờ xác nhận",
                    CreatedAt = DateTime.UtcNow
                };

                await SaveOrderAndUpdateStockAsync(buyNowOrder, new List<CartItem> { cartItem });

                HttpContext.Session.Remove("CartItem");
                return RedirectToAction("PaymentHistory", "Checkout");
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Items == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Cart");
            }

            var selectedProductIds = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString("SelectedProductIds") ?? "[]");
            var selectedItems = cart.Items.Where(item => selectedProductIds.Contains(item.ProductId)).ToList();
            if (!selectedItems.Any())
            {
                return RedirectToAction("Cart");
            }

            decimal totalAmountCheckout = selectedItems.Sum(item => (item.Price * (1 - item.DiscountPercentage / 100)) * item.Quantity);
            decimal discountAmount = 0;

            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");
            string voucherId = HttpContext.Session.GetString("SelectedVoucherId");
            if (!string.IsNullOrEmpty(voucherDiscount) && !string.IsNullOrEmpty(voucherId))
            {
                decimal discountValue = decimal.Parse(voucherDiscount);
                discountAmount = totalAmountCheckout * (discountValue / 100);

                var voucher = await _voucherClientService.GetVoucherByIdAsync(voucherId);
                if (voucher != null)
                {
                    voucher.UsageCount += 1;
                    await _voucherClientService.UpdateVoucherUsageCountAsync(voucher);
                }
            }

            decimal finalAmountCheckout = totalAmountCheckout - discountAmount;

            var checkoutOrder = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Items = selectedItems,
                TotalAmount = totalAmountCheckout,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmountCheckout,
                Status = "Chờ xác nhận",
                CreatedAt = DateTime.UtcNow
            };

            await SaveOrderAndUpdateStockAsync(checkoutOrder, selectedItems);

            HttpContext.Session.Remove("SelectedProductIds");
            HttpContext.Session.Remove("SelectedVoucher");

            return RedirectToAction("PaymentHistory", "Checkout");
        }
        private async Task SaveOrderAndUpdateStockAsync(Order order, List<CartItem> items)
        {
            await _orderService.SaveOrderAsync(order);

            foreach (var item in items)
            {
                var product = await _categoryProductAdminService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    await _categoryProductAdminService.UpdateProductAsync(product);
                }
            }

            if (!string.IsNullOrEmpty(order.UserId))
            {
                await _cartService.RemoveItemsFromCartAsync(order.UserId, items.Select(i => i.ProductId).ToList());
            }
        }


        [HttpGet]
        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> PaymentHistory(string? status = null)
        {
            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            ViewBag.IsLoggedIn = isLoggedIn;

            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("Sign_in", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);

            if (orders == null || !orders.Any())
                return View(new List<Order>());

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                orders = orders.Where(o => o.Status == status).ToList();
            }

            ViewBag.CurrentStatus = status ?? "Tất cả";

            return View(orders.OrderByDescending(o => o.CreatedAt).ToList());
        }

        [HttpGet]
        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> OrderDetails(string orderId)
        {

            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            if (!isLoggedIn)
            {
                return RedirectToAction("Sign_in", "User");
            }


            ViewBag.IsLoggedIn = isLoggedIn;
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng");
            }

            return View(order);
        }
        public async Task<IActionResult> ContactSeller(string orderId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            var order = await _orderService.GetOrderByIdAsync(orderId);
            string sellerId = order.Items.First().SellerId;

            return RedirectToAction("Index", "Chat", new
            {
                orderId = orderId,
                buyerId = userId,
                sellerId = sellerId
            });
        }


        public IActionResult ReturnReason()
        {
            return View(); 
        }

        [HttpGet]
        public async Task<IActionResult> ReviewProduct(string orderId, string productId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Sign_in", "User");

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound("Không tìm thấy đơn hàng");

            var item = order.Items.FirstOrDefault(p => p.ProductId == productId);
            if (item == null) return NotFound("Sản phẩm không tồn tại trong đơn hàng này");

            var hasReviewed = await _reviewService.HasReviewedAsync(orderId, productId);
            if (hasReviewed)
            {
                TempData["ErrorMessage"] = "Bạn đã đánh giá sản phẩm này rồi.";
                return RedirectToAction("PaymentHistory");
            }

            decimal orderTotalCalculated = order.Items.Sum(i => i.Price * i.Quantity * (1 - i.DiscountPercentage / 100m));

            decimal itemTotalCalculated = item.Price * item.Quantity * (1 - item.DiscountPercentage / 100m);

            decimal ratio = (orderTotalCalculated == 0) ? 0 : (itemTotalCalculated / orderTotalCalculated);

            decimal actualPaidTotal = order.FinalAmount * ratio;

            ViewBag.OrderId = orderId;
            ViewBag.ActualPaidTotal = actualPaidTotal;

            return View(item);
        }
        [HttpPost]
        [RequestSizeLimit(104857600)] // 100MB
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        public async Task<IActionResult> SubmitReview(string orderId, string productId, int qualityRating, int serviceRating, string comment, List<IFormFile> mediaFiles)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Sign_in", "User");

            try
            {
                _logger.LogInformation($"Starting review submission for Order: {orderId}, Product: {productId}");

                List<string> uploadedUrls = new List<string>();
                if (mediaFiles != null && mediaFiles.Count > 0)
                {
                    _logger.LogInformation($"Processing {mediaFiles.Count} media files");

                    foreach (var file in mediaFiles)
                    {
                        if (file.Length > 0)
                        {
                            _logger.LogInformation($"Uploading file: {file.FileName}, Size: {file.Length} bytes");

                            var url = await _cloudinaryService.UploadMediaAsync(file);

                            if (!string.IsNullOrEmpty(url))
                            {
                                uploadedUrls.Add(url);
                                _logger.LogInformation($"File uploaded successfully: {url}");
                            }
                            else
                            {
                                _logger.LogWarning($"Failed to upload file: {file.FileName}");
                            }
                        }
                    }

                    _logger.LogInformation($"Successfully uploaded {uploadedUrls.Count} files");
                }

                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogError($"Order not found: {orderId}");
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("PaymentHistory");
                }

                var item = order.Items.FirstOrDefault(p => p.ProductId == productId);
                if (item == null)
                {
                    _logger.LogError($"Product not found in order: {productId}");
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm";
                    return RedirectToAction("PaymentHistory");
                }

                var currentUser = await _userService.GetUserByIdAsync(userId);
                var shop = await _shopService.GetShopByUserIdAsync(item.SellerId);
                string shopId = shop?.Id;

                var review = new Review
                {
                    OrderId = orderId,
                    ShopId = shopId,
                    ProductId = productId,
                    UserId = userId,
                    UserName = currentUser?.Name ?? "Khách hàng",
                    UserAvatar = !string.IsNullOrEmpty(currentUser?.ProfileImage)
                                 ? currentUser.ProfileImage
                                 : "default-image-url",
                    ProductTitle = item.Title,
                    ProductImage = item.Image,
                    QualityRating = qualityRating,
                    ServiceRating = serviceRating,
                    Comment = comment,
                    MediaUrls = uploadedUrls,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation($"Saving review to database with {uploadedUrls.Count} media URLs");

                await _reviewService.CreateAsync(review);

                _logger.LogInformation("Review saved successfully");

                TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
                return RedirectToAction("PaymentHistory");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SubmitReview: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("PaymentHistory");
            }
        }
    }
}