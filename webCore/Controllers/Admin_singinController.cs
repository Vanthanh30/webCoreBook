using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;
using webCore.Helpers; // ← THÊM DÒNG NÀY

namespace webCore.Controllers
{
    public class Admin_singinController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();  
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}