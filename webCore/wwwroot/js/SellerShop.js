// Extract dominant color from shop avatar
function getAvatarDominantColor() {
    const shopAvatar = document.querySelector('.shop-avatar img');
    if (!shopAvatar) return '#66C5FF'; // Fallback color

    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');

    canvas.width = shopAvatar.width;
    canvas.height = shopAvatar.height;

    // Wait for image to load
    if (!shopAvatar.complete) {
        shopAvatar.addEventListener('load', function () {
            extractAndApplyColor(shopAvatar, canvas, ctx);
        });
    } else {
        extractAndApplyColor(shopAvatar, canvas, ctx);
    }
}

function extractAndApplyColor(img, canvas, ctx) {
    try {
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        const data = imageData.data;

        let r = 0, g = 0, b = 0;
        let count = 0;

        // Sample every 10th pixel for performance
        for (let i = 0; i < data.length; i += 40) {
            r += data[i];
            g += data[i + 1];
            b += data[i + 2];
            count++;
        }

        r = Math.floor(r / count);
        g = Math.floor(g / count);
        b = Math.floor(b / count);

        // Make color more vibrant
        const dominantColor = `rgb(${r}, ${g}, ${b})`;
        const vibrantColor = makeColorVibrant(r, g, b);

        applyThemeColor(vibrantColor);
    } catch (e) {
        console.error('Error extracting color:', e);
        // Use default color if CORS or other error
        applyThemeColor('#66C5FF');
    }
}

function makeColorVibrant(r, g, b) {
    // Increase saturation and adjust brightness
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    const delta = max - min;

    if (delta === 0) return '#66C5FF'; // Gray color, use default

    // Boost saturation
    const boost = 1.3;
    r = Math.min(255, Math.floor(r * boost));
    g = Math.min(255, Math.floor(g * boost));
    b = Math.min(255, Math.floor(b * boost));

    return `rgb(${r}, ${g}, ${b})`;
}

function applyThemeColor(color) {
    const shopHeaderContainer = document.querySelector('.shop-header .container');
    const followBtn = document.querySelector('.btn-follow');

    if (shopHeaderContainer) {
        shopHeaderContainer.style.background = color;
    }

    if (followBtn && !followBtn.classList.contains('following')) {
        followBtn.style.color = color;
    }

    // Store color for later use
    document.documentElement.style.setProperty('--shop-theme-color', color);
}

// Tab switching functionality
document.addEventListener('DOMContentLoaded', function () {
    // Apply avatar color theme
    getAvatarDominantColor();

    const navTabs = document.querySelectorAll('.nav-tab');
    const tabContents = document.querySelectorAll('.tab-content');

    navTabs.forEach(tab => {
        tab.addEventListener('click', function () {
            const targetTab = this.getAttribute('data-tab');

            // Remove active class from all tabs
            navTabs.forEach(t => t.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));

            // Add active class to clicked tab
            this.classList.add('active');
            document.getElementById(targetTab + '-tab').classList.add('active');
        });
    });

    // Filter buttons functionality
    const filterBtns = document.querySelectorAll('.filter-btn');
    filterBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            filterBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            // Add your filter logic here
            console.log('Filter applied:', this.textContent);
        });
    });

    // Product card click
    const productCards = document.querySelectorAll('.product-card');
    productCards.forEach(card => {
        card.addEventListener('click', function () {
            // Navigate to product detail page
            console.log('Product clicked');
            // window.location.href = '/Product/Detail?id=' + productId;
        });
    });

    // Category item click
    const categoryItems = document.querySelectorAll('.category-item');
    categoryItems.forEach(item => {
        item.addEventListener('click', function () {
            const categoryName = this.querySelector('.category-name').textContent;
            console.log('Category clicked:', categoryName);
            // Navigate to category products page
            // window.location.href = '/Shop/Category?name=' + categoryName;
        });
    });

    // Follow button
    const followBtn = document.querySelector('.btn-follow');
    if (followBtn) {
        followBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            const isFollowing = this.classList.contains('following');

            if (!isFollowing) {
                this.classList.add('following');
                this.innerHTML = '<i class="fas fa-check"></i> Đang theo dõi';
                this.style.background = '#28a745';
                this.style.color = 'white';
                this.style.borderColor = '#28a745';
            } else {
                this.classList.remove('following');
                this.innerHTML = '<i class="fas fa-plus"></i> Theo dõi';
                this.style.background = 'white';
                const themeColor = getComputedStyle(document.documentElement).getPropertyValue('--shop-theme-color') || '#66C5FF';
                this.style.color = themeColor;
                this.style.borderColor = 'white';
            }

            // Add AJAX call here to update follow status
            console.log('Follow status changed');
        });
    }

    // Chat button
    const chatBtn = document.querySelector('.btn-chat');
    if (chatBtn) {
        chatBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            // Redirect to chat page
            window.location.href = '/Chat_buyer/Index';
        });
    }

    // Video section click
    const videoSection = document.querySelector('.video-section a');
    if (videoSection) {
        videoSection.addEventListener('click', function (e) {
            e.preventDefault();
            // Open video modal or redirect to video page
            console.log('Video section clicked');
        });
    }
});