using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;
        private readonly OrderService _orderService;
        private readonly ShopService _shopService;
        private readonly UserService _userService;

        public ChatController(
            ChatService chatService,
            OrderService orderService,
            ShopService shopService,
            UserService userService)
        {
            _chatService = chatService;
            _orderService = orderService;
            _shopService = shopService;
            _userService = userService;
        }

        // ======================== INDEX ========================
        public async Task<IActionResult> Index(string sellerId, string buyerId)
        {
            string currentUserId =
                HttpContext.Session.GetString("SellerId") ??
                HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized("Người dùng chưa đăng nhập.");

            var roles = (HttpContext.Session.GetString("UserRoles") ?? "").Split(',');
            bool isSeller = roles.Contains("Seller");

            // ===== Lấy danh sách cuộc trò chuyện =====
            var conversations =
                await _chatService.GetConversationsForUserAsync(currentUserId, isSeller);

            // ===== Xác định người đang chat =====
            string selectedSellerId = null;
            string selectedBuyerId = null;
            string selectedOtherUserId = null;

            if (!string.IsNullOrEmpty(sellerId))
            {
                selectedSellerId = sellerId;
                selectedBuyerId = currentUserId;
                selectedOtherUserId = sellerId;
            }
            else if (!string.IsNullOrEmpty(buyerId))
            {
                selectedSellerId = currentUserId;
                selectedBuyerId = buyerId;
                selectedOtherUserId = buyerId;
            }
            else if (conversations.Any())
            {
                selectedOtherUserId = conversations.First().OtherUserId;

                if (isSeller)
                {
                    selectedSellerId = currentUserId;
                    selectedBuyerId = selectedOtherUserId;
                }
                else
                {
                    selectedSellerId = selectedOtherUserId;
                    selectedBuyerId = currentUserId;
                }
            }

            // ===== Load tin nhắn =====
            var messages = new List<ChatMessage>();
            var relatedOrders = new List<Order>();

            string otherUserName = "Chọn người để chat";
            string otherUserAvatar = "/image/macdinh.jpg";

            if (!string.IsNullOrEmpty(selectedSellerId) &&
                !string.IsNullOrEmpty(selectedBuyerId))
            {
                messages = await _chatService
                    .GetMessagesBetweenUsersAsync(selectedBuyerId, selectedSellerId);

                if (isSeller)
                {
                    // ===== SELLER → LẤY TÊN BUYER TỪ USER =====
                    var buyerUser =
                        await _userService.GetUserByIdAsync(selectedBuyerId);

                    if (buyerUser != null)
                    {
                        otherUserName = buyerUser.Name ?? "Khách hàng";
                        otherUserAvatar = !string.IsNullOrEmpty(buyerUser.ProfileImage)
                            ? buyerUser.ProfileImage
                            : "/image/macdinh.jpg";
                    }

                    var buyerOrders =
                        await _orderService.GetOrdersByUserIdAsync(selectedBuyerId);

                    relatedOrders = buyerOrders
                        .Where(o => o.Items != null &&
                                    o.Items.Any(i => i.SellerId == currentUserId))
                        .ToList();
                }
                else
                {
                    // ===== BUYER → LẤY SHOP =====
                    var shop =
                        await _shopService.GetShopByUserIdAsync(selectedSellerId);

                    if (shop != null)
                    {
                        otherUserName = shop.ShopName ?? "Shop";
                        otherUserAvatar = !string.IsNullOrEmpty(shop.ShopImage)
                            ? shop.ShopImage
                            : "/image/macdinh.jpg";
                    }

                    var myOrders =
                        await _orderService.GetOrdersByUserIdAsync(currentUserId);

                    relatedOrders = myOrders
                        .Where(o => o.Items != null &&
                                    o.Items.Any(i => i.SellerId == selectedSellerId))
                        .ToList();
                }
            }

            // ================= SIDEBAR =================
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

                if (isSeller)
                {
                    // ===== SELLER SIDEBAR → USER =====
                    var buyerUser =
                        await _userService.GetUserByIdAsync(conv.OtherUserId);

                    var buyerOrders =
                        await _orderService.GetOrdersByUserIdAsync(conv.OtherUserId);

                    display.DisplayName =
                        buyerUser?.Name ?? "Khách hàng";

                    display.AvatarUrl =
                        !string.IsNullOrEmpty(buyerUser?.ProfileImage)
                            ? buyerUser.ProfileImage
                            : "/image/macdinh.jpg";

                    display.OrderCount = buyerOrders.Count(o =>
                        o.Items != null &&
                        o.Items.Any(i => i.SellerId == currentUserId));
                }
                else
                {
                    // ===== BUYER SIDEBAR → SHOP =====
                    var shop =
                        await _shopService.GetShopByUserIdAsync(conv.OtherUserId);

                    display.DisplayName =
                        shop?.ShopName ?? "Shop";

                    display.AvatarUrl =
                        !string.IsNullOrEmpty(shop?.ShopImage)
                            ? shop.ShopImage
                            : "/image/macdinh.jpg";

                    var myOrders =
                        await _orderService.GetOrdersByUserIdAsync(currentUserId);

                    display.OrderCount = myOrders.Count(o =>
                        o.Items != null &&
                        o.Items.Any(i => i.SellerId == conv.OtherUserId));
                }

                conversationDisplayList.Add(display);
            }

            // ===== ViewBag =====
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.SelectedSellerId = selectedSellerId;
            ViewBag.SelectedBuyerId = selectedBuyerId;
            ViewBag.SelectedOtherUserId = selectedOtherUserId;
            ViewBag.OtherUserName = otherUserName;
            ViewBag.OtherUserAvatar = otherUserAvatar;
            ViewBag.IsSeller = isSeller;
            ViewBag.ConversationList = conversationDisplayList;

            return View(
                new Tuple<List<Order>, List<ChatMessage>>(relatedOrders, messages)
            );
        }

        // ================= SEND MESSAGE =================
        [HttpPost]
        public async Task<IActionResult> SendMessage(
            string sellerId,
            string buyerId,
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest();

            string currentUserId =
                HttpContext.Session.GetString("SellerId") ??
                HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

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

    // ================= VIEW MODEL =================
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
