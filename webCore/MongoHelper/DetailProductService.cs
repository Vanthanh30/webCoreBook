using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class DetailProductService
    {
        private readonly IMongoCollection<Product_admin> _detailProductCollection;

        public DetailProductService(MongoDBService mongoDBService)
        {
            _detailProductCollection = mongoDBService._detailProductCollection;
        }

        public async Task<Product_admin> GetProductByIdAsync(string productId)
        {
            FilterDefinition<Product_admin> filter;

            if (ObjectId.TryParse(productId, out var objectId))
            {
                filter = Builders<Product_admin>.Filter.Eq("_id", objectId);
            }
            else
            {
                filter = Builders<Product_admin>.Filter.Eq("_id", productId);
            }

            return await _detailProductCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<List<Product_admin>> GetProductsByCategoryAsync(string categoryId)
        {
            var filter = Builders<Product_admin>.Filter.Eq(p => p.CategoryId, categoryId);
            var products = await _detailProductCollection.Find(filter).ToListAsync();  

            return products;
        }
    }
}
