using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace webCore.Models
{
    public class ReturnRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string OrderId { get; set; } 
        public string UserId { get; set; } 
        public string Reason { get; set; }  
        public List<string> MediaUrls { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
