using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

public class SetLoginStatusFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var isLoggedIn = !string.IsNullOrEmpty(context.HttpContext.Session.GetString("UserToken"));
        context.HttpContext.Items["IsLoggedIn"] = isLoggedIn;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
