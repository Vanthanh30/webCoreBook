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
    public class SellerOrderService
    {
        private readonly IMongoCollection<Order> _orders;

        public SellerOrderService(MongoDBService mongoDBService)
        {
            _orders = mongoDBService._orders;
        }

        // Lấy tất cả đơn hàng của seller
        public async Task<List<Order>> GetOrdersBySellerIdAsync(string sellerId, string status = null)
        {
            var filterBuilder = Builders<Order>.Filter;
            var filter = filterBuilder.ElemMatch(x => x.Items, item => item.SellerId == sellerId);

            if (!string.IsNullOrEmpty(status))
            {
                filter = filter & filterBuilder.Eq(x => x.Status, status);
            }

            var orders = await _orders.Find(filter)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();

            // Lọc items chỉ của seller này và tính lại tổng tiền
            foreach (var order in orders)
            {
                order.Items = order.Items.Where(item => item.SellerId == sellerId).ToList();

                // Tính lại tổng tiền cho các sản phẩm của seller này
                order.TotalAmount = order.Items.Sum(item =>
                    item.Price * (1 - item.DiscountPercentage / 100) * item.Quantity);
                order.FinalAmount = order.TotalAmount - order.DiscountAmount;
            }

            return orders;
        }

        // Lấy đơn hàng theo ID
        public async Task<Order> GetOrderByIdAsync(string orderId, string sellerId = null)
        {
            var order = await _orders.Find(x => x.Id == ObjectId.Parse(orderId)).FirstOrDefaultAsync();

            if (order != null && !string.IsNullOrEmpty(sellerId))
            {
                // Lọc items chỉ của seller này
                order.Items = order.Items.Where(item => item.SellerId == sellerId).ToList();

                // Tính lại tổng tiền
                order.TotalAmount = order.Items.Sum(item =>
                    item.Price * (1 - item.DiscountPercentage / 100) * item.Quantity);
                order.FinalAmount = order.TotalAmount - order.DiscountAmount;
            }

            return order;
        }

        // Đếm số lượng đơn hàng theo trạng thái
        public async Task<Dictionary<string, int>> GetOrderStatusCountAsync(string sellerId)
        {
            var filter = Builders<Order>.Filter.ElemMatch(x => x.Items, item => item.SellerId == sellerId);
            var allOrders = await _orders.Find(filter).ToListAsync();

            return new Dictionary<string, int>
            {
                { "All", allOrders.Count },
                { "Pending", allOrders.Count(x => x.Status == "Chờ xác nhận") },
                { "Packing", allOrders.Count(x => x.Status == "Chờ lấy hàng") },
                { "Shipping", allOrders.Count(x => x.Status == "Đang giao") },
                { "Completed", allOrders.Count(x => x.Status == "Đã giao") },
                { "Return", allOrders.Count(x => x.Status == "Đã hủy" || x.Status == "Trả hàng") }
            };
        }

        // Cập nhật trạng thái đơn hàng
        public async Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus)
        {
            var update = Builders<Order>.Update.Set(x => x.Status, newStatus);

            var result = await _orders.UpdateOneAsync(
                x => x.Id == ObjectId.Parse(orderId),
                update
            );

            return result.ModifiedCount > 0;
        }

        // Xác nhận đơn hàng
        public async Task<bool> ConfirmOrderAsync(string orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "Chờ lấy hàng");
        }

        // Xác nhận nhiều đơn hàng
        public async Task<int> ConfirmMultipleOrdersAsync(List<string> orderIds)
        {
            int count = 0;
            foreach (var orderId in orderIds)
            {
                if (await ConfirmOrderAsync(orderId))
                    count++;
            }
            return count;
        }

        // Đánh dấu đã chuẩn bị hàng (chuyển sang đang giao)
        public async Task<bool> ReadyToShipAsync(string orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "Đang giao");
        }

        // Đánh dấu đã giao hàng
        public async Task<bool> CompleteOrderAsync(string orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "Đã giao");
        }

        // Hủy đơn hàng
        public async Task<bool> CancelOrderAsync(string orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "Đã hủy");
        }

        // Hủy nhiều đơn hàng
        public async Task<int> CancelMultipleOrdersAsync(List<string> orderIds)
        {
            int count = 0;
            foreach (var orderId in orderIds)
            {
                if (await CancelOrderAsync(orderId))
                    count++;
            }
            return count;
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
