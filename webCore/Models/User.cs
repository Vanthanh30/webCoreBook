using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace webCore.Models
{
    [BsonIgnoreExtraElements] // Thêm dòng này để bỏ qua các trường không có trong model
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = GenerateRandomString(10);

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }

        // Trường này chỉ dùng trong đăng ký, không lưu vào MongoDB
        [BsonIgnore]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; } = GenerateRandomString(20);
        public int? Status { get; set; }
        public bool Deleted { get; set; }
        public DateTime? Birthday { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string ProfileImage { get; set; } = "default-image-url";

        // HOẶC nếu bạn muốn sử dụng RoleId, thêm property này:
        // public string RoleId { get; set; }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}