using MongoDB.Driver;
using webCore.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson;
using System;

namespace webCore.MongoHelper
{
    public class User_adminService
    {
        private readonly IMongoCollection<User> _userAdminCollection;

        public User_adminService(IConfiguration configuration)
        {
            // Kết nối MongoDB từ cấu hình
            var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _userAdminCollection = mongoDatabase.GetCollection<User>("Users"); // Tên collection là "Orders"
        }

        // Lấy tất cả
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userAdminCollection.Find(user => true).ToListAsync();
        }
        public async Task<User> GetUserByIdAsync(string id)
        {
            // Chuyển đổi id (string) sang Guid và truy vấn người dùng trong cơ sở dữ liệu
           // var userId = Guid.Parse(id); // Chuyển đổi id sang Guid
            return await _userAdminCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
        }
        public async Task<bool> UpdateUserStatusAsync(string id, int newStatus)
        {
            try
            {
                // Kiểm tra giá trị id
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("ID không hợp lệ.", nameof(id));
                }

                // Chuyển đổi id từ string sang Guid
               // var userId = Guid.Parse(id);

                // Tìm người dùng trong cơ sở dữ liệu
                var user = await _userAdminCollection.Find(user => user.Id == id).FirstOrDefaultAsync();

                if (user == null)
                {
                    throw new Exception("Người dùng không tồn tại.");
                }

                // Kiểm tra nếu status hiện tại là null, thiết lập giá trị mặc định là 1 nếu null
                if (!user.Status.HasValue)
                {
                    user.Status = 1;  // Nếu chưa có giá trị, set mặc định là 1
                }

                // Tạo bộ lọc để xác định người dùng cần cập nhật
                var filter = Builders<User>.Filter.Eq(user => user.Id, id);

                // Tạo bộ cập nhật trạng thái
                var update = Builders<User>.Update.Set(user => user.Status, newStatus);

                // Thực hiện cập nhật
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
