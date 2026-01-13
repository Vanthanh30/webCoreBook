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

        public string SellerId { get; set; }    
        public string BuyerId { get; set; }     

        public string SenderId { get; set; }      
        public string ReceiverId { get; set; }   

        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        [BsonIgnoreIfNull]
        public string RelatedOrderId { get; set; }

    }
}