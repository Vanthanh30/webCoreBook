using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
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
            var review = await _reviews.Find(r => r.OrderId == orderId && r.ProductId == productId).FirstOrDefaultAsync();
            return review != null;
        }
    }
}
