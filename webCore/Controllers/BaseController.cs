using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using webCore.Helpers;

namespace webCore.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Gửi login info sang View
            ViewBag.IsLoggedIn = AuthHelper.IsLoggedIn(HttpContext);
            ViewBag.UserName = AuthHelper.GetUserName(HttpContext);
            ViewBag.UserRoles = AuthHelper.GetUserRoles(HttpContext);

            base.OnActionExecuting(context);
        }
    }
}
