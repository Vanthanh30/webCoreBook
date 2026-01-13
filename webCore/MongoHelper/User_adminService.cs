using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;
using System;
using System.Linq;

namespace webCore.MongoHelper
{
    public class User_adminService
    {
        private readonly IMongoCollection<User> _userAdminCollection;

        public User_adminService(IConfiguration configuration)
        {
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _userAdminCollection = mongoDatabase.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetAllUsersAsync(List<string> excludeRoleIds = null)
        {
            var allUsers = await _userAdminCollection.Find(user => true).ToListAsync();

            if (excludeRoleIds != null && excludeRoleIds.Count > 0)
            {
                return allUsers
                    .Where(u =>
                        u.RoleId != null &&
                        u.RoleId.Count > 0 &&
                        !u.RoleId.Any(roleId => excludeRoleIds.Contains(roleId))
                    )
                    .ToList();
            }

            return allUsers;
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _userAdminCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateUserStatusAsync(string id, int newStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("ID không hợp lệ.", nameof(id));
                }

                var user = await _userAdminCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new Exception("Người dùng không tồn tại.");
                }

                if (!user.Status.HasValue)
                {
                    user.Status = 1;
                }

                var filter = Builders<User>.Filter.Eq(user => user.Id, id);

                var update = Builders<User>.Update.Set(user => user.Status, newStatus);

                var result = await _userAdminCollection.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật trạng thái người dùng: {ex.Message}");
                return false;
            }
        }
    }
}

