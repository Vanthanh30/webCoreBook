using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatApiController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatApiController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("GetMessages")]
        public async Task<IActionResult> GetMessages(string orderId, string buyerId, string sellerId)
        {
            var messages = await _chatService.GetMessagesAsync(orderId, buyerId, sellerId);
            return Ok(messages);
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.OrderId) ||
                string.IsNullOrEmpty(message.SenderId) ||
                string.IsNullOrEmpty(message.ReceiverId))
            {
                return BadRequest("OrderId, SenderId, ReceiverId phải có giá trị");
            }

            // <-- sửa ở đây: dùng SentAt thay vì Timestamp
            message.SentAt = DateTime.UtcNow;

            await _chatService.AddMessageAsync(message);

            return Ok(message);
        }
    }
}
