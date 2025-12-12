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
            // ✅ Lấy userId hiện tại
            string currentUserId = HttpContext.Session.GetString("SellerId") ??
                                   HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Người dùng chưa login.");

            var userRoles = (HttpContext.Session.GetString("UserRoles") ?? "").Split(',');
            bool isSeller = userRoles.Contains("Seller");

            Console.WriteLine($"[Chat Index] ========== START ==========");
            Console.WriteLine($"[Chat Index] CurrentUserId: {currentUserId}");
            Console.WriteLine($"[Chat Index] IsSeller: {isSeller}");
            Console.WriteLine($"[Chat Index] Requested SellerId: {sellerId}");
            Console.WriteLine($"[Chat Index] Requested BuyerId: {buyerId}");

            // ✅ Lấy danh sách các cuộc trò chuyện
            var conversations = await _chatService.GetConversationsForUserAsync(currentUserId, isSeller);

            Console.WriteLine($"[Chat Index] Found {conversations.Count} conversations");

            // ✅ Xác định người để chat
            string selectedOtherUserId = null;
            string selectedSellerId = null;
            string selectedBuyerId = null;

            if (!string.IsNullOrEmpty(sellerId))
            {
                // Buyer mode - Đang chọn chat với một seller cụ thể
                selectedOtherUserId = sellerId;
                selectedSellerId = sellerId;
                selectedBuyerId = currentUserId;

                Console.WriteLine($"[Chat Index] MODE: Buyer chatting with Seller {sellerId}");
            }
            else if (!string.IsNullOrEmpty(buyerId))
            {
                // Seller mode - Đang chọn chat với một buyer cụ thể
                selectedOtherUserId = buyerId;
                selectedSellerId = currentUserId;
                selectedBuyerId = buyerId;

                Console.WriteLine($"[Chat Index] MODE: Seller chatting with Buyer {buyerId}");
            }
            else if (conversations.Any())
            {
                // ✅ Chọn cuộc trò chuyện đầu tiên
                selectedOtherUserId = conversations.First().OtherUserId;

                if (isSeller)
                {
                    selectedBuyerId = selectedOtherUserId;
                    selectedSellerId = currentUserId;
                    Console.WriteLine($"[Chat Index] MODE: Seller - Auto-selected Buyer {selectedOtherUserId}");
                }
                else
                {
                    selectedSellerId = selectedOtherUserId;
                    selectedBuyerId = currentUserId;
                    Console.WriteLine($"[Chat Index] MODE: Buyer - Auto-selected Seller {selectedOtherUserId}");
                }
            }

            // ✅ Load tin nhắn và thông tin người dùng
            List<ChatMessage> messages = new List<ChatMessage>();
            string otherUserName = "Chọn người để chat";
            string otherUserAvatar = "/image/macdinh.jpg";
            List<Order> relatedOrders = new List<Order>();

            if (!string.IsNullOrEmpty(selectedSellerId) && !string.IsNullOrEmpty(selectedBuyerId))
            {
                messages = await _chatService.GetMessagesBetweenUsersAsync(selectedBuyerId, selectedSellerId);

                Console.WriteLine($"[Chat Index] Loaded {messages.Count} messages between Buyer:{selectedBuyerId} and Seller:{selectedSellerId}");

                // ✅ Lấy thông tin đơn hàng liên quan
                if (isSeller)
                {
                    // Seller: Lấy đơn của buyer này
                    var allBuyerOrders = await _orderService.GetOrdersByUserIdAsync(selectedBuyerId);
                    relatedOrders = allBuyerOrders
                        .Where(o => o.Items != null && o.Items.Any(i => i.SellerId == currentUserId))
                        .ToList();

                    Console.WriteLine($"[Chat Index] Seller found {relatedOrders.Count} orders from buyer {selectedBuyerId}");

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
                        .Where(o => o.Items != null && o.Items.Any(i => i.SellerId == selectedSellerId))
                        .ToList();

                    Console.WriteLine($"[Chat Index] Buyer found {relatedOrders.Count} orders from seller {selectedSellerId}");

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

            // ✅ Tạo danh sách hiển thị sidebar với thông tin đầy đủ
            var conversationDisplayList = new List<ConversationDisplay>();

            Console.WriteLine($"[Chat Index] Building conversation display list...");

            foreach (var conv in conversations)
            {
                var display = new ConversationDisplay
                {
                    OtherUserId = conv.OtherUserId,
                    LastMessage = conv.LastMessage,
                    LastMessageTime = conv.LastMessageTime,
                    MessageCount = conv.MessageCount
                };

                Console.WriteLine($"[Chat Index] Processing conversation with {conv.OtherUserId}");

                // ✅ Lấy thông tin hiển thị từ đơn hàng
                if (isSeller)
                {
                    // Seller: Hiển thị thông tin buyer
                    var buyerOrders = await _orderService.GetOrdersByUserIdAsync(conv.OtherUserId);
                    var order = buyerOrders.FirstOrDefault(o => o.Items != null && o.Items.Any(i => i.SellerId == currentUserId));

                    if (order != null)
                    {
                        display.DisplayName = order.FullName ?? "Khách hàng";
                        display.AvatarUrl = order.Items.FirstOrDefault()?.Image ?? "/image/macdinh.jpg";
                        display.OrderCount = buyerOrders.Count(o => o.Items != null && o.Items.Any(i => i.SellerId == currentUserId));
                    }
                    else
                    {
                        display.DisplayName = "Khách hàng";
                        display.AvatarUrl = "/image/macdinh.jpg";
                        display.OrderCount = 0;
                    }

                    Console.WriteLine($"[Chat Index] Seller view - Buyer {conv.OtherUserId}: {display.DisplayName}, Orders: {display.OrderCount}");
                }
                else
                {
                    // ✅ Buyer: Hiển thị thông tin seller
                    var myOrders = await _orderService.GetOrdersByUserIdAsync(currentUserId);
                    var order = myOrders.FirstOrDefault(o => o.Items != null && o.Items.Any(i => i.SellerId == conv.OtherUserId));

                    if (order != null)
                    {
                        var item = order.Items.FirstOrDefault(i => i.SellerId == conv.OtherUserId);
                        display.DisplayName = item?.Title ?? "Shop";
                        display.AvatarUrl = item?.Image ?? "/image/macdinh.jpg";
                        display.OrderCount = myOrders.Count(o => o.Items != null && o.Items.Any(i => i.SellerId == conv.OtherUserId));
                    }
                    else
                    {
                        display.DisplayName = "Shop";
                        display.AvatarUrl = "/image/macdinh.jpg";
                        display.OrderCount = 0;
                    }

                    Console.WriteLine($"[Chat Index] Buyer view - Seller {conv.OtherUserId}: {display.DisplayName}, Orders: {display.OrderCount}");
                }

                conversationDisplayList.Add(display);
            }

            Console.WriteLine($"[Chat Index] Created {conversationDisplayList.Count} conversation displays");
            Console.WriteLine($"[Chat Index] ========== END ==========");

            // ✅ ViewBag
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

        // ✅ Gửi tin nhắn
        [HttpPost]
        public async Task<IActionResult> SendMessage(string sellerId, string buyerId, string message)
        {
            Console.WriteLine($"[SendMessage] ========== START ==========");
            Console.WriteLine($"[SendMessage] Received - SellerId: {sellerId}, BuyerId: {buyerId}, Message: {message}");

            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(sellerId) || string.IsNullOrEmpty(buyerId))
            {
                Console.WriteLine($"[SendMessage] ERROR: Missing required fields");
                return BadRequest("Thông tin không hợp lệ.");
            }

            string currentUserId = HttpContext.Session.GetString("SellerId") ??
                                   HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
            {
                Console.WriteLine($"[SendMessage] ERROR: User not logged in");
                return Unauthorized("Người dùng chưa đăng nhập.");
            }

            // ✅ Xác định người nhận
            string receiverId = currentUserId == sellerId ? buyerId : sellerId;

            // ✅ Lấy RelatedOrderId từ session (nếu có)
            string relatedOrderId = HttpContext.Session.GetString("RelatedOrderId");

            Console.WriteLine($"[SendMessage] CurrentUserId: {currentUserId}");
            Console.WriteLine($"[SendMessage] ReceiverId: {receiverId}");
            Console.WriteLine($"[SendMessage] RelatedOrderId: {relatedOrderId}");

            var chatMessage = new ChatMessage
            {
                SellerId = sellerId,
                BuyerId = buyerId,
                SenderId = currentUserId,
                ReceiverId = receiverId,
                Message = message,
                SentAt = DateTime.UtcNow,
                RelatedOrderId = relatedOrderId
            };

            try
            {
                await _chatService.AddMessageAsync(chatMessage);
                Console.WriteLine($"[SendMessage] Message saved successfully with Id: {chatMessage.Id}");
                Console.WriteLine($"[SendMessage] ========== END ==========");
                return Ok(new { success = true, messageId = chatMessage.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendMessage] ERROR: {ex.Message}");
                Console.WriteLine($"[SendMessage] ========== END ==========");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
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