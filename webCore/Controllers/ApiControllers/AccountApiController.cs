using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webCore.Helpers;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers.ApiControllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountApiController : Controller
    {
        private readonly AccountService _accountService;
        private readonly CloudinaryService _cloudinaryService;
        private readonly RoleService _roleService;

        public AccountApiController(AccountService accountService, CloudinaryService cloudinaryService, RoleService roleService)
        {
            _accountService = accountService;
            _cloudinaryService = cloudinaryService;
            _roleService = roleService;
        }

        [HttpGet("index")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var accounts = await _accountService.GetAccounts();

            var selected = accounts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new List<object>();

            foreach (var acc in selected)
            {
                // Lấy tên role từ RoleId
                var roleNames = new List<string>();

                foreach (var roleId in acc.RoleId)
                {
                    var role = await _roleService.GetRoleByIdAsync(roleId);
                    if (role != null)
                        roleNames.Add(role.Name);
                }

                response.Add(new
                {
                    id = acc.Id,
                    name = acc.Name,
                    email = acc.Email,
                    profileImage = acc.ProfileImage,
                    status = acc.Status,
                    roleName = roleNames
                });
            }

            return Ok(new
            {
                success = true,
                total = accounts.Count(),
                data = response
            });
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] User user, IFormFile ProfileImage)
        {
            var duplicate = (await _accountService.GetAccounts())
                .FirstOrDefault(a => a.Email == user.Email);

            if (duplicate != null)
                return BadRequest(new { success = false, message = "Email đã tồn tại" });

            if (!string.IsNullOrEmpty(user.Password))
                user.Password = PasswordHasher.HashPassword(user.Password);

            if (ProfileImage != null)
                user.ProfileImage = await _cloudinaryService.UploadImageAsync(ProfileImage);

            await _accountService.SaveAccountAsync(user);

            return Ok(new { success = true, message = "Tạo account thành công" });
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var acc = await _accountService.GetAccountByIdAsync(id);
            return acc == null ? NotFound() : Ok(acc);
        }
        public class UpdateAccountDto
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public int Status { get; set; }
            public List<string> RoleId { get; set; }
            public string Password { get; set; } 
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Edit(
            string id,
            [FromForm] UpdateAccountDto updated,
            IFormFile ProfileImage)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { success = false, message = "ID không hợp lệ." });

            var existingAccount = await _accountService.GetAccountByIdAsync(id);
            if (existingAccount == null)
                return NotFound(new { success = false, message = "Không tìm thấy tài khoản." });

            // KIỂM TRA EMAIL ĐÃ TỒN TẠI TRONG USER KHÁC
            var duplicate = (await _accountService.GetAccounts())
                .FirstOrDefault(a => a.Email == updated.Email && a.Id != id);

            if (duplicate != null)
                return BadRequest(new { success = false, message = "Email đã tồn tại." });

            existingAccount.Name = updated.Name;
            existingAccount.Email = updated.Email;
            existingAccount.Phone = updated.Phone;
            existingAccount.Status = updated.Status;

            // ROLE
            if (updated.RoleId != null && updated.RoleId.Any())
                existingAccount.RoleId = updated.RoleId;

            if (!string.IsNullOrEmpty(updated.Password))
            {
                existingAccount.Password = PasswordHasher
                    .HashPassword(updated.Password);
            }

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                try
                {
                    var url = await _cloudinaryService.UploadImageAsync(ProfileImage);
                    existingAccount.ProfileImage = url;
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = "Lỗi upload ảnh: " + ex.Message });
                }
            }

            existingAccount.UpdatedAt = DateTime.UtcNow;

            await _accountService.UpdateAccountAsync(existingAccount);

            return Ok(new { success = true, message = "Cập nhật tài khoản thành công!" });
        }



        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            return account == null ? NotFound() : Ok(account);
        }

        [HttpDelete("deleteconfirmed/{id}")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var acc = await _accountService.GetAccountByIdAsync(id);
            if (acc == null)
                return NotFound();

            await _accountService.DeleteAccountAsync(id);

            return Ok(new { success = true, message = "Xóa thành công" });
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            return Ok(new { success = true });
        }
    }
}
