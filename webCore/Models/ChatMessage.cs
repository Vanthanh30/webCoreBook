using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public enum MessageType
    {
        Text,
        Emoji,
        Image,
        Video
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string SenderId { get; set; }       // Buyer/Seller
        public string SenderRole { get; set; }     // "buyer" / "seller"

        public string ReceiverId { get; set; }     // Ngược lại

        public string OrderId { get; set; }        // Liên kết Order
        public string ProductId { get; set; }      // Nếu muốn chi tiết theo sản phẩm

        public string MessageText { get; set; }    // Text
        public string Emoji { get; set; }          // Emoji
        public string ImageUrl { get; set; }       // URL ảnh
        public string VideoUrl { get; set; }       // URL video

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MessageType Type { get; set; }
    }
}
