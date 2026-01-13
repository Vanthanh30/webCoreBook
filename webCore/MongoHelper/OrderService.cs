using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class OrderService
    {
        private readonly IMongoCollection<Order> _orders;

        public OrderService(MongoDBService mongoDBService)
        {
            _orders = mongoDBService._orders;
        }

        public async Task SaveOrderAsync(Order order)
        {
            if (order.Id == ObjectId.Empty)
            {
                await _orders.InsertOneAsync(order);
            }
            else
            {
                var filter = Builders<Order>.Filter.Eq(x => x.Id, order.Id);
                await _orders.ReplaceOneAsync(filter, order, new ReplaceOptions { IsUpsert = true });
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.UserId, userId);
            return await _orders.Find(filter)
                                .SortByDescending(o => o.CreatedAt)
                                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersBySellerIdAsync(string sellerId)
        {
            var filter = Builders<Order>.Filter.ElemMatch(o => o.Items, i => i.SellerId == sellerId);
            return await _orders.Find(filter)
                                .SortByDescending(o => o.CreatedAt)
                                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(string id)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, ObjectId.Parse(id));
            return await _orders.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return (int)await _orders.CountDocumentsAsync(Builders<Order>.Filter.Empty);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var orders = await _orders.Find(Builders<Order>.Filter.Empty).ToListAsync();
            return orders.Sum(o => o.FinalAmount);
        }

    }
}
