using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public CheckoutController(CartService cartService, OrderService orderService, VoucherClientService voucherClientService, CategoryProduct_adminService categoryProduct_AdminService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _voucherClientService = voucherClientService;
            _categoryProductAdminService = categoryProduct_AdminService;
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
            // Lưu đơn hàng vào MongoDB
            await _orderService.SaveOrderAsync(order);

            // Cập nhật số lượng tồn kho
            foreach (var item in items)
            {
                var product = await _categoryProductAdminService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    await _categoryProductAdminService.UpdateProductAsync(product);
                }
            }

            // Nếu là giỏ hàng, xóa các sản phẩm đã mua
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

            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
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

            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                orders = orders.Where(o => o.Status == status).ToList();
            }

            // Gửi trạng thái hiện tại để hiển thị active nút lọc
            ViewBag.CurrentStatus = status ?? "Tất cả";

            // Sắp xếp đơn hàng mới nhất lên đầu
            return View(orders.OrderByDescending(o => o.CreatedAt).ToList());
        }

        [HttpGet]
        [ServiceFilter(typeof(SetLoginStatusFilter))]
        public async Task<IActionResult> OrderDetails(string orderId)
        {

            var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

            // Kiểm tra đăng nhập
            if (!isLoggedIn)
            {
                return RedirectToAction("Sign_in", "User");
            }


            // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
            ViewBag.IsLoggedIn = isLoggedIn;
            // Tìm đơn hàng theo ID
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                // Xử lý nếu không tìm thấy đơn hàng
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
    }
}