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
    public class CheckoutController : BaseController
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
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly ReturnRequestService _returnRequestService;

        public CheckoutController(CartService cartService, OrderService orderService,
    VoucherClientService voucherClientService, CategoryProduct_adminService categoryProduct_AdminService,
    ReviewService reviewService, UserService userService, CloudinaryService cloudinaryService,
    ILogger<CheckoutController> logger, ShopService shopService, IConversationService conversationService, IMessageService messageService, ReturnRequestService returnRequestService)
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
            _conversationService = conversationService;
            _messageService = messageService;
            _returnRequestService = returnRequestService;
        }


        [HttpGet]
        public IActionResult PaymentInfo()
        {
            var itemsJson = HttpContext.Session.GetString("CheckoutItems");
            if (itemsJson == null)
                return RedirectToAction("Cart");

            var items = JsonConvert.DeserializeObject<List<CartItem>>(itemsJson);

            decimal totalAmount = decimal.Parse(HttpContext.Session.GetString("TotalAmount"));
            decimal discountAmount = decimal.Parse(HttpContext.Session.GetString("DiscountAmount"));
            decimal finalAmount = decimal.Parse(HttpContext.Session.GetString("FinalAmount"));

            return View(new PaymentInfoViewModel
            {
                Items = items,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount
            });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(PaymentInfoViewModel model)
        {
            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
                return RedirectToAction("Sign_in", "User");

            var userId = HttpContext.Session.GetString("UserId");

            var itemsJson = HttpContext.Session.GetString("CheckoutItems");
            if (itemsJson == null)
                return RedirectToAction("Cart");

            var items = JsonConvert.DeserializeObject<List<CartItem>>(itemsJson);

            decimal totalAmount = decimal.Parse(HttpContext.Session.GetString("TotalAmount"));
            decimal discountAmount = decimal.Parse(HttpContext.Session.GetString("DiscountAmount"));
            decimal finalAmount = decimal.Parse(HttpContext.Session.GetString("FinalAmount"));

            // Nếu có voucher thì cập nhật số lần dùng
            string voucherId = HttpContext.Session.GetString("SelectedVoucherId");
            if (!string.IsNullOrEmpty(voucherId))
            {
                var voucher = await _voucherClientService.GetVoucherByIdAsync(voucherId);
                if (voucher != null)
                {
                    voucher.UsageCount++;
                    await _voucherClientService.UpdateVoucherUsageCountAsync(voucher);
                }
            }

            // Tạo đơn hàng
            var order = new Order
            {
                UserId = userId,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Items = items,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Status = "Chờ xác nhận",
                CreatedAt = DateTime.UtcNow
            };

            await SaveOrderAndUpdateStockAsync(order, items);

            // Clear session
            HttpContext.Session.Remove("CheckoutItems");
            HttpContext.Session.Remove("SelectedVoucher");
            HttpContext.Session.Remove("SelectedVoucherId");

            HttpContext.Session.Remove("TotalAmount");
            HttpContext.Session.Remove("DiscountAmount");
            HttpContext.Session.Remove("FinalAmount");

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
        [HttpGet]
        public async Task<IActionResult> ContactSeller(string orderId)
        {
            var buyerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(buyerId))
                return RedirectToAction("Sign_in", "User");

            // 1️⃣ LẤY ORDER
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found");

            // 2️⃣ LẤY SELLER + SHOP
            var firstItem = order.Items.FirstOrDefault();
            if (firstItem == null)
                return BadRequest("Order has no items");

            var sellerId = firstItem.SellerId;

            // Lấy shop theo sellerId (đúng DB bạn)
            var shop = await _shopService.GetShopByUserIdAsync(sellerId);
            if (shop == null)
                return BadRequest("Shop not found");

            var conversation = await _conversationService.GetOrCreateAsync(
                buyerId: buyerId,
                sellerId: sellerId,
                shopId: shop.Id
            );

            // 6️⃣ Gửi system message gắn Order (Shopee-style)
            await _messageService.SaveSystemAsync(
                conversationId: conversation.Id,
                content: $"📦 Trao đổi về đơn hàng #{order.Id}",
                orderId: order.Id.ToString()   
            );

            // 5️⃣ REDIRECT SANG CHAT + AUTO OPEN
            return RedirectToAction("Index", "Chat", new
            {
                mode = "buyer",
                conversationId = conversation.Id
            });
        }



        [HttpGet]
        public async Task<IActionResult> ReturnReason(string orderId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            if (string.IsNullOrEmpty(orderId))
            {
                return RedirectToAction("PaymentHistory");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null || order.UserId != userId)
            {
                return NotFound("Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.");
            }

            var existingRequest = await _returnRequestService.GetByOrderIdAsync(orderId);

            if (existingRequest != null)
            {
                ViewBag.OrderInfo = order;

                return View("ReturnDetails", existingRequest);
            }

            if (order.Status != "Đã giao")
            {
                TempData["ErrorMessage"] = "Chỉ có thể yêu cầu trả hàng cho đơn đã giao thành công.";
                return RedirectToAction("PaymentHistory");
            }

            return View(order);
        }
        [HttpPost]
        [RequestSizeLimit(104857600)] // 100MB
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)] 
        public async Task<IActionResult> SubmitReturnRequest(string orderId, string reason, List<IFormFile> mediaFiles)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Sign_in", "User");

            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction("PaymentHistory");
                }

                var exists = await _returnRequestService.HasReturnRequestAsync(orderId);
                if (exists)
                {
                    TempData["ErrorMessage"] = "Bạn đã gửi yêu cầu cho đơn này rồi.";
                    return RedirectToAction("PaymentHistory");
                }

                List<string> uploadedUrls = new List<string>();
                if (mediaFiles != null && mediaFiles.Count > 0)
                {
                    foreach (var file in mediaFiles)
                    {
                        if (file.Length > 0)
                        {
                            var url = await _cloudinaryService.UploadMediaAsync(file);
                            if (!string.IsNullOrEmpty(url)) uploadedUrls.Add(url);
                        }
                    }
                }

                var returnRequest = new ReturnRequest
                {
                    OrderId = orderId,
                    UserId = userId,
                    Reason = reason,
                    MediaUrls = uploadedUrls,
                    CreatedAt = DateTime.UtcNow
                };

                await _returnRequestService.CreateAsync(returnRequest);

                order.Status = "Trả hàng/Hoàn tiền";
                await _orderService.SaveOrderAsync(order);

                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        var product = await _categoryProductAdminService.GetProductByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                            await _categoryProductAdminService.UpdateProductAsync(product);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Gửi yêu cầu trả hàng thành công!";
                return RedirectToAction("PaymentHistory");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("PaymentHistory");
            }
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
        [HttpGet]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order != null && order.UserId == userId && order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã hủy";
                await _orderService.SaveOrderAsync(order);

                if (order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        var product = await _categoryProductAdminService.GetProductByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Stock += item.Quantity;
                            await _categoryProductAdminService.UpdateProductAsync(product);
                        }
                    }
                }

                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng này (Đã được xử lý hoặc không tồn tại).";
            }

            return RedirectToAction("PaymentHistory");
        }
    }
}