using Microsoft.AspNetCore.Http;

namespace webCore.Helpers
{
    public static class AdminAuthHelper
    {
        // Lấy ID admin
        public static string GetAdminId(HttpContext context)
            => context.Session.GetString("AdminId");

        // Lấy token admin
        public static string GetAdminToken(HttpContext context)
            => context.Session.GetString("AdminToken");

        // Lấy tên admin
        public static string GetAdminName(HttpContext context)
            => context.Session.GetString("AdminName");

        // Lấy danh sách RoleId (chuỗi)
        public static string GetAdminRoleIds(HttpContext context)
            => context.Session.GetString("RoleId");

        // Lấy danh sách RoleName (chuỗi)
        public static string GetAdminRoleNames(HttpContext context)
            => context.Session.GetString("RoleName");

        // Admin đã đăng nhập?
        public static bool IsAdminLoggedIn(HttpContext context)
            => !string.IsNullOrEmpty(GetAdminId(context));

        // Admin có role cụ thể không?
        public static bool HasRole(HttpContext context, string roleName)
        {
            var roles = GetAdminRoleNames(context);
            if (string.IsNullOrEmpty(roles)) return false;

            return roles.Contains(roleName, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
