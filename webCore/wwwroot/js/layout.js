document.addEventListener("DOMContentLoaded", function () {
    var userIcon = document.getElementById("userIcon");
    var userDropdownMenu = document.getElementById("userDropdownMenu");
    var profileLink = document.getElementById("profileLink");
    var sellerChannelContainer = document.getElementById("sellerChannelContainer");
    var sellerChannelLink = document.getElementById("sellerChannelLink");
    var orderLink = document.getElementById("orderLink");
    var logoutForm = document.getElementById("logoutForm");
    var loginLink = document.getElementById("loginLink");
    var registerLink = document.getElementById("registerLink");

    // Nhận trạng thái đăng nhập từ layout (được render vào attribute)
    var isLoggedIn = document.body.getAttribute("data-logged-in") === "True";

    // Toggle dropdown user
    userIcon.addEventListener('click', function (e) {
        e.preventDefault();
        userDropdownMenu.classList.toggle("hidden");
    });

    // Click ngoài để đóng menu
    document.addEventListener('click', function (e) {
        if (!userIcon.contains(e.target) && !userDropdownMenu.contains(e.target)) {
            userDropdownMenu.classList.add("hidden");
        }
    });

    // Hiển thị menu đúng theo đăng nhập
    if (isLoggedIn) {
        profileLink.classList.remove("hidden");
        sellerChannelContainer.classList.remove("hidden");
        orderLink.classList.remove("hidden");
        loginLink.classList.add("hidden");
        registerLink.classList.add("hidden");
    } else {
        profileLink.classList.add("hidden");
        sellerChannelContainer.classList.add("hidden");
        orderLink.classList.add("hidden");
        logoutForm.classList.add("hidden");
        loginLink.classList.remove("hidden");
        registerLink.classList.remove("hidden");
    }

    // 👉 EVENT: click kênh người bán → kiểm tra shop
    if (sellerChannelLink) {
        sellerChannelLink.addEventListener("click", async function (e) {
            e.preventDefault();

            const res = await fetch("/api/seller/check-shop");
            const data = await res.json();

            if (!data.success) {
                window.location.href = "/User/Sign_in";
                return;
            }

            if (!data.hasShop) {
                window.location.href = "/Shop_register/Index";
            } else {
                window.location.href = `/SellerDashboard/Dashboard?shopId=${data.shopId}`;
            }
        });
    }
});


// LOGOUT
async function handleLogout(event) {
    event.preventDefault();

    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    localStorage.removeItem("userRoles");

    await fetch('/api/UserApi/logout', { method: 'POST' });

    window.location.href = "/Home/Index";
}
