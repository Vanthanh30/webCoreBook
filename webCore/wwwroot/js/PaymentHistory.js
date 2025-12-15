const filters = document.querySelectorAll(".status-filter a");
const orderCards = document.querySelectorAll(".order-card");
const emptyState = document.getElementById("filterEmptyState");
const emptyTitle = document.getElementById("emptyTitle");
filters.forEach(btn => {
    btn.addEventListener("click", function (e) {
        e.preventDefault();

        filters.forEach(f => f.classList.remove("active"));
        this.classList.add("active");

        const status = this.getAttribute("data-status");

        orderCards.forEach(card => {
            const cardStatus = card.getAttribute("data-status");
            if (status === "Tất cả" || cardStatus === status) {
                card.style.display = "block";
            } else {
                card.style.display = "none";
            }
        });
    });
});

let productRatingVal = 0;
let serviceRatingVal = 0;
const overlay = document.getElementById("reviewOverlay");
const dialog = document.getElementById("reviewDialog");

// Mở dialog và điền dữ liệu
$(document).on("click", ".btnReview", function () {
    // Lấy dữ liệu từ nút bấm
    const img = $(this).data("product-image");
    const title = $(this).data("product-title");
    const price = $(this).data("product-price");
    const qty = $(this).data("quantity");
    const total = $(this).data("total");

    // Điền vào dialog HTML
    $("#dlgImg").attr("src", img);
    $("#dlgTitle").text(title);
    $("#dlgPrice").text(new Intl.NumberFormat('vi-VN').format(price) + "đ");
    $("#dlgQty").text("X" + qty);
    $("#dlgTotal").text(new Intl.NumberFormat('vi-VN').format(total) + "đ");

    // Hiển thị
    overlay.style.display = "block";
    dialog.style.display = "block";
});

// Hàm đóng dialog & Reset & Quay về tab "Đã giao"
function closeAndResetDialog() {
    overlay.style.display = "none";
    dialog.style.display = "none";

    // Reset form
    productRatingVal = 0;
    serviceRatingVal = 0;
    $("#reviewComment").val('');
    $(".star-rating .star").removeClass("selected");
    $(".media-box").removeClass("active");

    // === LOGIC QUAN TRỌNG: Quay về tab "Đã giao" ===
    const targetStatus = 'Đã giao';
    const deliveredBtn = document.querySelector(
        `.status-filter a[data-status='${targetStatus}']`
    );
    if (deliveredBtn) {
        deliveredBtn.click();
    }

}

$("#btnCloseX").on("click", function (e) {
    e.preventDefault(); // Ngăn chặn hành vi mặc định
    closeDialogLogic();
});

// Sự kiện click vào vùng đen bên ngoài (Overlay)
$("#reviewOverlay").on("click", function (e) {
    closeDialogLogic();
});

// --- 3. Xử lý Chọn Sao ---
$(".star-rating .star").on("click", function () {
    const value = $(this).data("value");
    const parentId = $(this).parent().attr("id");

    if (parentId === "productRating") productRatingVal = value;
    else serviceRatingVal = value;

    $(this).siblings().removeClass("selected");
    $(this).addClass("selected").prevAll().addClass("selected");
});

// --- 4. Xử lý Upload Media ---
$(".media-box").on("click", function () {
    $(".media-box").removeClass("active");
    $(this).addClass("active");

    const type = $(this).data("type");
    const acceptTypes = type === "image" ? "image/*" : "video/*";
    $("#reviewMedia").attr("accept", acceptTypes);
    $("#reviewMedia").click();
});

// --- 5. Nút Gửi ---
$("#btnSubmitReview").on("click", function () {
    alert("Đã gửi đánh giá thành công!");
    closeAndResetDialog();
});

// --- BẮT SỰ KIỆN KHI CLICK NÚT X ---
$("#btnCloseX").on("click", function (e) {
    e.preventDefault(); // Ngăn chặn hành vi mặc định (tránh lỗi nhảy trang)
    closeDialogLogic(); // Gọi hàm đóng dialog
});

// --- BẮT SỰ KIỆN KHI CLICK RA NGOÀI (VÙNG ĐEN) ---
$("#reviewOverlay").on("click", function (e) {
    closeDialogLogic(); // Gọi hàm đóng dialog
});

function closeDialogLogic() {
    // 1. Ẩn giao diện (Fade Out)
    $("#reviewOverlay").fadeOut(200);
    $("#reviewDialog").fadeOut(200);

    // 2. Reset form (để lần sau mở lên nó trống trơn, không lưu cái cũ)
    setTimeout(function () {
        $("#reviewComment").val('');
        $(".star-rating .star").removeClass("selected");
        $(".media-box").removeClass("active");
    }, 200);

    // 3. Logic quan trọng: Tự động bấm vào tab "Đã giao"
    const deliveredBtn = document.querySelector(
        `.status-filter a[data-status='Đã giao']`
    );
    if (deliveredBtn) {
        deliveredBtn.click();
    }
}