using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Sign_in() => View();

        public IActionResult Sign_up() => View();

        public IActionResult SellerChannel() => View();



    }
}
