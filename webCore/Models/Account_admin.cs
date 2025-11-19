using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace webCore.Models
{
    public class Account_admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        [BsonElement("FullName")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        [BsonElement("Email")]
        public string Email { get; set; }

        [MaxLength(100)]
        [BsonElement("Password")]
        public string Password { get; set; }

        [MaxLength(100)]
        [BsonElement("Token")]
        public string Token { get; set; } = GenerateRandomString(20);

        [Phone]
        [MaxLength(15)]
        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("Avatar")]
        public string Avatar { get; set; }

        [BsonElement("RoleId")]
        public string RoleId { get; set; }

        [MaxLength(50)]
        [BsonElement("Status")]
        public string Status { get; set; }

        [BsonElement("Deleted")]
        public bool Deleted { get; set; } = false;

        [BsonElement("DeletedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? DeletedAt { get; set; }

        [BsonElement("CreatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}