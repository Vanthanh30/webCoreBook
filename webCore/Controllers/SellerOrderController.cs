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
        private readonly ReturnRequestService _returnRequestService;

        public SellerOrderController(SellerOrderService orderService, ReturnRequestService returnRequestService)
        {
            _orderService = orderService;
            _returnRequestService = returnRequestService;
        }
        private string GetCurrentSellerId()
        {
            var userId = HttpContext.Session.GetString("UserId");
            return userId;
        }

        private bool IsSellerRole()
        {
            var roles = HttpContext.Session.GetString("UserRoles");
            if (string.IsNullOrEmpty(roles))
                return false;
            var roleList = roles.Split(',').Select(r => r.Trim()).ToList();
            return roleList.Contains("Seller");
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> OrderManagement(string status = null)
        {
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

            var orders = await _orderService.GetOrdersBySellerIdAsync(sellerId, status);

            var statusCount = await _orderService.GetOrderStatusCountAsync(sellerId);

            ViewBag.StatusCount = statusCount;
            ViewBag.CurrentStatus = status;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(orders);
        }

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
        [HttpGet]
        public async Task<IActionResult> ReturnRequestDetail(string orderId)
        {
            var sellerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(sellerId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            var request = await _returnRequestService.GetByOrderIdAsync(orderId);
            if (request == null)
            {
                return NotFound("Không tìm thấy yêu cầu trả hàng cho đơn này.");
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            ViewBag.OrderInfo = order;

            return View(request);
        }

    }
}
