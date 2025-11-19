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

namespace webCore.Controllers
{
    [AuthenticateHelper]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AccountService accountService, CloudinaryService cloudinaryService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // Combine the two Index methods
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            // Fetch admin name and avatar from session
            var adminName = HttpContext.Session.GetString("AdminName");
            var adminId = HttpContext.Session.GetString("AdminId");

            ViewBag.AdminName = adminName;

            // Lấy thông tin admin để hiển thị avatar
            if (!string.IsNullOrEmpty(adminId))
            {
                var admin = await _accountService.GetAccountByIdAsync(adminId);
                ViewBag.AdminAvatar = admin?.Avatar ?? "";
            }

            try
            {
                // Fetch accounts asynchronously from MongoDB
                var accounts = await _accountService.GetAccounts();

                // Calculate pagination details
                var totalAccounts = accounts.Count();
                var totalPages = (int)Math.Ceiling(totalAccounts / (double)pageSize);

                // Get accounts for the current page
                var accountsOnPage = accounts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Pass pagination data to the view
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(accountsOnPage); // Pass the accounts for the current page
            }
            catch (Exception ex)
            {
                // Log error if fetching accounts fails
                _logger.LogError(ex, "Error fetching accounts from MongoDB.");
                return View("Error"); // Or any other error view
            }
        }

        public async Task<IActionResult> Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            var adminId = HttpContext.Session.GetString("AdminId");

            ViewBag.AdminName = adminName;

            // Lấy thông tin admin để hiển thị avatar
            if (!string.IsNullOrEmpty(adminId))
            {
                var admin = await _accountService.GetAccountByIdAsync(adminId);
                ViewBag.AdminAvatar = admin?.Avatar ?? "";
            }

            return View();
        }

        // POST: Account/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account_admin account, IFormFile Avatar)
        {
            if (ModelState.IsValid)
            {
                var existingAccount = (await _accountService.GetAccounts())
                    .FirstOrDefault(a => a.Email == account.Email);

                if (existingAccount != null)
                {
                    // If email already exists, add error message and return to view
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    return View(account);
                }

                account.Id = Guid.NewGuid().ToString();
                account.RoleId = account.RoleId;
                // Handle avatar upload if provided
                if (Avatar != null && Avatar.Length > 0)
                {
                    try
                    {
                        account.Avatar = await _cloudinaryService.UploadImageAsync(Avatar);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image to Cloudinary.");
                        ModelState.AddModelError("", "Failed to upload image. Please try again.");
                        return View(account);
                    }
                }

                try
                {
                    // Save the new account to MongoDB
                    await _accountService.SaveAccountAsync(account);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving account to MongoDB.");
                    ModelState.AddModelError("", "Could not save account to database. Please try again.");
                    return View(account);
                }

                // Redirect to Index page after successful creation
                return RedirectToAction(nameof(Index));
            }

            return View(account);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                // Lấy admin info để hiển thị header
                var adminName = HttpContext.Session.GetString("AdminName");
                var adminId = HttpContext.Session.GetString("AdminId");

                ViewBag.AdminName = adminName;

                if (!string.IsNullOrEmpty(adminId))
                {
                    var admin = await _accountService.GetAccountByIdAsync(adminId);
                    ViewBag.AdminAvatar = admin?.Avatar ?? "";
                }

                // Lấy tài khoản từ MongoDB
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound();

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account for editing.");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Account_admin updatedAccount, IFormFile Avatar, string Password)
        {
            if (id != updatedAccount.Id)
                return BadRequest("Account ID mismatch.");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy tài khoản hiện tại từ MongoDB
                    var existingAccount = await _accountService.GetAccountByIdAsync(id);
                    if (existingAccount == null)
                        return NotFound("Account not found.");

                    // Kiểm tra trùng email (ngoại trừ email của tài khoản hiện tại)
                    var duplicateEmailAccount = (await _accountService.GetAccounts())
                        .FirstOrDefault(a => a.Email == updatedAccount.Email && a.Id != id);
                    if (duplicateEmailAccount != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                        return View(updatedAccount);
                    }

                    // Cập nhật các trường
                    existingAccount.FullName = updatedAccount.FullName;
                    existingAccount.Email = updatedAccount.Email;
                    existingAccount.Phone = updatedAccount.Phone;
                    existingAccount.Status = updatedAccount.Status;
                    existingAccount.RoleId = updatedAccount.RoleId;

                    // Nếu mật khẩu không trống thì cập nhật mật khẩu mới
                    if (!string.IsNullOrEmpty(Password))
                    {
                        existingAccount.Password = Password; // Lưu mật khẩu mới (nên mã hóa mật khẩu trước khi lưu)
                    }

                    // Xử lý tải ảnh đại diện nếu có
                    if (Avatar != null && Avatar.Length > 0)
                    {
                        try
                        {
                            existingAccount.Avatar = await _cloudinaryService.UploadImageAsync(Avatar);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error uploading avatar.");
                            ModelState.AddModelError("", "Failed to upload avatar. Please try again.");
                            return View(updatedAccount);
                        }
                    }

                    // Ghi thời gian cập nhật
                    existingAccount.UpdatedAt = DateTime.UtcNow;

                    // Lưu thay đổi vào MongoDB
                    await _accountService.UpdateAccountAsync(existingAccount);

                    // Quay lại trang danh sách tài khoản
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating account.");
                    ModelState.AddModelError("", "Could not update account. Please try again.");
                }
            }

            // Trả lại view với thông tin tài khoản đã chỉnh sửa nếu có lỗi
            return View(updatedAccount);
        }

        // GET: Account/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                // Lấy admin info để hiển thị header
                var adminName = HttpContext.Session.GetString("AdminName");
                var adminId = HttpContext.Session.GetString("AdminId");

                ViewBag.AdminName = adminName;

                if (!string.IsNullOrEmpty(adminId))
                {
                    var admin = await _accountService.GetAccountByIdAsync(adminId);
                    ViewBag.AdminAvatar = admin?.Avatar ?? "";
                }

                // Fetch the account by ID from MongoDB
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound();

                // Return the account to the view for confirmation
                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account for deletion.");
                return View("Error");
            }
        }

        // POST: Account/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                // Lấy tài khoản theo ID để đảm bảo tài khoản tồn tại
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound();

                // Xóa tài khoản khỏi MongoDB
                await _accountService.DeleteAccountAsync(id);

                // Quay lại trang Index sau khi xóa thành công
                return RedirectToAction(nameof(Index)); // Quay lại danh sách tài khoản
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tài khoản.");
                return View("Error");
            }
        }

        //role
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.User.Identity.Name;

            // Lấy thông tin người dùng từ MongoDB
            var user = (await _accountService.GetAccounts())
                       .FirstOrDefault(u => u.Email == userId);

            if (user == null)
            {
                return Unauthorized(); // Trả về Unauthorized nếu người dùng không tồn tại
            }

            ViewBag.UserRole = user.RoleId; // Lưu vai trò của người dùng vào ViewBag
            return View(user); // Trả về view với thông tin người dùng
        }
    }
}