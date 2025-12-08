// Global flag to prevent multiple initializations
var dropdownInitialized = false;

// Initialize dropdown functionality
function initializeUserDropdown() {
    console.log("=== Initializing user dropdown ===");

    // Get all DOM elements
    var userIcon = document.getElementById("userIcon");
    var userDropdownMenu = document.getElementById("userDropdownMenu");
    var profileLink = document.getElementById("profileLink");
    var sellerChannelContainer = document.getElementById("sellerChannelContainer");
    var sellerChannelLink = document.getElementById("sellerChannelLink");
    var orderLink = document.getElementById("orderLink");
    var logoutButton = document.getElementById("logoutButton");
    var loginLink = document.getElementById("loginLink");
    var registerLink = document.getElementById("registerLink");

    // Get login status from body attribute
    var isLoggedIn = document.body.getAttribute("data-logged-in") === "True";

    console.log("Elements found:", {
        userIcon: !!userIcon,
        userDropdownMenu: !!userDropdownMenu,
        isLoggedIn: isLoggedIn
    });

    if (!userIcon || !userDropdownMenu) {
        console.error("❌ Required elements not found!");
        return false;
    }

    // Remove any existing click handlers by cloning
    var newUserIcon = userIcon.cloneNode(true);
    userIcon.parentNode.replaceChild(newUserIcon, userIcon);
    userIcon = document.getElementById("userIcon");

    // Add click handler to toggle dropdown
    userIcon.addEventListener('click', function (e) {
        console.log("✅ User icon clicked!");
        e.preventDefault();
        e.stopPropagation();

        var dropdown = document.getElementById("userDropdownMenu");
        if (dropdown) {
            dropdown.classList.toggle("hidden");
            console.log("Dropdown now:", dropdown.classList.contains("hidden") ? "hidden" : "visible");
        }
    });

    // Close dropdown when clicking outside
    var clickOutsideHandler = function (e) {
        var icon = document.getElementById("userIcon");
        var dropdown = document.getElementById("userDropdownMenu");

        if (dropdown && icon &&
            !icon.contains(e.target) &&
            !dropdown.contains(e.target)) {
            dropdown.classList.add("hidden");
        }
    };

    // Remove old listener and add new one
    document.removeEventListener('click', clickOutsideHandler);
    document.addEventListener('click', clickOutsideHandler);

    // Show/hide menu items based on login status
    console.log("Setting menu visibility for isLoggedIn:", isLoggedIn);

    if (isLoggedIn) {
        // User is logged in - show profile, seller channel, orders, logout
        if (profileLink) profileLink.style.display = "block";
        if (sellerChannelContainer) sellerChannelContainer.style.display = "block";
        if (orderLink) orderLink.style.display = "block";
        if (logoutButton && logoutButton.parentElement) {
            logoutButton.parentElement.style.display = "block";
        }
        // Hide login and register
        if (loginLink) loginLink.style.display = "none";
        if (registerLink) registerLink.style.display = "none";
    } else {
        // User is not logged in - hide all user options
        if (profileLink) profileLink.style.display = "none";
        if (sellerChannelContainer) sellerChannelContainer.style.display = "none";
        if (orderLink) orderLink.style.display = "none";
        if (logoutButton && logoutButton.parentElement) {
            logoutButton.parentElement.style.display = "none";
        }
        // Show login and register
        if (loginLink) loginLink.style.display = "block";
        if (registerLink) registerLink.style.display = "block";
    }

    // Handle seller channel link click - check if user has shop
    if (sellerChannelLink) {
        // Remove old handler
        var newSellerLink = sellerChannelLink.cloneNode(true);
        sellerChannelLink.parentNode.replaceChild(newSellerLink, sellerChannelLink);

        newSellerLink.addEventListener("click", async function (e) {
            e.preventDefault();
            try {
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
            } catch (error) {
                console.error('Error checking shop:', error);
            }
        });
    }

    console.log("✅ User dropdown initialized successfully");
    return true;
}

// Main initialization
function init() {
    console.log("=== Layout.js init() called ===");
    initializeUserDropdown();
    updateCartCount();
}

// Initialize on DOMContentLoaded
if (document.readyState === 'loading') {
    document.addEventListener("DOMContentLoaded", init);
} else {
    // DOM already loaded
    init();
}

// Also try on window load as backup
window.addEventListener('load', function () {
    console.log("=== Window load event ===");
    // Small delay to ensure everything is ready
    setTimeout(function () {
        initializeUserDropdown();
    }, 100);
});

// Handle logout
async function handleLogout(event) {
    event.preventDefault();

    // Clear local storage
    localStorage.removeItem("token");
    localStorage.removeItem("userName");
    localStorage.removeItem("userRoles");

    try {
        await fetch('/api/UserApi/logout', { method: 'POST' });
        window.location.href = "/Home/Index";
    } catch (error) {
        console.error('Logout error:', error);
        window.location.href = "/Home/Index";
    }
}

// Update cart item count
function updateCartCount() {
    const cartCountElement = document.getElementById('cart-item-1');

    if (cartCountElement) {
        fetch('/api/cart/count')
            .then(response => response.json())
            .then(data => {
                cartCountElement.textContent = data.count || 0;
            })
            .catch(error => {
                console.error('Error fetching cart count:', error);
                cartCountElement.textContent = 0;
            });
    }
}