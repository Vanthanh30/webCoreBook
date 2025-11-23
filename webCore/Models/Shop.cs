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

        [Required(ErrorMessage = "UserId là bắt buộc")]
        [BsonElement("UserId")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Tên Shop không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên Shop tối đa 100 ký tự")]
        [BsonElement("ShopName")]
        public string ShopName { get; set; }

        [Required(ErrorMessage = "Mô tả shop là bắt buộc")]
        [MaxLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        [BsonElement("Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Loại hình kinh doanh là bắt buộc")]
        [MaxLength(50, ErrorMessage = "Loại hình kinh doanh tối đa 50 ký tự")]
        [BsonElement("BusinessType")]
        public string BusinessType { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        [BsonElement("Address")]
        public string Address { get; set; }

        [BsonElement("ShopImage")]
        [MaxLength(500)]
        public string ShopImage { get; set; } = "default-image-url";

        [BsonElement("CreatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
