using Microsoft.AspNetCore.Mvc;

namespace webCore.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
