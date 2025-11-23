using Microsoft.AspNetCore.Mvc;

namespace webCore.Controllers
{
    public class SellerController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult ShopProfile()
        {
            return View();
        }
        public IActionResult ProductManagement()
        {
            return View();
        }
        public IActionResult AddProduct()
        {
            return View();
        }
        public IActionResult EditProduct()
        {
            return View();
        }
        public IActionResult OrderManagement()
        {
            return View();
        }
        public IActionResult OrderDetail()
        {
            return View();
        }
        public IActionResult CancelDetail()
        {
            return View();
        }
    }
}
