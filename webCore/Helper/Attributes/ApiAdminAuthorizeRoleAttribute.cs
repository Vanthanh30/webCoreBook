using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using webCore.Helpers;   

namespace webCore.Helper.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiAdminAuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public ApiAdminAuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            if (!AdminAuthHelper.IsAdminLoggedIn(http))
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Bạn chưa đăng nhập (admin)!"
                })
                { StatusCode = 401 };
                return;
            }

            var adminRoles = AdminAuthHelper.GetAdminRoleNames(http);

            if (string.IsNullOrEmpty(adminRoles))
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Không có quyền truy cập (admin)!"
                })
                { StatusCode = 403 };
                return;
            }

            bool hasRole = _roles.Any(role =>
                adminRoles.Contains(role, StringComparison.OrdinalIgnoreCase));

            if (!hasRole)
            {
                context.Result = new JsonResult(new
                {
                    success = false,
                    message = "Bạn không có quyền thực hiện API này (admin)!"
                })
                { StatusCode = 403 };
            }
        }
    }
}
