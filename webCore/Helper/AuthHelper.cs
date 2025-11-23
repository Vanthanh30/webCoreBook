using Microsoft.AspNetCore.Http;

namespace webCore.Helpers
{
    public static class AuthHelper
    {
        public static string GetUserId(HttpContext context)
            => context.Session.GetString("UserId");

        public static string GetUserName(HttpContext context)
            => context.Session.GetString("UserName");

        public static string GetUserRoles(HttpContext context)
            => context.Session.GetString("UserRoles");

        public static bool IsLoggedIn(HttpContext context)
            => !string.IsNullOrEmpty(GetUserId(context));
    }
}
