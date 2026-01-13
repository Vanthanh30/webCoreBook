async function showRegistrationForm() {
    try {
        const res = await fetch("/api/shop/info", { method: "GET" });
        if (!res.ok) {
            const errorData = await res.json().catch(() => null);

            alert(errorData?.message || "Bạn cần cập nhật thông tin trước khi đăng ký!");

            window.location.href = "/Home/Index";
            return;
        }
        const data = await res.json();

        if (!data.success) {
            alert(data.message || "Bạn cần cập nhật thông tin trước khi đăng ký!");
            window.location.href = "/Home/Index";
            return;
        }

        document.getElementById('welcomePage').style.display = 'none';
        document.getElementById('registrationForm').classList.add('active');

    } catch (err) {
        alert("Lỗi kết nối server!");
        console.error(err);
    }
}

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

const shopNameInput = document.getElementById('shopName');
const charCountSpan = document.getElementById('charCount');

if (shopNameInput && charCountSpan) {
    shopNameInput.addEventListener('input', function () {
        charCountSpan.textContent = this.value.length;
    });
}

const descriptionInput = document.getElementById('description');
const descCharCountSpan = document.getElementById('descCharCount');

if (descriptionInput && descCharCountSpan) {
    descriptionInput.addEventListener('input', function () {
        descCharCountSpan.textContent = this.value.length;
    });
}

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

async function loadUserInfo() {
    const res = await fetch("/api/shop/info", { credentials: "include" });
    if (!res.ok) {
        return;
    }
    const data = await res.json();

    if (!data.success) return;

    document.getElementById("emailDisplay").value = data.email;
    document.getElementById("phoneDisplay").value = data.phone;

    document.getElementById("email").value = data.email;
    document.getElementById("phone").value = data.phone;
}

loadUserInfo();

if (apiShopForm) {
    apiShopForm.addEventListener("submit", async function (e) {
        e.preventDefault();

        const formData = new FormData(apiShopForm);

        try {
            const response = await fetch("/api/shop/register", {
                method: "POST",
                body: formData,
                credentials: "include"
            });

            const text = await response.text();
            console.log("RAW:", text);

            let result = JSON.parse(text);

            if (!result.success) {
                alert(" " + (result.message || "Đăng ký shop thất bại!"));
                return;
            }

            alert(" Đăng ký shop thành công!");
            window.location.href = "/SellerDashboard/Dashboard";

        } catch (err) {
            console.error("Lỗi API:", err);
            alert(" Lỗi khi gọi API!");
        }
    });
}

