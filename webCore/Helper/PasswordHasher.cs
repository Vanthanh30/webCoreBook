using BCrypt.Net;

namespace webCore.Helpers
{
    public static class PasswordHasher
    {
        // Mã hóa mật khẩu
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        // Kiểm tra mật khẩu
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}