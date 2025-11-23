using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;

namespace webCore.Services
{
    public class MongoDBService
    {
        internal readonly IMongoCollection<Product_admin> _productCollection;
        internal readonly IMongoCollection<User> _userCollection;

        internal IMongoCollection<T> GetCollection<T>(string v)
        {
            throw new NotImplementedException();
        }

        internal IMongoCollection<Product_admin> _detailProductCollection;
        internal readonly IMongoCollection<Cart> _cartCollection;
        internal readonly IMongoCollection<Voucher> _voucherCollection;
        internal readonly IMongoCollection<Category> _categoryCollection;
        internal readonly IMongoCollection<Order> _orders;
        private readonly IMongoDatabase _mongoDatabase;
        internal readonly IMongoCollection<User> _accountCollection;
        internal readonly IMongoCollection<Role> _roleCollection;
        internal readonly IMongoCollection<Shop> _shopCollection;

        public IMongoCollection<ForgotPassword> ForgotPasswords { get; internal set; }

        public MongoDBService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _accountCollection = mongoDatabase.GetCollection<User>("Users");
            _productCollection = mongoDatabase.GetCollection<Product_admin>("Product");
            _userCollection = mongoDatabase.GetCollection<User>("Users");
            ForgotPasswords = mongoDatabase.GetCollection<ForgotPassword>("ForgotPasswords");
            _categoryCollection = mongoDatabase.GetCollection<Category>("Category");
            _detailProductCollection = mongoDatabase.GetCollection<Product_admin>("Product");
            _cartCollection = mongoDatabase.GetCollection<Cart>("Cart");
            _orders = mongoDatabase.GetCollection<Order>("Orders");
            _voucherCollection = mongoDatabase.GetCollection<Voucher>("Vouchers");
            _roleCollection = mongoDatabase.GetCollection<Role>("Roles");
            _shopCollection = mongoDatabase.GetCollection<Shop>("Shops");
        }

    }
}
