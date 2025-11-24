using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using webCore.Helpers;

namespace webCore.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            // 1) Kiểm tra đăng nhập
            if (!UserAuthHelper.IsLoggedIn(http))
            {
                context.Result = new RedirectToActionResult("Sign_in", "User", null);
                return;
            }

            // 2) Lấy danh sách role từ session
            var userRolesString = UserAuthHelper.GetUserRoles(http);

            if (string.IsNullOrEmpty(userRolesString))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }

            // Roles lưu dạng JSON → tách ra
            var userRoles = userRolesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(r => r.Trim().ToLower())
                                           .ToList();

            // 3) Kiểm tra user có ít nhất 1 role hợp lệ không
            bool allowed = _roles.Any(requiredRole =>
                userRoles.Contains(requiredRole.ToLower())
            );

            if (!allowed)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }
        }
    }
}
