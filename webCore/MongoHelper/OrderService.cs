
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
    public class OrderService

    {
        private readonly IMongoCollection<Order> _orders;

        public OrderService(MongoDBService mongoDBService)
        {
            _orders = mongoDBService._orders;
        }

        public async Task SaveOrderAsync(Order order)
        {
            await _orders.InsertOneAsync(order);
        }
        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _orders
                .Find(order => order.UserId == userId)
                .ToListAsync();
        }
        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _orders.Find(order => order.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
        }
        public async Task<int> GetTotalOrdersAsync()
        {
            var filter = Builders<Order>.Filter.Empty;
            return (int)await _orders.CountDocumentsAsync(filter);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var filter = Builders<Order>.Filter.Empty;
            var orders = await _orders.Find(filter).ToListAsync();
            return orders.Sum(order => order.FinalAmount);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orders.Find(_ => true).ToListAsync();
        }

        public async Task<List<Order>> GetOrdersBySellerIdAsync(string sellerId, List<string> productIds)
        {
            var filter = Builders<Order>.Filter.ElemMatch(o => o.Items,
                item => productIds.Contains(item.ProductId));

            return await _orders.Find(filter).ToListAsync();
        }

        public async Task<int> GetOrderCountByStatusAsync(List<string> productIds, string status)
        {
            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Eq(o => o.Status, status),
                Builders<Order>.Filter.ElemMatch(o => o.Items,
                    item => productIds.Contains(item.ProductId))
            );

            return (int)await _orders.CountDocumentsAsync(filter);
        }

        public async Task<Dictionary<string, int>> GetOrderStatsBySellerAsync(List<string> productIds)
        {
            var filter = Builders<Order>.Filter.ElemMatch(o => o.Items,
                item => productIds.Contains(item.ProductId));

            var orders = await _orders.Find(filter).ToListAsync();

            return new Dictionary<string, int>
    {
        { "Chờ xác nhận", orders.Count(o => o.Status == "Chờ xác nhận" || o.Status == "Pending") },
        { "Chờ lấy hàng", orders.Count(o => o.Status == "Chờ lấy hàng" || o.Status == "Processing") },
        { "Đang giao", orders.Count(o => o.Status == "Đang giao" || o.Status == "Shipping") },
        { "Đã giao", orders.Count(o => o.Status == "Đã giao" || o.Status == "Completed" || o.Status == "Delivered") },
        { "Đã hủy", orders.Count(o => o.Status == "Đã hủy" || o.Status == "Cancelled") }
    };
        }
        public async Task<decimal> GetSellerRevenueAsync(List<string> productIds)
        {
            if (productIds == null || !productIds.Any())
            {
                return 0;
            }

            var filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.ElemMatch(o => o.Items,
                    item => productIds.Contains(item.ProductId)),
                Builders<Order>.Filter.In(o => o.Status,
                    new[] { "Đã giao", "Completed", "Delivered" })
            );

            var completedOrders = await _orders.Find(filter).ToListAsync();

            decimal totalRevenue = 0;
            foreach (var order in completedOrders)
            {
                foreach (var item in order.Items.Where(i => productIds.Contains(i.ProductId)))
                {
                    totalRevenue += item.Price * item.Quantity;
                }
            }

            return totalRevenue;
        }

    }

}