using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;

namespace webCore.Controllers
{
    public class ChatController : BaseController
    {
        public IActionResult Index(string mode = "buyer")
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Sign_in", "User"); 

            mode = (mode ?? "buyer").ToLower().Trim();
            if (mode != "buyer" && mode != "seller") mode = "buyer";

            ViewBag.Mode = mode;
            ViewBag.UserId = userId;

            return View();
        }
    }
}