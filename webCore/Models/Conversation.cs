using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webCore.Models
{
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        // Users._id
        public string BuyerId { get; set; } = string.Empty;

        // Users._id (shop owner)
        public string SellerId { get; set; } = string.Empty;

        // Shops._id
        public string ShopId { get; set; } = string.Empty;

        public string ? LastMessage { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
}
