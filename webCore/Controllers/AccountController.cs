using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using webCore.MongoHelper;
using System.Linq;
using webCore.Helpers; 

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class AccountController : Controller
    {
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            return View();
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        public async Task<IActionResult> Delete(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            return View();
        }
    }
}