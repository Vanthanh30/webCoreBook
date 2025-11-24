using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    [AuthorizeRole("Seller")]
    public class SellerOrderController : Controller
    {

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
