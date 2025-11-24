using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace webCore.Models
{
    public class Shop
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [Required]
        public string ShopName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string BusinessType { get; set; }

        [Required]
        public string Address { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }

        public string ShopImage { get; set; } = "default-image-url";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
