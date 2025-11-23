using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ShopService
    {
        private readonly IMongoCollection<Shop> _shopCollection;

        public ShopService(MongoDBService mongoDBService)
        {
            _shopCollection = mongoDBService._shopCollection;
        }

        public async Task CreateShopAsync(Shop shop)
        {
            shop.CreatedAt = DateTime.UtcNow;
            shop.UpdatedAt = DateTime.UtcNow;

            await _shopCollection.InsertOneAsync(shop);
        }

        public async Task<Shop> GetShopByIdAsync(string id)
        {
            return await _shopCollection
                .Find(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Shop> GetShopByUserIdAsync(string userId)
        {
            return await _shopCollection
                .Find(s => s.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateShopAsync(Shop shop)
        {
            shop.UpdatedAt = DateTime.UtcNow;

            var result = await _shopCollection.ReplaceOneAsync(
                s => s.Id == shop.Id,
                shop
            );

            return result.ModifiedCount > 0;
        }

    }
}
