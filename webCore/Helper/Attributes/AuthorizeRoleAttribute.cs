using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using webCore.Helpers;

namespace webCore.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _role;

        public AuthorizeRoleAttribute(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            // 1) Kiểm tra đăng nhập
            if (!AuthHelper.IsLoggedIn(http))
            {
                context.Result = new RedirectToActionResult("Sign_in", "User", null);
                return;
            }

            // 2) Kiểm tra role
            var roles = AuthHelper.GetUserRoles(http);

            if (string.IsNullOrEmpty(roles) || !roles.Contains(_role, StringComparison.OrdinalIgnoreCase))
            {
                // Không đủ quyền → chuyển sang trang 403
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }
        }
    }
}
