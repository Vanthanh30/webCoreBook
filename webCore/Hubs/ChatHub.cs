using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;

namespace webCore.Hubs
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
                Groups.AddToGroupAsync(Context.ConnectionId, userId);
            return base.OnConnectedAsync();
        }

        public async Task SendMessage(ChatMessage msg)
        {
            // Gửi cho cả sender & receiver
            await Clients.Group(msg.ReceiverId).SendAsync("ReceiveMessage", msg);
            await Clients.Group(msg.SenderId).SendAsync("ReceiveMessage", msg);
        }
    }
}
