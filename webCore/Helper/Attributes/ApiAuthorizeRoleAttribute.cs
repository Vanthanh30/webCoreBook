using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using webCore.Helpers;

namespace webCore.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiAuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public ApiAuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            // 1. Kiểm tra đăng nhập (từ Session)
            if (!UserAuthHelper.IsLoggedIn(http))
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập!"
                })
                { StatusCode = 401 }; // Unauthorized
                return;
            }

            // 2. Lấy danh sách role của user từ session
            var userRoles = UserAuthHelper.GetUserRoles(http);

            if (string.IsNullOrEmpty(userRoles))
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Không có quyền truy cập!"
                })
                { StatusCode = 403 };
                return;
            }

            // 3. Kiểm tra user có thuộc role yêu cầu hay không
            bool hasRole = _roles.Any(role =>
                userRoles.Contains(role, StringComparison.OrdinalIgnoreCase));

            if (!hasRole)
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Bạn không có quyền thực hiện hành động này!"
                })
                { StatusCode = 403 };
            }
        }
    }
}
