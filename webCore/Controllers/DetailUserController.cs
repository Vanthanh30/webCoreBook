using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

public class DetailUserController : Controller
{
    private readonly MongoDBService _mongoDBService;
    private readonly UserService _userService;
    private readonly ProductService _productService;
    private readonly CloudinaryService _cloudinaryService;

    public DetailUserController(MongoDBService mongoDBService, UserService userService, ProductService productService, CloudinaryService cloudinaryService)
    {
        _mongoDBService = mongoDBService;
        _productService = productService;
        _userService = userService;
        _cloudinaryService = cloudinaryService;
    }
    // Phương thức tìm kiếm sản phẩm
    public async Task<IActionResult> Search(string searchQuery)
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            return PartialView("_ProductList", new List<Product_admin>());
        }

        // Tìm kiếm sản phẩm từ MongoDB
        var allProducts = await _productService.GetProductsAsync();
        var searchResults = allProducts
            .Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return PartialView("_ProductList", searchResults);
    }
    // GET: DetailUser/Index
    [HttpGet]
    [ServiceFilter(typeof(SetLoginStatusFilter))]
    public async Task<IActionResult> Index()
    {
        var isLoggedIn = HttpContext.Session.GetString("UserToken") != null;

        // Truyền thông tin vào ViewBag hoặc Model để sử dụng trong View
        ViewBag.IsLoggedIn = isLoggedIn;
        var userToken = HttpContext.Session.GetString("UserToken"); 
        if (string.IsNullOrEmpty(userToken))
        {
            return RedirectToAction("SignIn", "User");
        }
        var userId = HttpContext.Session.GetString("UserId");
        var user = await _userService.GetUserByIdAsync(userId); 
        if (user == null)
        {
            TempData["Message"] = "Không tìm thấy thông tin người dùng.";
            return View();
        }

        // Đảm bảo ảnh đại diện hiện tại được gửi sang view
        ViewBag.ProfileImage = user.ProfileImage;

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Index(User model, IFormFile ProfileImage)
    {
        try
        {
            var userToken = HttpContext.Session.GetString("UserToken");
            if (string.IsNullOrEmpty(userToken))
            {
                return RedirectToAction("SignIn", "User");
            }
            var userId = HttpContext.Session.GetString("UserId");
            // Lấy thông tin người dùng hiện tại từ MongoDB
            var currentUser = await _userService.GetUserByIdAsync(userId);
            if (currentUser == null)
            {
                TempData["Message"] = "Không tìm thấy thông tin người dùng.";
                return View(model);
            }

            // Cập nhật thông tin người dùng
            currentUser.Name = model.Name;
            HttpContext.Session.SetString("UserName", model.Name); // Cập nhật session với tên mới
            bool isPhone = await _userService.IsPhoneUsedAsync(model.Phone, userId);

            if (isPhone)
            {
                ModelState.AddModelError("Phone", "Số điện thoại này đã được sử dụng bởi người dùng khác!");
                ViewBag.ProfileImage = currentUser.ProfileImage;
                return View(model);
            }

            currentUser.Phone = model.Phone;
            currentUser.Gender = model.Gender;

            // Đảm bảo ngày sinh được lưu với DateTimeKind.Unspecified
            currentUser.Birthday = model.Birthday.HasValue
                ? DateTime.SpecifyKind(model.Birthday.Value.Date, DateTimeKind.Unspecified)
                : currentUser.Birthday;

            currentUser.Address = model.Address;

            // Cập nhật mật khẩu nếu có thay đổi
            if (!string.IsNullOrEmpty(model.Password))
            {
                currentUser.Password = PasswordHasher.HashPassword(model.Password);
            }

            // Xử lý ảnh đại diện
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(ProfileImage);

                // Nếu upload thành công (URL không rỗng), gán vào model
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    currentUser.ProfileImage = imageUrl;
                }
            }

            // Thực hiện cập nhật thông tin người dùng trong MongoDB
            var updateResult = await _userService.UpdateUserAsync(currentUser);
            TempData["Message"] = updateResult ? "Cập nhật thông tin thành công!" : "Không có thay đổi nào được thực hiện.";

            // Đảm bảo ảnh đại diện hiển thị lại sau khi cập nhật
            ViewBag.ProfileImage = currentUser.ProfileImage;
        }
        catch (Exception ex)
        {
            TempData["Message"] = $"Đã xảy ra lỗi: {ex.Message}";
            var user = await _userService.GetUserByIdAsync(HttpContext.Session.GetString("UserId"));
            ViewBag.ProfileImage = user?.ProfileImage;
        }

        return View(model);
    }
}

