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

        // Lưu đơn hàng
        public async Task SaveOrderAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
        }

        // Lấy tất cả đơn hàng của buyer
        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.UserId, userId);
            return await _orders.Find(filter)
                                .SortByDescending(o => o.CreatedAt)
                                .ToListAsync();
        }

        // Lấy tất cả đơn hàng có item thuộc seller
        public async Task<List<Order>> GetOrdersBySellerIdAsync(string sellerId)
        {
            var filter = Builders<Order>.Filter.ElemMatch(o => o.Items, i => i.SellerId == sellerId);
            return await _orders.Find(filter)
                                .SortByDescending(o => o.CreatedAt)
                                .ToListAsync();
        }

        // Lấy đơn hàng theo ID
        public async Task<Order> GetOrderByIdAsync(string id)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.Id, ObjectId.Parse(id));
            return await _orders.Find(filter).FirstOrDefaultAsync();
        }

        // Tổng đơn hàng
        public async Task<int> GetTotalOrdersAsync()
        {
            return (int)await _orders.CountDocumentsAsync(Builders<Order>.Filter.Empty);
        }

        // Tổng doanh thu
        public async Task<decimal> GetTotalRevenueAsync()
        {
            var orders = await _orders.Find(Builders<Order>.Filter.Empty).ToListAsync();
            return orders.Sum(o => o.FinalAmount);
        }
    }
}
