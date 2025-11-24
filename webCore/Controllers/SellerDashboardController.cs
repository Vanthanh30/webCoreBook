using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers.Attributes;

namespace webCore.Controllers
{
    /*[AuthorizeRole("Seller")]*/
    public class SellerDashboardController : BaseController
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
