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
            // Number of items per page
            const int pageSize = 5;

            // Fetch admin name from session
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;

            // Fetch total number of vouchers
            var totalVouchers = await _voucherService.GetVouchers();

            // Cập nhật trạng thái cho tất cả voucher dựa trên ngày hiện tại
            var currentDate = DateTime.Now.Date;
            foreach (var voucher in totalVouchers)
            {
                // Kiểm tra nếu voucher đã hết hạn
                if (voucher.EndDate.Date < currentDate)
                {
                    voucher.IsActive = false;
                }
                // Kiểm tra nếu voucher đang trong thời gian hoạt động
                else if (voucher.StartDate.Date <= currentDate && voucher.EndDate.Date >= currentDate)
                {
                    voucher.IsActive = true;
                }
                // Nếu chưa đến ngày bắt đầu, giữ nguyên IsActive
            }

            var totalPages = (int)Math.Ceiling(totalVouchers.Count / (double)pageSize);

            // Fetch vouchers for the current page
            var vouchers = totalVouchers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Pass the vouchers and pagination info to the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(vouchers);
        }

        // GET: VoucherController/Create - Giao diện tạo voucher cho Admin
        public IActionResult Create()
        {
            var adminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AdminName = adminName;
            return View();
        }

        // POST: VoucherController/Create - Xử lý tạo voucher từ Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra ngày bắt đầu và ngày kết thúc hợp lệ
                if (voucher.StartDate <= voucher.EndDate)
                {
                    voucher.UsageCount = 0; // Đặt số lần sử dụng ban đầu là 0

                    // Tự động xác định trạng thái dựa trên ngày hiện tại
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

        // GET: VoucherController/Edit/{id}
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
                    // Kiểm tra ngày bắt đầu và ngày kết thúc hợp lệ
                    if (updatedVoucher.StartDate > updatedVoucher.EndDate)
                    {
                        ModelState.AddModelError("", "Ngày bắt đầu phải trước ngày kết thúc.");
                        return View(updatedVoucher);
                    }

                    // Gán lại ObjectId cho voucher
                    updatedVoucher.Id = new ObjectId(id);

                    // Tự động cập nhật trạng thái dựa trên ngày hiện tại
                    var currentDate = DateTime.Now.Date;

                    // Kiểm tra nếu voucher còn trong thời gian hoạt động
                    if (updatedVoucher.StartDate <= currentDate && updatedVoucher.EndDate >= currentDate)
                    {
                        // Nếu trong thời gian hoạt động, set IsActive = true
                        updatedVoucher.IsActive = true;
                    }
                    else if (updatedVoucher.EndDate < currentDate)
                    {
                        // Nếu đã quá ngày kết thúc, set IsActive = false
                        updatedVoucher.IsActive = false;
                    }
                    // Nếu chưa tới ngày bắt đầu, giữ nguyên giá trị IsActive từ form

                    // Gọi service để cập nhật voucher
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

        // GET: VoucherController/Delete/{id}
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