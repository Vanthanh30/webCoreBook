using Microsoft.AspNetCore.Mvc;

namespace webCore.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
