// wwwroot/js/shop-register.js

// Show registration form
function showRegistrationForm() {
    document.getElementById('welcomePage').style.display = 'none';
    document.getElementById('registrationForm').classList.add('active');
}

// Avatar Upload Preview
const avatarInput = document.getElementById('avatarInput');
const avatarPreview = document.getElementById('avatarPreview');
const avatarPlaceholder = document.getElementById('avatarPlaceholder');

if (avatarInput) {
    avatarInput.addEventListener('change', function (e) {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                avatarPreview.src = e.target.result;
                avatarPreview.style.display = 'block';
                avatarPlaceholder.style.display = 'none';
            };
            reader.readAsDataURL(file);
        }
    });
}

// Character counter for shop name
const shopNameInput = document.getElementById('shopName');
const charCountSpan = document.getElementById('charCount');

if (shopNameInput && charCountSpan) {
    shopNameInput.addEventListener('input', function () {
        charCountSpan.textContent = this.value.length;
    });
}

// Character counter for description
const descriptionInput = document.getElementById('description');
const descCharCountSpan = document.getElementById('descCharCount');

if (descriptionInput && descCharCountSpan) {
    descriptionInput.addEventListener('input', function () {
        descCharCountSpan.textContent = this.value.length;
    });
}

// Save form to localStorage
/*function saveForm() {
    const formData = {
        shopName: document.getElementById('shopName').value,
        description: document.getElementById('description').value,
        businessType: document.getElementById('businessType').value,
        email: document.getElementById('email').value,
        phone: document.getElementById('phone').value,
        website: document.getElementById('website').value,
        address: document.getElementById('address').value,
        province: document.getElementById('province').value,
        district: document.getElementById('district').value,
        ward: document.getElementById('ward').value
    };

    localStorage.setItem('shopRegistrationDraft', JSON.stringify(formData));

    // Show success notification
    alert('✅ Đã lưu nháp thành công!');
}*/

// Load draft from localStorage
/*function loadDraft() {
    const draft = localStorage.getItem('shopRegistrationDraft');
    if (draft) {
        const formData = JSON.parse(draft);

        document.getElementById('shopName').value = formData.shopName || '';
        document.getElementById('description').value = formData.description || '';
        document.getElementById('businessType').value = formData.businessType || '';
        document.getElementById('email').value = formData.email || '';
        document.getElementById('phone').value = formData.phone || '';
        document.getElementById('website').value = formData.website || '';
        document.getElementById('address').value = formData.address || '';
        document.getElementById('province').value = formData.province || '';
        document.getElementById('district').value = formData.district || '';
        document.getElementById('ward').value = formData.ward || '';

        // Update character counts
        if (charCountSpan) charCountSpan.textContent = formData.shopName?.length || 0;
        if (descCharCountSpan) descCharCountSpan.textContent = formData.description?.length || 0;
    }
}*/

// Load draft when page loads
/*document.addEventListener('DOMContentLoaded', function () {
    // Ask user if they want to load draft
    const draft = localStorage.getItem('shopRegistrationDraft');
    if (draft) {
        const loadDraftConfirm = confirm('Bạn có muốn tải bản nháp đã lưu trước đó không?');
        if (loadDraftConfirm) {
            showRegistrationForm();
            loadDraft();
        }
    }
});
*/
// Form submission
/*const shopForm = document.getElementById('shopForm');
if (shopForm) {
    shopForm.addEventListener('submit', function (e) {
        e.preventDefault();

        // Validate form
        const requiredFields = shopForm.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                isValid = false;
                field.style.borderColor = '#ff4444';
            } else {
                field.style.borderColor = '#e5e5e5';
            }
        });

        if (!isValid) {
            alert('⚠️ Vui lòng điền đầy đủ thông tin bắt buộc!');
            return;
        }

        // Validate email
        const email = document.getElementById('email').value;
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email)) {
            alert('Email không hợp lệ!');
            return;
        }

        // Validate phone
        const phone = document.getElementById('phone').value;
        const phoneRegex = /^[0-9]{10,11}$/;
        if (!phoneRegex.test(phone)) {
            alert('Số điện thoại không hợp lệ!');
            return;
        }

        // Show loading
        const submitBtn = this.querySelector('button[type="submit"]');
        const originalText = submitBtn.innerHTML;
        submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xử lý...';
        submitBtn.disabled = true;

        // Submit form (uncomment khi có backend)
        // this.submit();

        // Simulate submission
        setTimeout(() => {
            alert('Đăng ký shop thành công! Chúng tôi sẽ xét duyệt trong vòng 24h.');
            // Clear draft
            localStorage.removeItem('shopRegistrationDraft');
            // Redirect
            // window.location.href = '/Home/Index';

            submitBtn.innerHTML = originalText;
            submitBtn.disabled = false;
        }, 2000);
    });
}*/

