using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;
using System;

namespace webCore.Services
{
    public class Order_adminService
    {
        private readonly IMongoCollection<Order> _orderCollection;

        public Order_adminService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _orderCollection = mongoDatabase.GetCollection<Order>("Orders"); // Tên collection là "Orders"
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderCollection.Find(order => true).ToListAsync();
        }
        public async Task<List<Order>> GetRecentOrdersAsync()
        {
            try
            {
                var recentOrders = await _orderCollection
                    .Find(order => true) 
                    .Sort(Builders<Order>.Sort.Descending(order => order.CreatedAt)) 
                    .Limit(3) 
                    .ToListAsync();

                return recentOrders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching recent orders: {ex.Message}");
                return new List<Order>(); 
            }
        }


        public async Task<Order> GetOrderByIdAsync(string id)
        {
            return await _orderCollection.Find(order => order.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(newStatus))
            {
                return false;
            }

            var validStatuses = new List<string> { "Đang chờ duyệt", "Đã duyệt", "Đã hủy" };

            if (!validStatuses.Contains(newStatus))
            {
                throw new ArgumentException("Invalid order status provided.");
            }

            try
            {
                var update = Builders<Order>.Update.Set(order => order.Status, newStatus);

                var result = await _orderCollection.UpdateOneAsync(
                    order => order.Id == ObjectId.Parse(orderId), 
                    update
                );

                return result.IsAcknowledged && result.ModifiedCount > 0;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error in updating order status: {ex.Message}");
                return false;
            }
        }
    }
}
