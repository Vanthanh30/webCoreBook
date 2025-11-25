using Microsoft.AspNetCore.Mvc;
using webCore.Services;
using webCore.Models;
using Microsoft.AspNetCore.SignalR;
using webCore.Hubs;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;

namespace webCore.Controllers.ApiControllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatApiController : ControllerBase
    {
        private readonly MongoDBService _db;
        private readonly IHubContext<ChatHub> _hub;
        private readonly CloudinaryService _cloud;

        public ChatApiController(MongoDBService db, IHubContext<ChatHub> hub, CloudinaryService cloud)
        {
            _db = db;
            _hub = hub;
            _cloud = cloud;
        }

        [HttpGet("history/{orderId}")]
        public async Task<IActionResult> GetHistory(string orderId)
        {
            var msgs = await _db._chatCollection
                                .Find(x => x.OrderId == orderId)
                                .SortBy(x => x.CreatedAt)
                                .ToListAsync();
            return Ok(msgs);
        }

        [HttpPost("send-text")]
        public async Task<IActionResult> SendText([FromBody] ChatMessage msg)
        {
            await _db._chatCollection.InsertOneAsync(msg);
            await _hub.Clients.Group(msg.ReceiverId).SendAsync("ReceiveMessage", msg);
            await _hub.Clients.Group(msg.SenderId).SendAsync("ReceiveMessage", msg);
            return Ok();
        }

        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] string senderId,
                                                    [FromForm] string receiverId,
                                                    [FromForm] string orderId,
                                                    [FromForm] string type,
                                                    IFormFile file)
        {
/*            string url = await _cloud.UploadFileAsync(file);*/
            var msg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                OrderId = orderId
/*                Type = type == "image" ? MessageType.Image : MessageType.Video,
                ImageUrl = type == "image" ? url : null,
                VideoUrl = type == "video" ? url : null*/
            };

            await _db._chatCollection.InsertOneAsync(msg);
            await _hub.Clients.Group(receiverId).SendAsync("ReceiveMessage", msg);
            await _hub.Clients.Group(senderId).SendAsync("ReceiveMessage", msg);
            return Ok(msg);
        }
    }

}
