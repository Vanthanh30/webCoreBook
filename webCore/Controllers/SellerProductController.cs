using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    [AuthorizeRole("Seller")]
    public class SellerProductController : BaseController
    {
        public IActionResult ProductManagement()
        {
            return View();
        }
        public IActionResult EditProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("ProductManagement");
            }
            return View();
        }
        public IActionResult AddProduct()
        {
            return View();
        }
    }
}
