using MongoDB.Driver;
using webCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ChatService
    {
        private readonly IMongoCollection<ChatMessage> _chatCollection;

        public ChatService(MongoDBService mongoService)
        {
            _chatCollection = mongoService._chatCollection; // đảm bảo tên collection đúng
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string orderId, string buyerId, string sellerId)
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
