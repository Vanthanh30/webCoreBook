using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace webCore.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Thay đổi: Chat theo SellerId thay vì OrderId
        public string SellerId { get; set; }      // ID người bán
        public string BuyerId { get; set; }       // ID người mua

        public string SenderId { get; set; }      // Người gửi (có thể là buyer hoặc seller)
        public string ReceiverId { get; set; }    // Người nhận

        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        [BsonIgnoreIfNull]
        public string RelatedOrderId { get; set; }

    }
}