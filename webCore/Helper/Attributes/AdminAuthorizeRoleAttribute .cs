using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers;

namespace webCore.Helper.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public AdminAuthorizeRoleAttribute(string role)
        {
            _requiredRole = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            if (!AdminAuthHelper.IsAdminLoggedIn(http))
            {
                context.Result = new RedirectToActionResult("AdminLogin", "Admin", null);
                return;
            }

            if (!AdminAuthHelper.HasRole(http, _requiredRole))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }
        }
    }

}