// Province/District/Ward dropdown (Vietnamese location data)
const locationData = {
    'hanoi': {
        name: 'Hà Nội',
        districts: {
            'badinh': {
                name: 'Ba Đình',
                wards: ['Phường Phúc Xá', 'Phường Trúc Bạch', 'Phường Vĩnh Phúc', 'Phường Cống Vị', 'Phường Liễu Giai', 'Phường Nguyễn Trung Trực', 'Phường Quán Thánh', 'Phường Ngọc Hà', 'Phường Điện Biên', 'Phường Đội Cấn', 'Phường Ngọc Khánh', 'Phường Kim Mã', 'Phường Giảng Võ', 'Phường Thành Công']
            },
            'hoankeim': {
                name: 'Hoàn Kiếm',
                wards: ['Phường Hàng Bạc', 'Phường Hàng Bài', 'Phường Hàng Bồ', 'Phường Hàng Đào', 'Phường Hàng Gai', 'Phường Hàng Trống', 'Phường Cửa Đông', 'Phường Lý Thái Tổ', 'Phường Phan Chu Trinh', 'Phường Tràng Tiền', 'Phường Trần Hưng Đạo', 'Phường Cửa Nam', 'Phường Hàng Bông', 'Phường Đồng Xuân', 'Phường Chương Dương', 'Phường Phúc Tân', 'Phường Hàng Mã', 'Phường Hàng Buồm']
            },
            'dongda': {
                name: 'Đống Đa',
                wards: ['Phường Cát Linh', 'Phường Văn Miếu', 'Phường Quốc Tử Giám', 'Phường Láng Thượng', 'Phường Ô Chợ Dừa', 'Phường Văn Chương', 'Phường Hàng Bột', 'Phường Láng Hạ', 'Phường Khâm Thiên', 'Phường Thổ Quan', 'Phường Nam Đồng', 'Phường Trung Phụng', 'Phường Quang Trung', 'Phường Trung Liệt', 'Phường Phương Liên', 'Phường Thịnh Quang', 'Phường Trung Tự', 'Phường Kim Liên', 'Phường Phương Mai', 'Phường Ngã Tư Sở', 'Phường Khương Thượng']
            },
            'tayho': {
                name: 'Tây Hồ',
                wards: ['Phường Phú Thượng', 'Phường Nhật Tân', 'Phường Tứ Liên', 'Phường Quảng An', 'Phường Xuân La', 'Phường Yên Phụ', 'Phường Bưởi', 'Phường Thụy Khuê']
            },
            'cauvinh': {
                name: 'Cầu Giấy',
                wards: ['Phường Nghĩa Đô', 'Phường Nghĩa Tân', 'Phường Mai Dịch', 'Phường Dịch Vọng', 'Phường Dịch Vọng Hậu', 'Phường Quan Hoa', 'Phường Yên Hòa', 'Phường Trung Hòa']
            }
        }
    },
    'hcm': {
        name: 'TP. Hồ Chí Minh',
        districts: {
            'quan1': {
                name: 'Quận 1',
                wards: ['Phường Bến Nghé', 'Phường Bến Thành', 'Phường Cầu Kho', 'Phường Cầu Ông Lãnh', 'Phường Cô Giang', 'Phường Đa Kao', 'Phường Nguyễn Cư Trinh', 'Phường Nguyễn Thái Bình', 'Phường Phạm Ngũ Lão', 'Phường Tân Định']
            },
            'quan3': {
                name: 'Quận 3',
                wards: ['Phường 1', 'Phường 2', 'Phường 3', 'Phường 4', 'Phường 5', 'Phường 6', 'Phường 7', 'Phường 8', 'Phường 9', 'Phường 10', 'Phường 11', 'Phường 12', 'Phường 13', 'Phường 14']
            },
            'quan5': {
                name: 'Quận 5',
                wards: ['Phường 1', 'Phường 2', 'Phường 3', 'Phường 4', 'Phường 5', 'Phường 6', 'Phường 7', 'Phường 8', 'Phường 9', 'Phường 10', 'Phường 11', 'Phường 12', 'Phường 13', 'Phường 14', 'Phường 15']
            },
            'quan10': {
                name: 'Quận 10',
                wards: ['Phường 1', 'Phường 2', 'Phường 3', 'Phường 4', 'Phường 5', 'Phường 6', 'Phường 7', 'Phường 8', 'Phường 9', 'Phường 10', 'Phường 11', 'Phường 12', 'Phường 13', 'Phường 14', 'Phường 15']
            },
            'govap': {
                name: 'Gò Vấp',
                wards: ['Phường 1', 'Phường 3', 'Phường 4', 'Phường 5', 'Phường 6', 'Phường 7', 'Phường 8', 'Phường 9', 'Phường 10', 'Phường 11', 'Phường 12', 'Phường 13', 'Phường 14', 'Phường 15', 'Phường 16', 'Phường 17']
            },
            'binhtan': {
                name: 'Bình Tân',
                wards: ['Phường Bình Hưng Hòa', 'Phường Bình Hưng Hòa A', 'Phường Bình Hưng Hòa B', 'Phường Bình Trị Đông', 'Phường Bình Trị Đông A', 'Phường Bình Trị Đông B', 'Phường Tân Tạo', 'Phường Tân Tạo A', 'Phường An Lạc', 'Phường An Lạc A']
            }
        }
    },
    'danang': {
        name: 'Đà Nẵng',
        districts: {
            'haichau': {
                name: 'Hải Châu',
                wards: ['Phường Thanh Bình', 'Phường Thạch Thang', 'Phường Hải Châu 1', 'Phường Hải Châu 2', 'Phường Phước Ninh', 'Phường Hòa Thuận Tây', 'Phường Hòa Thuận Đông', 'Phường Nam Dương', 'Phường Bình Hiên', 'Phường Hòa Cường Bắc', 'Phường Hòa Cường Nam', 'Phường Bình Thuận', 'Phường Tân Chính']
            },
            'thankkhe': {
                name: 'Thanh Khê',
                wards: ['Phường Tam Thuận', 'Phường Thanh Khê Tây', 'Phường Thanh Khê Đông', 'Phường Xuân Hà', 'Phường Tân Chính', 'Phường Chính Gián', 'Phường Vĩnh Trung', 'Phường Thạc Gián', 'Phường An Khê', 'Phường Hòa Khê']
            },
            'sontra': {
                name: 'Sơn Trà',
                wards: ['Phường Thọ Quang', 'Phường Nại Hiên Đông', 'Phường Mân Thái', 'Phường An Hải Bắc', 'Phường Phước Mỹ', 'Phường An Hải Tây', 'Phường An Hải Đông']
            },
            'nguhanhson': {
                name: 'Ngũ Hành Sơn',
                wards: ['Phường Mỹ An', 'Phường Khuê Mỹ', 'Phường Hòa Quý', 'Phường Hòa Hải']
            }
        }
    },
    'haiphong': {
        name: 'Hải Phòng',
        districts: {
            'honggai': {
                name: 'Hồng Bàng',
                wards: ['Phường Quán Toan', 'Phường Hùng Vương', 'Phường Sở Dầu', 'Phường Thượng Lý', 'Phường Hạ Lý', 'Phường Minh Khai', 'Phường Trại Chuối', 'Phường Hoàng Văn Thụ', 'Phường Phan Bội Châu']
            },
            'ngoquyen': {
                name: 'Ngô Quyền',
                wards: ['Phường Máy Chai', 'Phường Máy Tơ', 'Phường Vạn Mỹ', 'Phường Cầu Tre', 'Phường Lạc Viên', 'Phường Gia Viên', 'Phường Đông Khê', 'Phường Cầu Đất', 'Phường Lê Lợi', 'Phường Đằng Giang', 'Phường Lạch Tray', 'Phường Đổng Quốc Bình']
            },
            'lechan': {
                name: 'Lê Chân',
                wards: ['Phường Cát Dài', 'Phường An Biên', 'Phường Lam Sơn', 'Phường An Dương', 'Phường Trần Nguyên Hãn', 'Phường Hồ Nam', 'Phường Trại Cau', 'Phường Dư Hàng', 'Phường Hàng Kênh', 'Phường Đông Hải', 'Phường Niệm Nghĩa', 'Phường Nghĩa Xá', 'Phường Dư Hàng Kênh', 'Phường Kênh Dương', 'Phường Vĩnh Niệm']
            }
        }
    },
    'cantho': {
        name: 'Cần Thơ',
        districts: {
            'ninhkieu': {
                name: 'Ninh Kiều',
                wards: ['Phường Cái Khế', 'Phường An Hòa', 'Phường Thới Bình', 'Phường An Nghiệp', 'Phường An Cư', 'Phường Tân An', 'Phường An Phú', 'Phường Xuân Khánh', 'Phường Hưng Lợi', 'Phường An Khánh', 'Phường An Bình']
            },
            'cairang': {
                name: 'Cái Răng',
                wards: ['Phường Lê Bình', 'Phường Hưng Phú', 'Phường Hưng Thạnh', 'Phường Ba Láng', 'Phường Thường Thạnh', 'Phường Phú Thứ', 'Phường Tân Phú']
            },
            'binhthuy': {
                name: 'Bình Thủy',
                wards: ['Phường Bình Thủy', 'Phường Trà An', 'Phường Trà Nóc', 'Phường Thới An Đông', 'Phường An Thới', 'Phường Bùi Hữu Nghĩa', 'Phường Long Hòa', 'Phường Long Tuyền']
            }
        }
    }
};

