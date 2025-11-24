using Microsoft.AspNetCore.Mvc;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    [AuthorizeRole("Buyer")]
    public class Shop_registerController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
