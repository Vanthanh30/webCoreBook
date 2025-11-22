using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace webCore.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        [BsonElement("Name")]
        public string Name { get; set; } = GenerateRandomString(10);

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [MaxLength(100)]
        [BsonElement("Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MaxLength(100)]
        [BsonElement("Password")]
        public string Password { get; set; }

        // Không lưu ConfirmPassword
        [BsonIgnore]
        public string ConfirmPassword { get; set; }

        [Phone]
        [MaxLength(15)]
        [BsonElement("Phone")]
        public string Phone { get; set; }

        [BsonElement("Gender")]
        [MaxLength(10)]
        public string Gender { get; set; }

        [BsonElement("Address")]
        [MaxLength(255)]
        public string Address { get; set; }

        [BsonElement("Birthday")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? Birthday { get; set; }

        [BsonElement("ProfileImage")]
        [MaxLength(500)]
        public string ProfileImage { get; set; } = "default-image-url";

        [BsonElement("Token")]
        [MaxLength(100)]
        public string Token { get; set; } = GenerateRandomString(20);

        [BsonElement("Status")]
        public int? Status { get; set; } = 1; // 1 = hoạt động, 0 = khóa

        [BsonElement("RoleId")]
        public List<string> RoleId { get; set; } = new List<string>();
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
