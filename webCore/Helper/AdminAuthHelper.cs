using Microsoft.AspNetCore.Http;

namespace webCore.Helpers
{
    public static class AdminAuthHelper
    {
        public static string GetAdminId(HttpContext context)
            => context.Session.GetString("AdminId");

        public static string GetAdminToken(HttpContext context)
            => context.Session.GetString("AdminToken");

        public static string GetAdminName(HttpContext context)
            => context.Session.GetString("AdminName");

        public static string GetAdminRoleIds(HttpContext context)
            => context.Session.GetString("RoleId");

        public static string GetAdminRoleNames(HttpContext context)
            => context.Session.GetString("RoleName");

        public static bool IsAdminLoggedIn(HttpContext context)
            => !string.IsNullOrEmpty(GetAdminId(context));

        public static bool HasRole(HttpContext context, string roleName)
        {
            var roles = GetAdminRoleNames(context);
            if (string.IsNullOrEmpty(roles)) return false;

            return roles.Contains(roleName, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
