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


        // Action xử lý đăng xuất
/*        [HttpPost]
        public IActionResult Sign_out()
        {
            // Xóa thông tin UserName và UserToken khỏi session
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.Remove("UserToken");

            // Chuyển hướng về trang chủ
            return RedirectToAction("Index", "Home");
        }*/

    }
}
