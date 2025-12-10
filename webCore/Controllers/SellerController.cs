using Microsoft.AspNetCore.Mvc;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    public class SellerController : BaseController
    {
        public IActionResult ShopProfile()
        {
            return View();
        }
    }
}
