document.addEventListener("DOMContentLoaded", function () {
    var userIcon = document.getElementById("userIcon");
    var userDropdownMenu = document.getElementById("userDropdownMenu");
    var profileLink = document.getElementById("profileLink");
    var logoutForm = document.getElementById("logoutForm");
    var loginLink = document.getElementById("loginLink");
    var registerLink = document.getElementById("registerLink");

    var isLoggedIn = '@ViewBag.IsLoggedIn' === 'True';

    userIcon.addEventListener('click', function (e) {
        e.preventDefault();
        userDropdownMenu.classList.toggle("hidden");
    });

    if (isLoggedIn) {
        profileLink.classList.remove("hidden");
        logoutForm.classList.remove("hidden");
        loginLink.classList.add("hidden");
        registerLink.classList.add("hidden");
    } else {
        profileLink.classList.add("hidden");
        logoutForm.classList.add("hidden");
        loginLink.classList.remove("hidden");
        registerLink.classList.remove("hidden");
    }
});