using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace webCore.Models
{
    public class Review
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string OrderId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string ProductId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }
        public string Name { get; set; } 

        public string ProductTitle { get; set; }
        public string ProductImage { get; set; }

        public string UserName { get; set; } 
        public string UserAvatar { get; set; }

        public int QualityRating { get; set; } 
        public int ServiceRating { get; set; } 
        public string Comment { get; set; }

        public List<string> MediaUrls { get; set; } = new List<string>();

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}