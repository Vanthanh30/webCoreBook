using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webCore.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System;
using webCore.MongoHelper;

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

        public async Task<IActionResult> Index(string orderId)
        {
            string currentUserId = HttpContext.Session.GetString("SellerId") ??
                                   HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Người dùng chưa login.");

            var userRoles = (HttpContext.Session.GetString("UserRoles") ?? "").Split(',');

            List<Order> orders = new List<Order>();

            if (userRoles.Contains("Seller"))
                orders = await _orderService.GetOrdersBySellerIdAsync(currentUserId);
            else
                orders = await _orderService.GetOrdersByUserIdAsync(currentUserId);

            var orderIdsWithMessages = await _chatService.GetOrderIdsWithMessagesAsync(currentUserId);

            if (string.IsNullOrEmpty(orderId))
                orderId = orderIdsWithMessages.FirstOrDefault() ?? orders.FirstOrDefault()?.Id.ToString();

            List<ChatMessage> messages = new List<ChatMessage>();
            string otherUserId = null;

            if (!string.IsNullOrEmpty(orderId))
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);

                if (order != null)
                {
                    var item = order.Items?.FirstOrDefault();
                    otherUserId = userRoles.Contains("Seller") ? order.UserId : item?.SellerId;

                    ViewBag.OtherUserAvatar = item?.Image;
                    ViewBag.OtherTitle = item?.Title;

                    messages = await _chatService.GetMessagesByOrderAsync(orderId);
                }
            }

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUserId = otherUserId;
            ViewBag.OrderId = orderId;
            ViewBag.OrdersWithMessages = orderIdsWithMessages;

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
