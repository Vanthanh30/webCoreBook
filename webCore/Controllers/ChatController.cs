using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System;

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

        // Trang chat - Hiển thị danh sách cuộc trò chuyện
        public async Task<IActionResult> Index(string sellerId, string buyerId)
        {
            string currentUserId = HttpContext.Session.GetString("SellerId") ??
                                   HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Người dùng chưa login.");

            var userRoles = (HttpContext.Session.GetString("UserRoles") ?? "").Split(',');
            bool isSeller = userRoles.Contains("Seller");

            // Lấy danh sách các cuộc trò chuyện
            var conversations = await _chatService.GetConversationsForUserAsync(currentUserId, isSeller);

            // Xác định người để chat
            string selectedOtherUserId = null;
            string selectedSellerId = null;
            string selectedBuyerId = null;

            if (!string.IsNullOrEmpty(sellerId))
            {
                // Đang chọn chat với một seller cụ thể (buyer mode)
                selectedOtherUserId = sellerId;
                selectedSellerId = sellerId;
                selectedBuyerId = currentUserId;
            }
            else if (!string.IsNullOrEmpty(buyerId))
            {
                // Đang chọn chat với một buyer cụ thể (seller mode)
                selectedOtherUserId = buyerId;
                selectedSellerId = currentUserId;
                selectedBuyerId = buyerId;
            }
            else if (conversations.Any())
            {
                // Chọn cuộc trò chuyện đầu tiên
                selectedOtherUserId = conversations.First().OtherUserId;
                if (isSeller)
                {
                    selectedBuyerId = selectedOtherUserId;
                    selectedSellerId = currentUserId;
                }
                else
                {
                    selectedSellerId = selectedOtherUserId;
                    selectedBuyerId = currentUserId;
                }
            }

            // Load tin nhắn
            List<ChatMessage> messages = new List<ChatMessage>();
            string otherUserName = "Chọn người để chat";
            string otherUserAvatar = "/image/macdinh.jpg";
            List<Order> relatedOrders = new List<Order>();

            if (!string.IsNullOrEmpty(selectedSellerId) && !string.IsNullOrEmpty(selectedBuyerId))
            {
                messages = await _chatService.GetMessagesBetweenUsersAsync(selectedBuyerId, selectedSellerId);

                // Lấy thông tin đơn hàng liên quan
                if (isSeller)
                {
                    // Seller: Lấy đơn của buyer này
                    var allBuyerOrders = await _orderService.GetOrdersByUserIdAsync(selectedBuyerId);
                    relatedOrders = allBuyerOrders
                        .Where(o => o.Items.Any(i => i.SellerId == currentUserId))
                        .ToList();

                    var firstOrder = relatedOrders.FirstOrDefault();
                    if (firstOrder != null)
                    {
                        otherUserName = firstOrder.FullName ?? "Khách hàng";
                        var item = firstOrder.Items.FirstOrDefault();
                        if (item != null)
                        {
                            otherUserAvatar = item.Image ?? "/image/macdinh.jpg";
                        }
                    }
                }
                else
                {
                    // Buyer: Lấy đơn từ seller này
                    var allMyOrders = await _orderService.GetOrdersByUserIdAsync(currentUserId);
                    relatedOrders = allMyOrders
                        .Where(o => o.Items.Any(i => i.SellerId == selectedSellerId))
                        .ToList();

                    var firstOrder = relatedOrders.FirstOrDefault();
                    if (firstOrder != null)
                    {
                        var item = firstOrder.Items.FirstOrDefault(i => i.SellerId == selectedSellerId);
                        if (item != null)
                        {
                            otherUserName = $"Shop: {item.Title}";
                            otherUserAvatar = item.Image ?? "/image/macdinh.jpg";
                        }
                    }
                }
            }

            // Tạo danh sách hiển thị sidebar với thông tin đầy đủ
            var conversationDisplayList = new List<ConversationDisplay>();

            foreach (var conv in conversations)
            {
                var display = new ConversationDisplay
                {
                    OtherUserId = conv.OtherUserId,
                    LastMessage = conv.LastMessage,
                    LastMessageTime = conv.LastMessageTime,
                    MessageCount = conv.MessageCount
                };

                // Lấy thông tin hiển thị từ đơn hàng
                if (isSeller)
                {
                    var buyerOrders = await _orderService.GetOrdersByUserIdAsync(conv.OtherUserId);
                    var order = buyerOrders.FirstOrDefault(o => o.Items.Any(i => i.SellerId == currentUserId));

                    if (order != null)
                    {
                        display.DisplayName = order.FullName ?? "Khách hàng";
                        display.AvatarUrl = order.Items.FirstOrDefault()?.Image ?? "/image/macdinh.jpg";
                        display.OrderCount = buyerOrders.Count(o => o.Items.Any(i => i.SellerId == currentUserId));
                    }
                }
                else
                {
                    var myOrders = await _orderService.GetOrdersByUserIdAsync(currentUserId);
                    var order = myOrders.FirstOrDefault(o => o.Items.Any(i => i.SellerId == conv.OtherUserId));

                    if (order != null)
                    {
                        var item = order.Items.FirstOrDefault(i => i.SellerId == conv.OtherUserId);
                        display.DisplayName = item?.Title ?? "Shop";
                        display.AvatarUrl = item?.Image ?? "/image/macdinh.jpg";
                        display.OrderCount = myOrders.Count(o => o.Items.Any(i => i.SellerId == conv.OtherUserId));
                    }
                }

                conversationDisplayList.Add(display);
            }

            // ViewBag
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.SelectedOtherUserId = selectedOtherUserId;
            ViewBag.SelectedSellerId = selectedSellerId;
            ViewBag.SelectedBuyerId = selectedBuyerId;
            ViewBag.OtherUserName = otherUserName;
            ViewBag.OtherUserAvatar = otherUserAvatar;
            ViewBag.IsSeller = isSeller;
            ViewBag.ConversationList = conversationDisplayList;

            return View(new Tuple<List<Order>, List<ChatMessage>>(relatedOrders, messages));
        }

        // Gửi tin nhắn
        [HttpPost]
        public async Task<IActionResult> SendMessage(string sellerId, string buyerId, string message)
        {
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(sellerId) || string.IsNullOrEmpty(buyerId))
                return BadRequest("Thông tin không hợp lệ.");

            string currentUserId = HttpContext.Session.GetString("SellerId") ??
                                   HttpContext.Session.GetString("UserId");

            var chatMessage = new ChatMessage
            {
                SellerId = sellerId,
                BuyerId = buyerId,
                SenderId = currentUserId,
                ReceiverId = currentUserId == sellerId ? buyerId : sellerId,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            await _chatService.AddMessageAsync(chatMessage);
            return Ok();
        }
    }

    // Class hỗ trợ hiển thị
    public class ConversationDisplay
    {
        public string OtherUserId { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int MessageCount { get; set; }
        public int OrderCount { get; set; }
    }
}