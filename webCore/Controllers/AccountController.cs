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
using webCore.Helpers; // ← THÊM DÒNG NÀY

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

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            var adminId = HttpContext.Session.GetString("AdminId");

            ViewBag.AdminName = adminName;

            if (!string.IsNullOrEmpty(adminId))
            {
                var admin = await _accountService.GetAccountByIdAsync(adminId);
                ViewBag.AdminAvatar = admin?.Avatar ?? "";
            }

            try
            {
                var accounts = await _accountService.GetAccounts();
                var totalAccounts = accounts.Count();
                var totalPages = (int)Math.Ceiling(totalAccounts / (double)pageSize);

                var accountsOnPage = accounts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(accountsOnPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accounts from MongoDB.");
                return View("Error");
            }
        }

        public async Task<IActionResult> Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            var adminId = HttpContext.Session.GetString("AdminId");

            ViewBag.AdminName = adminName;

            if (!string.IsNullOrEmpty(adminId))
            {
                var admin = await _accountService.GetAccountByIdAsync(adminId);
                ViewBag.AdminAvatar = admin?.Avatar ?? "";
            }

            return View();
        }

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
                    ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                    return View(account);
                }

                account.Id = Guid.NewGuid().ToString();
                account.RoleId = account.RoleId;

                // MÃ HÓA MẬT KHẨU TRƯỚC KHI LƯU
                if (!string.IsNullOrEmpty(account.Password))
                {
                    account.Password = PasswordHasher.HashPassword(account.Password);
                }

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
                    await _accountService.SaveAccountAsync(account);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving account to MongoDB.");
                    ModelState.AddModelError("", "Could not save account to database. Please try again.");
                    return View(account);
                }

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
                var adminName = HttpContext.Session.GetString("AdminName");
                var adminId = HttpContext.Session.GetString("AdminId");

                ViewBag.AdminName = adminName;

                if (!string.IsNullOrEmpty(adminId))
                {
                    var admin = await _accountService.GetAccountByIdAsync(adminId);
                    ViewBag.AdminAvatar = admin?.Avatar ?? "";
                }

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
                    var existingAccount = await _accountService.GetAccountByIdAsync(id);
                    if (existingAccount == null)
                        return NotFound("Account not found.");

                    var duplicateEmailAccount = (await _accountService.GetAccounts())
                        .FirstOrDefault(a => a.Email == updatedAccount.Email && a.Id != id);
                    if (duplicateEmailAccount != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng. Vui lòng chọn email khác.");
                        return View(updatedAccount);
                    }

                    existingAccount.FullName = updatedAccount.FullName;
                    existingAccount.Email = updatedAccount.Email;
                    existingAccount.Phone = updatedAccount.Phone;
                    existingAccount.Status = updatedAccount.Status;
                    existingAccount.RoleId = updatedAccount.RoleId;

                    // MÃ HÓA MẬT KHẨU MỚI NẾU ĐƯỢC NHẬP
                    if (!string.IsNullOrEmpty(Password))
                    {
                        existingAccount.Password = PasswordHasher.HashPassword(Password);
                    }

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

                    existingAccount.UpdatedAt = DateTime.UtcNow;
                    await _accountService.UpdateAccountAsync(existingAccount);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating account.");
                    ModelState.AddModelError("", "Could not update account. Please try again.");
                }
            }

            return View(updatedAccount);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var adminName = HttpContext.Session.GetString("AdminName");
                var adminId = HttpContext.Session.GetString("AdminId");

                ViewBag.AdminName = adminName;

                if (!string.IsNullOrEmpty(adminId))
                {
                    var admin = await _accountService.GetAccountByIdAsync(adminId);
                    ViewBag.AdminAvatar = admin?.Avatar ?? "";
                }

                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound();

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account for deletion.");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound();

                await _accountService.DeleteAccountAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tài khoản.");
                return View("Error");
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.User.Identity.Name;

            var user = (await _accountService.GetAccounts())
                       .FirstOrDefault(u => u.Email == userId);

            if (user == null)
            {
                return Unauthorized();
            }

            ViewBag.UserRole = user.RoleId;
            return View(user);
        }
    }
}