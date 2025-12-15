// ReviewService.cs - HOÀN CHỈNH

using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ReviewService
    {
        private readonly IMongoCollection<Review> _reviews;

        public ReviewService(MongoDBService mongoDBService)
        {
            // Sử dụng collection Reviews từ MongoDBService
            _reviews = mongoDBService.Reviews;
        }

        /// <summary>
        /// Tạo đánh giá mới
        /// </summary>
        public async Task CreateAsync(Review review)
        {
            await _reviews.InsertOneAsync(review);
        }

        /// <summary>
        /// Lấy tất cả đánh giá của sản phẩm, sắp xếp theo thời gian mới nhất
        /// </summary>
        public async Task<List<Review>> GetByProductIdAsync(string productId)
        {
            return await _reviews.Find(r => r.ProductId == productId)
                                 .SortByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra xem đơn hàng đã đánh giá sản phẩm này chưa
        /// </summary>
        public async Task<bool> HasReviewedAsync(string orderId, string productId)
        {
            var review = await _reviews.Find(r => r.OrderId == orderId && r.ProductId == productId)
                                       .FirstOrDefaultAsync();
            return review != null;
        }

        /// <summary>
        /// Lấy tất cả đánh giá của một user
        /// </summary>
        public async Task<List<Review>> GetByUserIdAsync(string userId)
        {
            return await _reviews.Find(r => r.UserId == userId)
                                 .SortByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        /// <summary>
        /// Lấy đánh giá theo ID
        /// </summary>
        public async Task<Review> GetByIdAsync(string reviewId)
        {
            return await _reviews.Find(r => r.Id == reviewId)
                                 .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Xóa đánh giá
        /// </summary>
        public async Task<bool> DeleteAsync(string reviewId)
        {
            var result = await _reviews.DeleteOneAsync(r => r.Id == reviewId);
            return result.DeletedCount > 0;
        }

        /// <summary>
        /// Cập nhật đánh giá
        /// </summary>
        public async Task<bool> UpdateAsync(string reviewId, Review updatedReview)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.Id, reviewId);
            var update = Builders<Review>.Update
                .Set(r => r.QualityRating, updatedReview.QualityRating)
                .Set(r => r.ServiceRating, updatedReview.ServiceRating)
                .Set(r => r.Comment, updatedReview.Comment)
                .Set(r => r.MediaUrls, updatedReview.MediaUrls);

            var result = await _reviews.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        /// <summary>
        /// Lấy số lượng đánh giá của sản phẩm
        /// </summary>
        public async Task<long> GetCountByProductIdAsync(string productId)
        {
            return await _reviews.CountDocumentsAsync(r => r.ProductId == productId);
        }

        /// <summary>
        /// Lấy điểm trung bình của sản phẩm
        /// </summary>
        public async Task<double> GetAverageRatingByProductIdAsync(string productId)
        {
            var reviews = await GetByProductIdAsync(productId);

            if (reviews.Count == 0)
                return 0;

            var totalQuality = reviews.Sum(r => r.QualityRating);
            var totalService = reviews.Sum(r => r.ServiceRating);
            var avgQuality = (double)totalQuality / reviews.Count;
            var avgService = (double)totalService / reviews.Count;

            return (avgQuality + avgService) / 2;
        }
    }
}