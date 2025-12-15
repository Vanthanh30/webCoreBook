using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using webCore.MongoHelper;

namespace webCore.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }

        private string? GetUserIdFromSession()
        {
            return Context.GetHttpContext()?.Session.GetString("UserId");
        }

        public async Task JoinConversation(string conversationId)
        {
            var userId = GetUserIdFromSession();
            if (string.IsNullOrEmpty(userId))
                throw new HubException("Unauthenticated");

            if (!await _messageService.CanAccessAsync(conversationId, userId))
                throw new HubException("Access denied");

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendMessage(string conversationId, string content)
        {
            var userId = GetUserIdFromSession();
            if (string.IsNullOrEmpty(userId))
                throw new HubException("Unauthenticated");

            if (string.IsNullOrWhiteSpace(content))
                return;

            if (!await _messageService.CanAccessAsync(conversationId, userId))
                throw new HubException("Access denied");

            var msg = await _messageService.SaveTextAsync(conversationId, userId, content);

            await Clients.Group(conversationId)
                .SendAsync("ReceiveMessage", msg);
        }
    }
}