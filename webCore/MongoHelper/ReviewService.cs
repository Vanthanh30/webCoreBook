

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
            _reviews = mongoDBService.Reviews;
        }

        public async Task CreateAsync(Review review)
        {
            await _reviews.InsertOneAsync(review);
        }

        public async Task<List<Review>> GetByProductIdAsync(string productId)
        {
            return await _reviews.Find(r => r.ProductId == productId)
                                 .SortByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<bool> HasReviewedAsync(string orderId, string productId)
        {
            var review = await _reviews.Find(r => r.OrderId == orderId && r.ProductId == productId)
                                       .FirstOrDefaultAsync();
            return review != null;
        }

        public async Task<List<Review>> GetByUserIdAsync(string userId)
        {
            return await _reviews.Find(r => r.UserId == userId)
                                 .SortByDescending(r => r.CreatedAt)
                                 .ToListAsync();
        }

        public async Task<Review> GetByIdAsync(string reviewId)
        {
            return await _reviews.Find(r => r.Id == reviewId)
                                 .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(string reviewId)
        {
            var result = await _reviews.DeleteOneAsync(r => r.Id == reviewId);
            return result.DeletedCount > 0;
        }
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

        public async Task<long> GetCountByProductIdAsync(string productId)
        {
            return await _reviews.CountDocumentsAsync(r => r.ProductId == productId);
        }

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