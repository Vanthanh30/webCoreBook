using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatApiController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IMessageService _messageService;
        private readonly UserService _userService;
        private readonly ShopService _shopService;
        private readonly OrderService _orderService;
        private readonly ProductService _productService;

        public ChatApiController(IConversationService conversationService, IMessageService messageService, 
            UserService userService, ShopService shopService, OrderService orderService,ProductService productService)
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _userService = userService;
            _shopService = shopService;
            _orderService = orderService;
            _productService = productService;
        }
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(string mode = "buyer")
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            List<Conversation> convos =
                mode == "seller"
                ? await _conversationService.GetBySellerAsync(userId)
                : await _conversationService.GetByBuyerAsync(userId);

            var result = new List<ConversationVm>();

            foreach (var c in convos)
            {
                var buyer = await _userService.GetUserByIdAsync(c.BuyerId);
                var shop = await _shopService.GetShopByIdAsync(c.ShopId);

                result.Add(new ConversationVm
                {
                    Id = c.Id,
                    BuyerId = c.BuyerId,
                    BuyerName = buyer?.Name ?? "Người mua",

                    SellerId = c.SellerId,
                    ShopId = c.ShopId,
                    ShopName = shop?.ShopName ?? "Shop",

                    LastMessage = c.LastMessage,
                    UpdatedAt = c.UpdatedAt
                });
            }

            return Ok(result);
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] string conversationId, [FromQuery] int limit = 50)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!await _messageService.CanAccessAsync(conversationId, userId))
                return Forbid();

            limit = Math.Clamp(limit, 1, 200);

            var messages = await _messageService.GetMessagesAsync(conversationId, limit);
            return Ok(messages);
        }
        [HttpGet("order/summary/{orderId}")]
        public async Task<IActionResult> GetOrderSummary(string orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null) return NotFound();

            var firstItem = order.Items.FirstOrDefault();
            if (firstItem == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(firstItem.ProductId);

            return Ok(new
            {
                id = order.Id.ToString(),
                totalPrice = order.FinalAmount,
                productName = product?.Title,
                image = product?.Image
            });
        }
        [HttpGet("product/summary/{id}")]
        public async Task<IActionResult> Summary(string id)
        {
            var p = await _productService.GetProductByIdAsync(id);
            if (p == null) return NotFound();

            return Ok(new
            {
                id = p.Id,
                title = p.Title,
                price = p.Price,
                image = p.Image
            });
        }
        public class ConversationVm
        {
            public string Id { get; set; }

            public string BuyerId { get; set; }
            public string BuyerName { get; set; }

            public string SellerId { get; set; }

            public string ShopId { get; set; }
            public string ShopName { get; set; }

            public string? LastMessage { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

    }
}
