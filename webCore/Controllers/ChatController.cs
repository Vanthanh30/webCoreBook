using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<IActionResult> Index(string orderId, string buyerId, string sellerId)
        {
            ViewBag.OrderId = orderId;
            ViewBag.BuyerId = buyerId;
            ViewBag.SellerId = sellerId;

            var messages = await _chatService.GetMessagesAsync(orderId, buyerId, sellerId);
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(ChatMessage msg)
        {
            msg.SentAt = DateTime.UtcNow;
            await _chatService.AddMessageAsync(msg);
            return Ok();
        }
    }
}
