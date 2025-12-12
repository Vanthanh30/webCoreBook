using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Lấy danh sách các cuộc trò chuyện của user (cả buyer và seller)
        public async Task<List<ConversationInfo>> GetConversationsForUserAsync(string userId, bool isSeller)
        {
            FilterDefinition<ChatMessage> filter;

            if (isSeller)
            {
                // Seller: Lấy tin nhắn nơi user này là seller
                filter = Builders<ChatMessage>.Filter.Eq(m => m.SellerId, userId);
            }
            else
            {
                // Buyer: Lấy tin nhắn nơi user này là buyer
                filter = Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, userId);
            }

            var messages = await _chatCollection.Find(filter).ToListAsync();

            // Nhóm theo người còn lại (seller thì nhóm theo buyer, buyer thì nhóm theo seller)
            var grouped = isSeller
                ? messages.GroupBy(m => m.BuyerId)
                : messages.GroupBy(m => m.SellerId);

            var conversations = new List<ConversationInfo>();

            foreach (var group in grouped)
            {
                var lastMessage = group.OrderByDescending(m => m.SentAt).FirstOrDefault();
                if (lastMessage != null)
                {
                    conversations.Add(new ConversationInfo
                    {
                        OtherUserId = group.Key,
                        LastMessage = lastMessage.Message,
                        LastMessageTime = lastMessage.SentAt,
                        MessageCount = group.Count()
                    });
                }
            }

            return conversations.OrderByDescending(c => c.LastMessageTime).ToList();
        }

        // Lấy tin nhắn giữa buyer và seller
        public async Task<List<ChatMessage>> GetMessagesBetweenUsersAsync(string buyerId, string sellerId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, buyerId),
                Builders<ChatMessage>.Filter.Eq(m => m.SellerId, sellerId)
            );

            return await _chatCollection.Find(filter)
                                        .SortBy(m => m.SentAt)
                                        .ToListAsync();
        }

        // Thêm tin nhắn mới
        public async Task AddMessageAsync(ChatMessage message)
        {
            await _chatCollection.InsertOneAsync(message);
        }

        // Kiểm tra xem đã có cuộc trò chuyện giữa buyer và seller chưa
        public async Task<bool> HasConversationAsync(string buyerId, string sellerId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, buyerId),
                Builders<ChatMessage>.Filter.Eq(m => m.SellerId, sellerId)
            );

            var count = await _chatCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        // Lấy tin nhắn cuối cùng giữa buyer và seller
        public async Task<ChatMessage> GetLastMessageAsync(string buyerId, string sellerId)
        {
            var filter = Builders<ChatMessage>.Filter.And(
                Builders<ChatMessage>.Filter.Eq(m => m.BuyerId, buyerId),
                Builders<ChatMessage>.Filter.Eq(m => m.SellerId, sellerId)
            );

            return await _chatCollection.Find(filter)
                                       .SortByDescending(m => m.SentAt)
                                       .FirstOrDefaultAsync();
        }
    }

    // Class hỗ trợ hiển thị thông tin cuộc trò chuyện
    public class ConversationInfo
    {
        public string OtherUserId { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int MessageCount { get; set; }
    }
}