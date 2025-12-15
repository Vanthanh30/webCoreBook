using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers.Attributes;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    public class SellerOrderController : BaseController
    {
        private readonly SellerOrderService _orderService;

        public SellerOrderController(SellerOrderService orderService)
        {
            _orderService = orderService;
        }
        private string GetCurrentSellerId()
        {
            // Lấy UserId từ Session, đây chính là SellerId
            var userId = HttpContext.Session.GetString("UserId");
            return userId;
        }

        // Kiểm tra role Seller
        private bool IsSellerRole()
        {
            var roles = HttpContext.Session.GetString("UserRoles");
            if (string.IsNullOrEmpty(roles))
                return false;
            // UserRoles được lưu dạng "Buyer,Seller" hoặc "Seller"
            var roleList = roles.Split(',').Select(r => r.Trim()).ToList();
            return roleList.Contains("Seller");
        }

        // Redirect về trang login API (hoặc trang chủ)
        private IActionResult RedirectToLogin()
        {
            // Redirect về trang chủ hoặc trang login của bạn
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> OrderManagement(string status = null)
        {
            // Kiểm tra đăng nhập và role
            if (!IsSellerRole())
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập với tài khoản Seller để truy cập trang này.";
                return RedirectToLogin();
            }

            var sellerId = GetCurrentSellerId();
            if (string.IsNullOrEmpty(sellerId))
            {
                return RedirectToLogin();
            }

            // Lấy danh sách đơn hàng
            var orders = await _orderService.GetOrdersBySellerIdAsync(sellerId, status);

            // Đếm số lượng đơn hàng theo trạng thái
            var statusCount = await _orderService.GetOrderStatusCountAsync(sellerId);

            ViewBag.StatusCount = statusCount;
            ViewBag.CurrentStatus = status;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(orders);
        }

        // GET: Chi tiết đơn hàng
        public async Task<IActionResult> OrderDetail(string id)
        {
            if (!IsSellerRole())
            {
                return RedirectToLogin();
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sellerId = GetCurrentSellerId();
            var order = await _orderService.GetOrderByIdAsync(id, sellerId);

            if (order == null || !order.Items.Any())
            {
                return NotFound();
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(order);
        }

        // GET: Chi tiết đơn hàng bị hủy
        public async Task<IActionResult> CancelDetail(string id)
        {
            if (!IsSellerRole())
            {
                return RedirectToLogin();
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var sellerId = GetCurrentSellerId();
            var order = await _orderService.GetOrderByIdAsync(id, sellerId);

            if (order == null || order.Status != "Đã hủy")
            {
                return NotFound();
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(order);
        }

        // POST: Xác nhận đơn hàng
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(string orderId)
        {
            if (!IsSellerRole())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var result = await _orderService.ConfirmOrderAsync(orderId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã xác nhận đơn hàng thành công" });
                }
                return Json(new { success = false, message = "Không thể xác nhận đơn hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Xác nhận nhiều đơn hàng
        [HttpPost]
        public async Task<IActionResult> ConfirmMultipleOrders([FromBody] List<string> orderIds)
        {
            if (!IsSellerRole())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var count = await _orderService.ConfirmMultipleOrdersAsync(orderIds);
                return Json(new { success = true, message = $"Đã xác nhận {count} đơn hàng", count = count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Đánh dấu đã chuẩn bị hàng
        [HttpPost]
        public async Task<IActionResult> ReadyToShip(string orderId)
        {
            if (!IsSellerRole())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var result = await _orderService.ReadyToShipAsync(orderId);
                if (result)
                {
                    return Json(new { success = true, message = "Đơn hàng đã sẵn sàng giao" });
                }
                return Json(new { success = false, message = "Không thể cập nhật trạng thái" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Đánh dấu đã giao hàng
        [HttpPost]
        public async Task<IActionResult> CompleteOrder(string orderId)
        {
            if (!IsSellerRole())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var result = await _orderService.CompleteOrderAsync(orderId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã hoàn thành đơn hàng" });
                }
                return Json(new { success = false, message = "Không thể cập nhật trạng thái" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Hủy đơn hàng
        [HttpPost]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            if (!IsSellerRole())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var result = await _orderService.CancelOrderAsync(orderId);
                if (result)
                {
                    return Json(new { success = true, message = "Đã hủy đơn hàng" });
                }
                return Json(new { success = false, message = "Không thể hủy đơn hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Hủy nhiều đơn hàng
        [HttpPost]
        public async Task<IActionResult> CancelMultipleOrders([FromBody] List<string> orderIds)
        {
            if (!IsSellerRole())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var count = await _orderService.CancelMultipleOrdersAsync(orderIds);
                return Json(new { success = true, message = $"Đã hủy {count} đơn hàng", count = count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
