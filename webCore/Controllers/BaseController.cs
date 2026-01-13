using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using webCore.Helpers;

namespace webCore.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.IsLoggedIn = UserAuthHelper.IsLoggedIn(HttpContext);
            ViewBag.UserName = UserAuthHelper.GetUserName(HttpContext);
            ViewBag.UserToken = UserAuthHelper.GetUserToken(HttpContext);
            ViewBag.UserRoles = UserAuthHelper.GetUserRoles(HttpContext);

            base.OnActionExecuting(context);
        }
    }
}
