using Microsoft.AspNetCore.Mvc;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    [AuthorizeRole("Seller")]
    public class SellerController : BaseController
    {
        public IActionResult ShopProfile()
        {
            return View();
        }
    }
}
