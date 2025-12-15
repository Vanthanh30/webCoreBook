using Microsoft.AspNetCore.Mvc;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    [AuthorizeRole("Seller")]
    public class Seller_shopController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