// Province change handler
const provinceSelect = document.getElementById('province');
const districtSelect = document.getElementById('district');
const wardSelect = document.getElementById('ward');

if (provinceSelect) {
    provinceSelect.addEventListener('change', function () {
        const province = this.value;
        districtSelect.innerHTML = '<option value="">Chọn Quận/Huyện</option>';
        wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';

        if (province && locationData[province]) {
            const districts = locationData[province].districts;
            for (const [key, value] of Object.entries(districts)) {
                const option = document.createElement('option');
                option.value = key;
                option.textContent = value.name;
                districtSelect.appendChild(option);
            }
        }
    });
}

// District change handler
if (districtSelect) {
    districtSelect.addEventListener('change', function () {
        const province = provinceSelect.value;
        const district = this.value;
        wardSelect.innerHTML = '<option value="">Chọn Phường/Xã</option>';

        if (province && district && locationData[province]?.districts[district]) {
            const wards = locationData[province].districts[district].wards;
            wards.forEach(ward => {
                const option = document.createElement('option');
                option.value = ward;
                option.textContent = ward;
                wardSelect.appendChild(option);
            });
        }
    });
}

const apiShopForm = document.getElementById("shopForm");

if (apiShopForm) {
    apiShopForm.addEventListener("submit", async function (e) {
        e.preventDefault();

        const formData = new FormData(apiShopForm);

        // Lấy UserId từ localStorage
        const userId = localStorage.getItem("UserId");
        if (!userId) {
            alert("Bạn chưa đăng nhập! Không thể tạo shop.");
            return;
        }

        // Gửi lên API
        try {
            const response = await fetch("/api/shop/register", {
                method: "POST",
                body: formData,
                headers: {
                    "UserId": userId
                }
            });

            const result = await response.json();

            if (!result.success) {
                alert("❌ " + (result.message || "Đăng ký shop thất bại!"));
                return;
            }

            alert("🎉 Đăng ký shop thành công!");
            localStorage.removeItem("shopRegistrationDraft");

            // Redirect seller dashboard hoặc trang bạn muốn
            window.location.href = "/Shop/Dashboard";

        } catch (err) {
            console.error("Lỗi API:", err);
            alert("⚠️ Lỗi khi gọi API!");
        }
    });
}