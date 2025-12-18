using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
namespace webCore.Models
{
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("Items")]
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CartItem
    {
        [BsonElement("ProductId")]
        public string ProductId { get; set; }
        [BsonElement("SellerId")]
        public string SellerId { get; set; }
        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("DiscountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [BsonElement("Quantity")]
        public int Quantity { get; set; }

        [BsonElement("Image")]
        public string Image { get; set; }
    }
}
