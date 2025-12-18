using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
namespace webCore.MongoHelper
{
    public class ReturnRequestService
    {
        private readonly IMongoCollection<ReturnRequest> _returnRequests;

        public ReturnRequestService(MongoDBService mongoDBService)
        {
            _returnRequests = mongoDBService.ReturnRequests;
        }

        public async Task CreateAsync(ReturnRequest request)
        {
            await _returnRequests.InsertOneAsync(request);
        }

        public async Task<ReturnRequest> GetByIdAsync(string id)
        {
            return await _returnRequests.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ReturnRequest> GetByOrderIdAsync(string orderId)
        {
            return await _returnRequests.Find(r => r.OrderId == orderId).FirstOrDefaultAsync();
        }

        public async Task<bool> HasReturnRequestAsync(string orderId)
        {
            var request = await _returnRequests.Find(r => r.OrderId == orderId).FirstOrDefaultAsync();
            return request != null;
        }

        public async Task<List<ReturnRequest>> GetByUserIdAsync(string userId)
        {
            return await _returnRequests.Find(r => r.UserId == userId)
                                        .SortByDescending(r => r.CreatedAt)
                                        .ToListAsync();
        }

        public async Task<bool> UpdateAsync(string id, ReturnRequest updatedRequest)
        {
            var filter = Builders<ReturnRequest>.Filter.Eq(r => r.Id, id);

            var update = Builders<ReturnRequest>.Update
                .Set(r => r.Reason, updatedRequest.Reason)
                .Set(r => r.MediaUrls, updatedRequest.MediaUrls);

            var result = await _returnRequests.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
