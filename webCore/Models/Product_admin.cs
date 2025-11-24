using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace webCore.Models
{
    public enum FeaturedStatus
    {
        None = 0,         // Không nổi bật
        Highlighted = 1,  // Nổi bật
        New = 2,          // Mới
        Suggested = 3     // Gợi ý
    }
    [Table("Product")]
    public class Product_admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SellerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title
        {
            get; set;
        }

        [MaxLength(1000000)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(10, 8)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; }

        public int Stock { get; set; }

        public string Image { get; set; }

        [ForeignKey("Category_admin")] // Khóa ngoại liên kết với bảng Category
        public string CategoryId { get; set; }

        public string CategoryTitle { get; set; }

        public int Featured { get; set; }


        [MaxLength(50)]
        public string Status { get; set; }

        public int Position { get; set; }

        public bool Deleted { get; set; } = false;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
