using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Linq;
using System.Threading.Tasks;
using webCore.Models;
using webCore.MongoHelper;
using webCore.Services;

namespace webCore.Controllers
{
    public class VoucherController : Controller
    {
        private readonly MongoDBService _mongoDBService;
        private readonly VoucherService _voucherService;

        public VoucherController(VoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 5;

            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;

            var totalVouchers = await _voucherService.GetVouchers();

            var currentDate = DateTime.Now.Date;
            foreach (var voucher in totalVouchers)
            {
                if (voucher.EndDate.Date < currentDate)
                {
                    voucher.IsActive = false;
                }
                else if (voucher.StartDate.Date <= currentDate && voucher.EndDate.Date >= currentDate)
                {
                    voucher.IsActive = true;
                }
            }

            var totalPages = (int)Math.Ceiling(totalVouchers.Count / (double)pageSize);

            var vouchers = totalVouchers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(vouchers);
        }

        public IActionResult Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                if (voucher.StartDate <= voucher.EndDate)
                {
                    voucher.UsageCount = 0; 

                    var currentDate = DateTime.Now.Date;
                    voucher.IsActive = voucher.StartDate <= currentDate && voucher.EndDate >= currentDate;

                    await _voucherService.CreateVoucherAsync(voucher);
                    TempData["Message"] = "Voucher đã được tạo thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc.");
                }
            }

            return View(voucher);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Voucher updatedVoucher)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Voucher ID không hợp lệ.");

            if (ModelState.IsValid)
            {
                try
                {
                    if (updatedVoucher.StartDate > updatedVoucher.EndDate)
                    {
                        ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc.");
                        return View(updatedVoucher);
                    }

                    updatedVoucher.Id = new ObjectId(id);

                    var currentDate = DateTime.Now.Date;

                    if (updatedVoucher.StartDate <= currentDate && updatedVoucher.EndDate >= currentDate)
                    {
                        updatedVoucher.IsActive = true;
                    }
                    else if (updatedVoucher.EndDate < currentDate)
                    {
                        updatedVoucher.IsActive = false;
                    }

                    bool result = await _voucherService.UpdateVoucherAsync(id, updatedVoucher);

                    if (!result)
                    {
                        ModelState.AddModelError("", "Không thể cập nhật voucher. Vui lòng thử lại.");
                        return View(updatedVoucher);
                    }

                    TempData["Message"] = "Voucher đã được cập nhật thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                }
            }

            return View(updatedVoucher);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _voucherService.DeleteVoucherAsync(id);

            if (result)
            {
                TempData["Message"] = "Voucher đã được xóa thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Không thể xóa voucher.");
            return RedirectToAction("Index");
        }
    }
}