using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace webCore.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string sellerId, string buyerId, string senderId, string message)
        {
            // Tạo tên group dựa trên sellerId và buyerId
            var groupName = GetGroupName(sellerId, buyerId);

            // Gửi tin nhắn cho tất cả members trong group
            await Clients.Group(groupName).SendAsync("ReceiveMessage", senderId, message);
        }

        public override async Task OnConnectedAsync()
        {
            var sellerId = Context.GetHttpContext().Request.Query["sellerId"];
            var buyerId = Context.GetHttpContext().Request.Query["buyerId"];

            if (!string.IsNullOrEmpty(sellerId) && !string.IsNullOrEmpty(buyerId))
            {
                var groupName = GetGroupName(sellerId, buyerId);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

                System.Console.WriteLine($"User connected to group: {groupName}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var sellerId = Context.GetHttpContext().Request.Query["sellerId"];
            var buyerId = Context.GetHttpContext().Request.Query["buyerId"];

            if (!string.IsNullOrEmpty(sellerId) && !string.IsNullOrEmpty(buyerId))
            {
                var groupName = GetGroupName(sellerId, buyerId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                System.Console.WriteLine($"User disconnected from group: {groupName}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Tạo tên group duy nhất cho mỗi cặp seller-buyer
        // Sử dụng format cố định để đảm bảo tính nhất quán
        private string GetGroupName(string sellerId, string buyerId)
        {
            return $"chat_{sellerId}_{buyerId}";
        }
    }
}