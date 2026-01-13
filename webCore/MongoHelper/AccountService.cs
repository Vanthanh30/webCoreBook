using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class AccountService
    {
        private readonly IMongoCollection<User> _accountCollection;
        private readonly RoleService _roleService;

        public AccountService(MongoDBService mongoDBService, RoleService roleService)
        {
            _accountCollection = mongoDBService._accountCollection;
            _roleService = roleService;
        }
        public async Task CreateAccountAsync(User account)
        {
            await _accountCollection.InsertOneAsync(account);
        }
        public async Task<List<User>> GetAccounts()
        {
            var adminRoleIds = await _roleService.GetAdminRoleIdsAsync();

            var roleFilter = Builders<User>.Filter.AnyIn(u => u.RoleId, adminRoleIds);
            var notDeletedFilter = Builders<User>.Filter.Eq(u => u.Deleted, false);

            var filter = Builders<User>.Filter.And(roleFilter, notDeletedFilter);

            return await _accountCollection.Find(filter).ToListAsync();
        }

        public async Task<User> GetAccountByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(a => a.Email, email);
            return await _accountCollection.Find(filter).FirstOrDefaultAsync();
        }

        internal async Task SaveAccountAsync(User account)
        {
            await _accountCollection.InsertOneAsync(account);
        }
        public async Task<User> GetAccountByIdAsync(string id)
        {
            return await _accountCollection.Find(account => account.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAccountAsync(User updatedAccount)
        {
            await _accountCollection.ReplaceOneAsync(account => account.Id == updatedAccount.Id, updatedAccount);
        }

        public async Task DeleteAccountAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(a => a.Id, id);
            var update = Builders<User>.Update
                .Set(a => a.Deleted, true)
                .Set(a => a.DeletedAt, DateTime.UtcNow);

            await _accountCollection.UpdateOneAsync(filter, update);
        }

    }
}
