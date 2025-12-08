using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Services;
using webCore.MongoHelper;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace webCore.Controllers.ApiControllers
{
    [Route("api/seller")]
    [ApiController]
    public class SellerDashboardApiController : ControllerBase
    {
        private readonly CategoryProduct_adminService _productService;
        private readonly SellerOrderService _sellerOrderService;
        private readonly ILogger<SellerDashboardApiController> _logger;

        public SellerDashboardApiController(
            CategoryProduct_adminService productService,
            SellerOrderService sellerOrderService,
            ILogger<SellerDashboardApiController> logger)
        {
            _productService = productService;
            _sellerOrderService = sellerOrderService;
            _logger = logger;
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập."
                });
            }

            try
            {
                var allProducts = await _productService.GetProduct();
                var sellerProducts = allProducts
                    .Where(p => p.SellerId == sellerId && !p.Deleted)
                    .ToList();

                int totalProducts = sellerProducts.Count;
                int outOfStockProducts = sellerProducts.Count(p => p.Stock == 0);

                var productIds = sellerProducts.Select(p => p.Id).ToList();

                int pendingOrders = 0;
                int processingOrders = 0;
                int shippingOrders = 0;
                int completedOrders = 0;
                int cancelledOrders = 0;

                if (productIds.Any())
                {
                    try
                    {
                        var orderStats = await _sellerOrderService.GetOrderStatsBySellerAsync(productIds);

                        pendingOrders = orderStats.GetValueOrDefault("Chờ xác nhận", 0);
                        processingOrders = orderStats.GetValueOrDefault("Chờ lấy hàng", 0);
                        shippingOrders = orderStats.GetValueOrDefault("Đang giao", 0);
                        completedOrders = orderStats.GetValueOrDefault("Đã giao", 0);
                        cancelledOrders = orderStats.GetValueOrDefault("Đã hủy", 0);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not fetch order statistics for seller {SellerId}", sellerId);
                    }
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        pendingOrders,      
                        processingOrders,   
                        shippingOrders,    
                        completedOrders,    
                        cancelledOrders,   

                        totalProducts,    
                        outOfStockProducts,

                        sellerId,
                        hasProducts = totalProducts > 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats for seller");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thống kê dashboard: " + ex.Message
                });
            }
        }

        [HttpGet("revenue-stats")]
        public async Task<IActionResult> GetRevenueStats()
        {
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập."
                });
            }

            try
            {
                var allProducts = await _productService.GetProduct();
                var sellerProducts = allProducts
                    .Where(p => p.SellerId == sellerId && !p.Deleted)
                    .ToList();

                var productIds = sellerProducts.Select(p => p.Id).ToList();

                int totalOrders = 0;
                decimal totalRevenue = 0;

                if (productIds.Any())
                {
                    try
                    {
                        var sellerOrders = await _sellerOrderService.GetOrdersBySellerIdAsync(sellerId, productIds);
                        totalOrders = sellerOrders.Count;

                        totalRevenue = sellerOrders
                            .Where(o => o.Status == "Đã giao" || o.Status == "Completed" || o.Status == "Delivered")
                            .Sum(o => o.FinalAmount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not calculate revenue stats");
                    }
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orders = totalOrders,
                        revenue = totalRevenue
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue stats");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy thống kê doanh thu: " + ex.Message
                });
            }
        }

        [HttpGet("revenue-chart-data")]
        public async Task<IActionResult> GetRevenueChartData()
        {
            var sellerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sellerId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập."
                });
            }

            try
            {
                var allProducts = await _productService.GetProduct();
                var sellerProducts = allProducts
                    .Where(p => p.SellerId == sellerId && !p.Deleted)
                    .ToList();

                var productIds = sellerProducts.Select(p => p.Id).ToList();

                var orders = new List<object>();

                if (productIds.Any())
                {
                    var sellerOrders = await _sellerOrderService.GetOrdersBySellerIdAsync(sellerId, productIds);

                    orders = sellerOrders.Select(o => new
                    {
                        orderId = o.Id.ToString(),
                        createdAt = o.CreatedAt,
                        status = o.Status,
                        finalAmount = o.FinalAmount
                    }).ToList<object>();
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orders = orders
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue chart data");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy dữ liệu biểu đồ: " + ex.Message
                });
            }
        }
    }
}
