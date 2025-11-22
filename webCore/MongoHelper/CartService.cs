using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;

        public CartService(MongoDBService mongoDBService)
        {
            _cartCollection = mongoDBService._cartCollection;
        }

        // Lấy giỏ hàng của người dùng
        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _cartCollection.Find(cart => cart.UserId == userId).FirstOrDefaultAsync();
        }

        // Thêm hoặc cập nhật giỏ hàng
        public async Task AddOrUpdateCartAsync(Cart cart)
        {
            var existingCart = await GetCartByUserIdAsync(cart.UserId);
            if (existingCart == null)
            {
                // Nếu giỏ hàng chưa tồn tại, tạo mới
                await _cartCollection.InsertOneAsync(cart);
            }
            else
            {
                // Cập nhật giỏ hàng hiện có
                await _cartCollection.ReplaceOneAsync(c => c.Id == existingCart.Id, cart);
            }
        }
        public async Task RemoveItemsFromCartAsync(string userId, List<string> productIds)
        {
            if (!string.IsNullOrEmpty(userId) && productIds != null && productIds.Any())
            {
                var filter = Builders<Cart>.Filter.Eq(cart => cart.UserId, userId);
                var update = Builders<Cart>.Update.PullFilter(cart => cart.Items, item => productIds.Contains(item.ProductId));

                await _cartCollection.UpdateOneAsync(filter, update);
            }
        }
        //xóa nhiều sản phẩm khỏi giỏ hàng
        public async Task<bool> RemoveMutipleItemsFromCartAsync(string userId, List<string> productIds)
        {
            if (string.IsNullOrEmpty(userId) || productIds == null || !productIds.Any())
                return false;

            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return false;

            // Xóa các item trong Items trùng ProductId
            cart.Items.RemoveAll(item => productIds.Contains(item.ProductId.Trim()));

            // Cập nhật MongoDB
            await AddOrUpdateCartAsync(cart);

            return true;
        }
        
    }
}
