using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;

namespace webCore.MongoHelper
{
    public interface IConversationService
    {
        Task<Conversation?> GetByIdAsync(string conversationId);
        Task<bool> CanAccessAsync(string conversationId, string userId);

        Task<Conversation> GetOrCreateAsync(string buyerId, string sellerId, string shopId);

        Task<List<Conversation>> GetByBuyerAsync(string buyerId);
        Task<List<Conversation>> GetBySellerAsync(string sellerId);
    }

    public class ConversationService : IConversationService
    {
        private readonly IMongoCollection<Conversation> _convos;

        public ConversationService(IMongoDatabase db)
        {
            _convos = db.GetCollection<Conversation>("ChatConversations");
        }

        public async Task<Conversation?> GetByIdAsync(string conversationId)
        {
            return await _convos.Find(x => x.Id == conversationId).FirstOrDefaultAsync();
        }

        public async Task<bool> CanAccessAsync(string conversationId, string userId)
        {
            if (string.IsNullOrEmpty(conversationId) || string.IsNullOrEmpty(userId))
                return false;

            var convo = await GetByIdAsync(conversationId);
            if (convo == null) return false;

            return convo.BuyerId == userId || convo.SellerId == userId;
        }

        public async Task<Conversation> GetOrCreateAsync(string buyerId, string sellerId, string shopId)
        {
            var existed = await _convos.Find(x =>
                x.BuyerId == buyerId &&
                x.SellerId == sellerId &&
                x.ShopId == shopId
            ).FirstOrDefaultAsync();

            if (existed != null) return existed;

            var convo = new Conversation
            {
                BuyerId = buyerId,
                SellerId = sellerId,
                ShopId = shopId,
                UpdatedAt = DateTime.UtcNow,
                LastMessage = null
            };

            await _convos.InsertOneAsync(convo);
            return convo;
        }

        public async Task<List<Conversation>> GetByBuyerAsync(string buyerId)
        {
            return await _convos
                .Find(x => x.BuyerId == buyerId)
                .SortByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetBySellerAsync(string sellerId)
        {
            return await _convos
                .Find(x => x.SellerId == sellerId)
                .SortByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }
    }
}
