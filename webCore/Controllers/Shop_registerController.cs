using Microsoft.AspNetCore.Mvc;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    public class Shop_registerController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
