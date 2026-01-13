using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class VoucherClientController : Controller
    {
        private readonly VoucherClientService _voucherService;

        public VoucherClientController(VoucherClientService voucherService)
        {
            _voucherService = voucherService;
        }

        public async Task<IActionResult> VoucherClient()
        {

            var userId = HttpContext.Session.GetString("UserToken");
            var voucherDiscount = HttpContext.Session.GetString("SelectedVoucher");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Sign_in", "User");
            }

            var vouchers = await _voucherService.GetActiveVouchersAsync();

            return View(vouchers);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(string discount, string voucherId)
        {
            ObjectId parsedVoucherId;
            if (!ObjectId.TryParse(voucherId, out parsedVoucherId))
            {
                return Json(new { success = false, message = "Voucher ID không hợp lệ." });
            }

            var voucher = await _voucherService.GetVoucherByIdAsync(voucherId); 

            if (voucher == null)
            {
                return Json(new { success = false, message = "Voucher không tồn tại." });
            }

            if (voucher.UsageCount >= voucher.UsageLimit)
            {
                return Json(new { success = false, message = "Voucher đã đạt giới hạn sử dụng." });
            }
            HttpContext.Session.SetString("SelectedVoucher", discount);
            HttpContext.Session.SetString("SelectedVoucherId", voucherId);  

            return Json(new { success = true });
        }


    }
}
