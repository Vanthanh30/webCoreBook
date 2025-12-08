using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class ProductService
    {
        private readonly IMongoCollection<Product_admin> _productCollection;

        public ProductService(MongoDBService mongoDBService)
        {
            _productCollection = mongoDBService._productCollection;
        }

        // Lấy danh sách sản phẩm có Status = "Hoạt động" và nhóm theo Featured (0,1,2,3)
        public async Task<Dictionary<string, List<Product_admin>>> GetProductsGroupedByFeaturedAsync()
        {
            // Lọc sản phẩm: Deleted = false, Status = "Hoạt động", Featured thuộc 0,1,2,3
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false) &
                         Builders<Product_admin>.Filter.Eq(p => p.Status, "Hoạt động") &
                         Builders<Product_admin>.Filter.In(p => p.Featured, new[] { 0, 1, 2, 3 });

            var products = await _productCollection.Find(filter).ToListAsync();

            var groupedProducts = products
                .GroupBy(p => p.Featured)
                .ToDictionary(
                    g => GetFeaturedStatusName((FeaturedStatus)g.Key),
                    g => g.ToList()
                );

            return groupedProducts;
        }

        private string GetFeaturedStatusName(FeaturedStatus status)
        {
            switch (status)
            {
                case FeaturedStatus.Highlighted: return "Nổi bật";
                case FeaturedStatus.New: return "Mới";
                case FeaturedStatus.Suggested: return "Gợi ý";
                case FeaturedStatus.None: return "Không nổi bật";
                default: return "Không nổi bật";
            }
        }

        public async Task<List<Product_admin>> GetProductsByCategoryPositionAsync(int position)
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Position, position) &
                         Builders<Product_admin>.Filter.Eq(p => p.Deleted, false) &
                         Builders<Product_admin>.Filter.Eq(p => p.Status, "Hoạt động");

            return await _productCollection.Find(filter).ToListAsync();
        }

        // Lấy tất cả sản phẩm
        public async Task<List<Product_admin>> GetProductsAsync()
        {
            return await _productCollection.Find(product => true).ToListAsync();
        }

        public async Task<Product_admin> GetProductBreadcrumbByIdAsync(string productId)
        {
            return await _productCollection.Find(p => p.Id == productId).FirstOrDefaultAsync();
        }

        public async Task<int> GetProductCountAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false) &
                         Builders<Product_admin>.Filter.Eq(p => p.Status, "Hoạt động");
            var productCount = await _productCollection.CountDocumentsAsync(filter);
            return (int)productCount;
        }

        // Lấy sản phẩm nổi bật có Status = "Hoạt động" và Featured thuộc 1,2,3
        public async Task<List<Product_admin>> GetFeaturedProductsAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false) &
                         Builders<Product_admin>.Filter.Eq(p => p.Status, "Hoạt động") &
                         Builders<Product_admin>.Filter.In(p => p.Featured, new[]
                         {
                             (int)FeaturedStatus.Highlighted,
                             (int)FeaturedStatus.New,
                             (int)FeaturedStatus.Suggested
                         });

            return await _productCollection.Find(filter).ToListAsync();
        }

        // Lấy danh sách sản phẩm bán chạy: Status = "Hoạt động", Featured thuộc 1,2,3
        // Sắp xếp theo ngày tạo mới nhất
        public async Task<List<Product_admin>> GetBestsellerProductsAsync()
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.Deleted, false) &
                         Builders<Product_admin>.Filter.Eq(p => p.Status, "Hoạt động") &
                         Builders<Product_admin>.Filter.In(p => p.Featured, new[]
                         {
                             (int)FeaturedStatus.Highlighted,  // 1
                             (int)FeaturedStatus.New,          // 2
                             (int)FeaturedStatus.Suggested     // 3
                         });

            var sort = Builders<Product_admin>.Sort.Descending(p => p.CreatedAt);

            return await _productCollection.Find(filter).Sort(sort).Limit(10).ToListAsync();
        }

        public async Task<List<Product_admin>> GetProductsByCategoryIdAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                return new List<Product_admin>();
            }

            var filter = Builders<Product_admin>.Filter.Eq(p => p.CategoryId, categoryId) &
                         Builders<Product_admin>.Filter.Eq(p => p.Deleted, false) &
                         Builders<Product_admin>.Filter.Eq(p => p.Status, "Hoạt động");

            return await _productCollection.Find(filter).ToListAsync();
        }
        public async Task<List<Product_admin>> SearchProductsAsync(string query)
        {
            var filter = Builders<Product_admin>.Filter
                .Regex("Title", new MongoDB.Bson.BsonRegularExpression(query, "i"));

            return await _productCollection.Find(filter).ToListAsync();
        }
        public async Task<Product_admin> GetProductByIdAsync(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return null;
            }

            var filter = Builders<Product_admin>.Filter.Eq(p => p.Id, productId);
            var product = await _productCollection.Find(filter).FirstOrDefaultAsync();

            return product;
        }
    }
}