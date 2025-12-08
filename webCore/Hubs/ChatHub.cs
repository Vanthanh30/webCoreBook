using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace webCore.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string orderId, string senderId, string message)
        {
            await Clients.Group(orderId).SendAsync("ReceiveMessage", senderId, message);
        }

        public override async Task OnConnectedAsync()
        {
            var orderId = Context.GetHttpContext().Request.Query["orderId"];
            if (!string.IsNullOrEmpty(orderId))
                await Groups.AddToGroupAsync(Context.ConnectionId, orderId);

            await base.OnConnectedAsync();
        }
    }
}
