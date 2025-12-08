using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ChatService
    {
        private readonly IMongoCollection<ChatMessage> _chatCollection;

        public ChatService(MongoDBService mongoService)
        {
            _chatCollection = mongoService._chatCollection;
        }

        public async Task<List<string>> GetOrderIdsWithMessagesAsync(string userId)
        {
            var filter = Builders<ChatMessage>.Filter.Or(
                Builders<ChatMessage>.Filter.Eq(m => m.SenderId, userId),
                Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, userId)
            );

            // Dùng DistinctAsync của driver
            var orderIds = await _chatCollection.DistinctAsync<string>("OrderId", filter);
            return await orderIds.ToListAsync();
        }

        // Lấy tin nhắn theo OrderId
        public async Task<List<ChatMessage>> GetMessagesByOrderAsync(string orderId)
        {
            var filter = Builders<ChatMessage>.Filter.Eq(m => m.OrderId, orderId);
            return await _chatCollection.Find(filter)
                                        .SortBy(m => m.SentAt)
                                        .ToListAsync();
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            await _chatCollection.InsertOneAsync(message);
        }
    }
}
