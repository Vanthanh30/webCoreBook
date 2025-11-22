using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;

namespace webCore.MongoHelper
{
    public class RoleService
    {
        private readonly IMongoCollection<Role> _roles;

        public RoleService(MongoDBService mongoDBService)
        {
            _roles = mongoDBService._roleCollection;
        }

        public async Task<Role> GetRoleByNameAsync(string name)
        {
            return await _roles.Find(r => r.Name == name).FirstOrDefaultAsync();
        }

        public async Task<Role> GetRoleByIdAsync(string id)
        {
            return await _roles.Find(r => r.Id == id).FirstOrDefaultAsync();
        }
        public async Task<List<string>> GetAdminRoleIdsAsync()
        {
            var filter = Builders<Role>.Filter.Eq(r => r.Name, "Admin");
            var roles = await _roles.Find(filter).ToListAsync();

            return roles.Select(r => r.Id).ToList();
        }
        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _roles.Find(_ => true).ToListAsync();
        }

    }

}
