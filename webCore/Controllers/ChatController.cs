using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System;

namespace webCore.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;
        private readonly OrderService _orderService;

        public ChatController(ChatService chatService, OrderService orderService)
        {
            _chatService = chatService;
            _orderService = orderService;
        }

        // Trang chat
        public async Task<IActionResult> Index(string orderId)
        {
            string currentUserId = HttpContext.Session.GetString("SellerId") ??
                                   HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Người dùng chưa login.");

            var userRoles = (HttpContext.Session.GetString("UserRoles") ?? "").Split(',');

            List<Order> orders = new List<Order>();

            if (userRoles.Contains("Seller"))
            {
                // Lấy tất cả đơn hàng có item thuộc seller
                orders = await _orderService.GetOrdersBySellerIdAsync(currentUserId);
            }
            else
            {
                // Buyer: lấy đơn của mình
                orders = await _orderService.GetOrdersByUserIdAsync(currentUserId);
            }

            // Danh sách orderId có tin nhắn
            var orderIdsWithMessages = await _chatService.GetOrderIdsWithMessagesAsync(currentUserId);

            // Nếu chưa chọn orderId, mặc định chọn order đầu tiên có tin nhắn hoặc đơn đầu tiên
            if (string.IsNullOrEmpty(orderId))
            {
                orderId = orderIdsWithMessages.FirstOrDefault() ?? orders.FirstOrDefault()?.Id.ToString();
            }

            List<ChatMessage> messages = new List<ChatMessage>();
            string otherUserId = null;

            if (!string.IsNullOrEmpty(orderId))
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order != null)
                {
                    otherUserId = userRoles.Contains("Seller")
                                  ? order.UserId
                                  : order.Items.FirstOrDefault()?.SellerId;

                    // Load tin nhắn
                    messages = await _chatService.GetMessagesByOrderAsync(orderId);
                }
            }

            // ViewBag
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUserId = otherUserId;
            ViewBag.OrderId = orderId;
            ViewBag.OrdersWithMessages = orderIdsWithMessages;

            // Gộp orders + messages vào Tuple
            return View(new Tuple<List<Order>, List<ChatMessage>>(orders, messages));
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(ChatMessage msg)
        {
            if (msg == null || string.IsNullOrEmpty(msg.Message))
                return BadRequest("Tin nhắn rỗng.");

            msg.SentAt = DateTime.UtcNow;
            await _chatService.AddMessageAsync(msg);
            return Ok();
        }
    }
}
