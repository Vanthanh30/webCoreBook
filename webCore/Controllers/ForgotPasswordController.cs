using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController : Controller
    {
        private readonly ForgotPasswordService _forgotPasswordService;
        private readonly UserService _userService;

        public ForgotPasswordController(ForgotPasswordService forgotPasswordService, UserService userService)
        {
            _forgotPasswordService = forgotPasswordService;
            _userService = userService;
        }

        [HttpGet("send-otp-view")]
        public IActionResult SendOtpView()
        {
            return View("SendOtp");
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromForm] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Email không được để trống.";
                return View("SendOtp");
            }

            try
            {
                var user = await _userService.GetAccountByEmailAsync(email);
                if (user == null)
                {
                    ViewBag.Error = "Email không tồn tại trong hệ thống.";
                    return View("SendOtp");
                }

                await _forgotPasswordService.GenerateAndSendOTP(email);
                ViewBag.Message = "OTP đã được gửi tới email của bạn.";
                return RedirectToAction("VerifyOtpView", new { email = email });
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
                return View("SendOtp");
            }
        }

        [HttpGet("verify-otp-view")]
        public IActionResult VerifyOtpView([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Email không hợp lệ.";
                return View("SendOtp");
            }

            ViewBag.Email = email; 
            return View("VerifyOtp");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromForm] string email, [FromForm] string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                ViewBag.Error = "Email và OTP không được để trống.";
                ViewBag.Email = email;
                return View("VerifyOtp");
            }

            try
            {
                var isVerified = await _forgotPasswordService.VerifyOTP(email, otp);

                if (!isVerified)
                {
                    ViewBag.Error = "OTP không hợp lệ hoặc đã hết hạn.";
                    ViewBag.Email = email;
                    return View("VerifyOtp");
                }

                TempData["Email"] = email; 
                return RedirectToAction("ResetPasswordView");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Có lỗi xảy ra: {ex.Message}";
                ViewBag.Email = email;
                return View("VerifyOtp");
            }
        }

        [HttpGet("reset-password-view")]
        public IActionResult ResetPasswordView()
        {
            if (TempData["Email"] != null)
            {
                ViewBag.Email = TempData["Email"].ToString();
                return View("ResetPassword");
            }

            ViewBag.Error = "Không tìm thấy email hợp lệ.";
            return RedirectToAction("SendOtpView");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromForm] string email, [FromForm] string newPassword, [FromForm] string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                ViewBag.Error = "Email, mật khẩu và xác nhận mật khẩu không được để trống và mật khẩu phải khớp.";
                ViewBag.Email = email;
                return View("ResetPassword");
            }

            try
            {
                var user = await _userService.GetAccountByEmailAsync(email);
                if (user == null)
                {
                    ViewBag.Error = "Không tìm thấy tài khoản với email này.";
                    ViewBag.Email = email;
                    return View("ResetPassword");
                }

                var isUpdated = await _userService.UpdatePasswordAsync(email, newPassword);
                if (!isUpdated)
                {
                    ViewBag.Error = "Không thể cập nhật mật khẩu. Vui lòng thử lại.";
                    ViewBag.Email = email;
                    return View("ResetPassword");
                }

                ViewBag.Message = "Mật khẩu của bạn đã được đặt lại thành công!";
                return RedirectToAction("Sign_in","User");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
                ViewBag.Email = email;
                return View("ResetPassword");
            }
        }
    }
}
