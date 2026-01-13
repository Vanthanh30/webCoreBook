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

            if (!UserAuthHelper.IsLoggedIn(http))
            {
                context.Result = new RedirectToActionResult("Sign_in", "User", null);
                return;
            }

            var userRolesString = UserAuthHelper.GetUserRoles(http);

            if (string.IsNullOrEmpty(userRolesString))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Error", null);
                return;
            }

            var userRoles = userRolesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(r => r.Trim().ToLower())
                                           .ToList();

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
