using Microsoft.AspNetCore.Http;

namespace webCore.Helpers
{
    public static class UserAuthHelper
    {
        public static string GetUserId(HttpContext context)
            => context.Session.GetString("UserId");
        public static string GetUserToken(HttpContext context)
            => context.Session.GetString("UserToken");
        public static string GetUserName(HttpContext context)
            => context.Session.GetString("UserName");

        public static string GetUserRoles(HttpContext context)
            => context.Session.GetString("UserRoles");

        public static bool IsLoggedIn(HttpContext context)
            => !string.IsNullOrEmpty(GetUserId(context));
    }
}
