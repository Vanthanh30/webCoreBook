using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;

namespace webCore.MongoHelper
{
    public interface IMessageService
    {
        Task<List<Message>> GetMessagesAsync(string conversationId, int limit = 50);
        Task<Message> SaveTextAsync(string conversationId, string senderId, string content);

        // Shopee-style context (system card)
        Task<Message> SaveSystemAsync(string conversationId, string content, string? productId = null, string? orderId = null);

        Task<bool> CanAccessAsync(string conversationId, string userId);
    }

    public class MessageService : IMessageService
    {
        private readonly IMongoCollection<Message> _messages;
        private readonly IMongoCollection<Conversation> _convos;

        public MessageService(IMongoDatabase db)
        {
            _messages = db.GetCollection<Message>("ChatMessages");
            _convos = db.GetCollection<Conversation>("ChatConversations");
        }

        public async Task<bool> CanAccessAsync(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(userId))
                return false;

            var convo = await _convos.Find(x => x.Id == conversationId).FirstOrDefaultAsync();
            if (convo == null) return false;

            return convo.BuyerId == userId || convo.SellerId == userId;
        }

        public async Task<List<Message>> GetMessagesAsync(string conversationId, int limit = 50)
        {
            return await _messages
                .Find(x => x.ConversationId == conversationId)
                .SortBy(x => x.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Message> SaveTextAsync(string conversationId, string senderId, string content)
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content?.Trim() ?? "",
                MessageType = "text",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _messages.InsertOneAsync(msg);

            // update conversation preview
            var update = Builders<Conversation>.Update
                .Set(x => x.LastMessage, msg.Content)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            await _convos.UpdateOneAsync(x => x.Id == conversationId, update);

            return msg;
        }

        public async Task<Message> SaveSystemAsync(string conversationId, string content, string? productId = null, string? orderId = null)
        {
            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = "system",
                Content = content,
                MessageType = "system",
                ProductId = productId,
                OrderId = orderId,
                CreatedAt = DateTime.UtcNow,
                IsRead = true
            };

            await _messages.InsertOneAsync(msg);
            return msg;
        }
    }
}
