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

$(document).on("click", ".btnReview", function () {
    const img = $(this).data("product-image");
    const title = $(this).data("product-title");
    const price = $(this).data("product-price");
    const qty = $(this).data("quantity");
    const total = $(this).data("total");

    $("#dlgImg").attr("src", img);
    $("#dlgTitle").text(title);
    $("#dlgPrice").text(new Intl.NumberFormat('vi-VN').format(price) + "đ");
    $("#dlgQty").text("X" + qty);
    $("#dlgTotal").text(new Intl.NumberFormat('vi-VN').format(total) + "đ");

    overlay.style.display = "block";
    dialog.style.display = "block";
});

function closeAndResetDialog() {
    overlay.style.display = "none";
    dialog.style.display = "none";

    productRatingVal = 0;
    serviceRatingVal = 0;
    $("#reviewComment").val('');
    $(".star-rating .star").removeClass("selected");
    $(".media-box").removeClass("active");

    const targetStatus = 'Đã giao';
    const deliveredBtn = document.querySelector(
        `.status-filter a[data-status='${targetStatus}']`
    );
    if (deliveredBtn) {
        deliveredBtn.click();
    }

}

$("#btnCloseX").on("click", function (e) {
    e.preventDefault(); 
    closeDialogLogic();
});

$("#reviewOverlay").on("click", function (e) {
    closeDialogLogic();
});

$(".star-rating .star").on("click", function () {
    const value = $(this).data("value");
    const parentId = $(this).parent().attr("id");

    if (parentId === "productRating") productRatingVal = value;
    else serviceRatingVal = value;

    $(this).siblings().removeClass("selected");
    $(this).addClass("selected").prevAll().addClass("selected");
});

$(".media-box").on("click", function () {
    $(".media-box").removeClass("active");
    $(this).addClass("active");

    const type = $(this).data("type");
    const acceptTypes = type === "image" ? "image/*" : "video/*";
    $("#reviewMedia").attr("accept", acceptTypes);
    $("#reviewMedia").click();
});

$("#btnSubmitReview").on("click", function () {
    alert("Đã gửi đánh giá thành công!");
    closeAndResetDialog();
});

$("#btnCloseX").on("click", function (e) {
    e.preventDefault(); 
    closeDialogLogic(); 
});

$("#reviewOverlay").on("click", function (e) {
    closeDialogLogic(); 
});

function closeDialogLogic() {
    $("#reviewOverlay").fadeOut(200);
    $("#reviewDialog").fadeOut(200);

    setTimeout(function () {
        $("#reviewComment").val('');
        $(".star-rating .star").removeClass("selected");
        $(".media-box").removeClass("active");
    }, 200);

    const deliveredBtn = document.querySelector(
        `.status-filter a[data-status='Đã giao']`
    );
    if (deliveredBtn) {
        deliveredBtn.click();
    }
}