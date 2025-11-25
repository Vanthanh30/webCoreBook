/*$(document).ready(function () {
    // Sự kiện khi giảm số lượng
    $('.btn-minus').on('click', function () {
        var quantityInput = $(this).closest('.quantity').find('.quantity-input');
        var quantity = parseInt(quantityInput.val()) - 1;
        if (quantity >= 1) {
            quantityInput.val(quantity);
            updatePrice($(this).closest('tr'));
            updateQuantityInDB($(this).closest('tr'), quantity);  // Cập nhật số lượng trong MongoDB
        }
    });

    // Sự kiện khi tăng số lượng
    $('.btn-plus').on('click', function () {
        var quantityInput = $(this).closest('.quantity').find('.quantity-input');
        var quantity = parseInt(quantityInput.val()) + 1;
        if (quantity <= 10) { // Giới hạn số lượng tối đa
            quantityInput.val(quantity);
            updatePrice($(this).closest('tr'));
            updateQuantityInDB($(this).closest('tr'), quantity);  // Cập nhật số lượng trong MongoDB
        }
    });

    // Hàm tính lại thành tiền
    function updatePrice(row) {
        var discountedPrice = parseFloat(row.find('.price-column').data('price')) *
            (1 - parseFloat(row.find('.price-column').data('discount')) / 100);
        var quantity = parseInt(row.find('.quantity-input').val()); // Lấy số lượng từ input

        // Tính thành tiền với số lượng
        var totalPrice = discountedPrice * quantity;

        // Cập nhật lại giá thành tiền trong bảng
        row.find('.price-column2').text((totalPrice.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + ' ₫');
    }

    // Cập nhật giá trị khi người dùng thay đổi trực tiếp số lượng
    $('.quantity-input').on('change', function () {
        var quantity = parseInt($(this).val());
        if (isNaN(quantity) || quantity < 1) {
            quantity = 1;  // Nếu người dùng nhập giá trị không hợp lệ, mặc định là 1
            $(this).val(quantity);  // Cập nhật lại giá trị trong input
        } else if (quantity > 10) {
            quantity = 10; // Giới hạn số lượng tối đa
            $(this).val(quantity);
        }
        updatePrice($(this).closest('tr'));
        updateQuantityInDB($(this).closest('tr'), quantity);  // Cập nhật số lượng trong MongoDB
    });

    // Hàm gửi yêu cầu cập nhật số lượng vào MongoDB
    function updateQuantityInDB(row, quantity) {
        var productId = row.data('product-id'); // Lấy ID của sản phẩm từ data attribute hoặc id
        $.ajax({
            url: '/Cart/UpdateQuantity', // URL của action backend
            method: 'POST',
            data: {
                productId: productId,
                quantity: quantity
            },
            success: function (response) {
                console.log('Cập nhật số lượng thành công');
            },
            error: function (error) {
                console.log('Có lỗi xảy ra khi cập nhật số lượng');
            }
        });
    }
});
*/

//////////////Tinh tong tien, xóa sp khi chọn checkbox
$(document).ready(function () {
    function updateSummary() {
        let totalAmount = 0; // Tổng thành tiền của các sản phẩm được chọn
        let discount = 0; // Khuyến mãi cố định, bạn có thể thay đổi hoặc lấy từ server

        // Duyệt qua các checkbox được chọn
        $(".select-item:checked").each(function () {
            // Lấy giá trị thành tiền từ thuộc tính 'data-total' (số liệu gốc, chưa định dạng tiền tệ)
            let itemTotal = $(this).closest("tr").find(".price-column2").data("total");

            // Cộng dồn giá trị thành tiền
            totalAmount += parseFloat(itemTotal); // Chuyển đổi thành số
        });

        // Tính tổng tiền sau khi trừ khuyến mãi
        let totalAfterDiscount = totalAmount - discount;

        // Cập nhật giao diện
        $(".summary-amount").text((totalAmount.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + " ₫");
        $(".summary-discount").text((discount.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + " ₫");
        $(".summary-total").text((totalAfterDiscount.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + " ₫");
    }

    // Khi chọn checkbox sản phẩm
    $(".select-item").change(function () {
        updateSummary();
    });

    // Khi chọn "Chọn tất cả"
    $(".select-all").change(function () {
        $(".select-item").prop("checked", $(this).prop("checked"));
        updateSummary();
    });
    function updateCartItemCount() {
        // Cập nhật số lượng sản phẩm trong giỏ hàng
        var itemCount = $("tr.cart-item").length;
        $("#cart-item-count").text(itemCount); // Cập nhật lại số lượng sản phẩm

        // Kiểm tra xem giỏ hàng còn sản phẩm không
        if (itemCount === 0) {
            // Nếu không còn sản phẩm, hiển thị thông báo giỏ hàng trống
            $("tbody").html('<tr><td colspan="5" class="text-center">Giỏ hàng trống</td></tr>');
            $(".select-all").prop("disabled", true); // Tắt checkbox "Chọn tất cả"
        } else {
            $(".select-all").prop("disabled", false); // Bật lại checkbox "Chọn tất cả" nếu có sản phẩm
        }
    }
    // Xử lý thay đổi giá khi cập nhật giá sản phẩm
    $(".price-column2").on('input', function () {
        // Cập nhật giá trị mới cho thuộc tính 'data-total'
        var newPrice = $(this).val(); // Lấy giá mới từ input
        $(this).data("total", parseFloat(newPrice));

        // Cập nhật tổng tiền sau khi thay đổi giá
        updateSummary();
    });
    // Xử lý xóa sản phẩm khi nhấn nút xóa
    $(".delete-product").click(function () {
        var productId = $(this).closest("tr").find(".select-item").data("id");
        var rowToDelete = $(this).closest("tr"); // Lưu lại tham chiếu đến dòng sản phẩm

        // Gửi yêu cầu AJAX để xóa sản phẩm khỏi MongoDB
        $.ajax({
            url: '/Cart/DeleteProduct',  // Địa chỉ API xóa sản phẩm
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    // Xóa sản phẩm khỏi giao diện giỏ hàng
                    rowToDelete.remove();  // Xóa dòng sản phẩm khỏi giỏ hàng trên giao diện
                    updateCartItemCount();
                    // Kiểm tra xem giỏ hàng còn sản phẩm không
                    if ($("tr.cart-item").length === 0) {
                        // Nếu không còn sản phẩm, hiển thị thông báo giỏ hàng trống
                        $("tbody").html('<tr><td colspan="5" class="text-center">Giỏ hàng trống</td></tr>');
                        $("#cart-item-count").text("0");
                        $(".select-all").prop("disabled", true);  // Tắt checkbox "Chọn tất cả"
                    }

                    updateSummary();
                
                }
            },
            error: function (error) {
                alert("Có lỗi xảy ra khi xóa sản phẩm.");
            }
        });
    });
});


///////////////////////áp vc

$(document).ready(function () {
    // Kiểm tra xem có sản phẩm nào được chọn không
    function checkIfProductSelected() {
        if ($(".select-item:checked").length === 0) {
            // Hiển thị thông báo lỗi trong View
            $("#error-message").text("Vui lòng chọn ít nhất một sản phẩm để áp dụng khuyến mãi.");
            $("#error-message").show(); // Hiển thị lỗi
            return false;
        }
        // Ẩn thông báo lỗi nếu có sản phẩm được chọn
        $("#error-message").hide();
        return true;
    }

    // Khi click vào liên kết chọn khuyến mãi
    $(".apply-discount").click(function (e) {
        // Kiểm tra xem người dùng đã chọn sản phẩm chưa
        if (!checkIfProductSelected()) {
            e.preventDefault(); // Ngừng chuyển trang nếu chưa chọn sản phẩm
        }
    });

    // Khi chọn checkbox sản phẩm
    $(".select-item").change(function () {
        // Cập nhật trạng thái của checkbox "Chọn tất cả"
        if ($(".select-item:checked").length === $(".select-item").length) {
            $(".select-all").prop("checked", true);
        } else {
            $(".select-all").prop("checked", false);
        }

        // Ẩn thông báo lỗi nếu đã chọn ít nhất một sản phẩm
        if ($(".select-item:checked").length > 0) {
            $("#error-message").hide();
        }
    });

    // Khi chọn "Chọn tất cả"
    $(".select-all").change(function () {
        $(".select-item").prop("checked", $(this).prop("checked"));

        // Ẩn thông báo lỗi nếu đã chọn ít nhất một sản phẩm
        if ($(this).prop("checked")) {
            $("#error-message").hide();
        }
    });
});




/////khi quay lại từ trang vc thì vẫn giữ được trạng thái
$(document).ready(function () {
    // Lưu trạng thái sản phẩm đã chọn lên server
    function saveSelectedProducts() {
        const selectedProductIds = $(".select-item:checked")
            .map(function () {
                return $(this).data("id").toString(); // Lấy ID của sản phẩm đã chọn và chuyển thành string
            })
            .get();

        // Kiểm tra dữ liệu trước khi gửi lên server
        console.log('Selected Product IDs:', selectedProductIds);

        // Gửi dữ liệu sản phẩm đã chọn lên server
        fetch('/Cart/SaveSelectedProducts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(selectedProductIds),  // Gửi mảng trực tiếp
        })
            .then(response => response.json()) // Đảm bảo nhận phản hồi JSON từ server
            .then(data => {
                // Xử lý phản hồi từ server nếu cần
                console.log('Selected products saved successfully');
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // Khi trạng thái của checkbox thay đổi
    $(".select-item").change(function () {
        saveSelectedProducts(); // Lưu trạng thái sản phẩm đã chọn
        updateSelectAllState();
    });

    // Khi trạng thái của checkbox "Chọn tất cả" thay đổi
    $(".select-all").change(function () {
        const isChecked = $(this).prop("checked");
        $(".select-item").prop("checked", isChecked); // Chọn hoặc bỏ chọn tất cả các checkbox sản phẩm
        saveSelectedProducts(); // Lưu trạng thái
    });

    // Cập nhật trạng thái checkbox "Chọn tất cả"
    function updateSelectAllState() {
        const totalItems = $(".select-item").length;
        const selectedItems = $(".select-item:checked").length;

        // Nếu tất cả các item được chọn, đánh dấu "Chọn tất cả" là checked
        $(".select-all").prop("checked", totalItems === selectedItems);
    }

    // Cập nhật trạng thái ban đầu của checkbox "Chọn tất cả"
    updateSelectAllState();
});


////////////////


// Cập nhật giao diện giỏ hàng
function updateCartSummary(discount, discountType) {
    // Lấy giá trị thành tiền ban đầu từ giỏ hàng (ví dụ)
    let amount = parseFloat(document.querySelector('.summary-amount').textContent.replace('đ', '').trim());

    // Tính toán giảm giá
    let discountValue = 0;
    if (discountType === 'Percentage') {
        discountValue = amount * (discount / 100);
    } else {
        discountValue = discount;
    }

    // Cập nhật giao diện giỏ hàng
    document.querySelector('.summary-discount').textContent = `${discountValue.toFixed(0)}đ`;
    document.querySelector('.summary-total').textContent = `${(amount - discountValue).toFixed(0)}đ`;
}

///////////
$(document).ready(function () {
    // Cập nhật giá thành tiền cho mỗi sản phẩm
    function updatePrice(row) {
        var discountedPrice = parseFloat(row.find('.price-column').data('price')) *
            (1 - parseFloat(row.find('.price-column').data('discount')) / 100);
        var quantity = parseInt(row.find('.quantity-input').val()); // Lấy số lượng từ input

        // Tính thành tiền với số lượng
        var totalPrice = discountedPrice * quantity;

        // Cập nhật lại giá thành tiền trong bảng
        row.find('.price-column2').text((totalPrice.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + ' ₫');

        // Cập nhật lại dữ liệu tổng tiền của sản phẩm
        row.find('.price-column2').data('total', totalPrice);
    }

    // Cập nhật tổng tiền của giỏ hàng
    function updateSummary() {
        let totalAmount = 0; // Tổng thành tiền của các sản phẩm được chọn
        let discount = 0; // Khuyến mãi cố định, bạn có thể thay đổi hoặc lấy từ server

        // Duyệt qua các checkbox được chọn
        $(".select-item:checked").each(function () {
            // Lấy giá trị thành tiền từ thuộc tính 'data-total' (số liệu gốc, chưa định dạng tiền tệ)
            let itemTotal = $(this).closest("tr").find(".price-column2").data("total");

            // Cộng dồn giá trị thành tiền
            totalAmount += parseFloat(itemTotal); // Chuyển đổi thành số
        });

        // Tính tổng tiền sau khi trừ khuyến mãi
        let totalAfterDiscount = totalAmount - discount;

        // Cập nhật giao diện
        $(".summary-amount").text((totalAmount.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + " ₫");
        $(".summary-discount").text((discount.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + " ₫");
        $(".summary-total").text((totalAfterDiscount.toFixed(2)).replace(/\d(?=(\d{3})+\.)/g, '$&,') + " ₫");
    }

    // Sự kiện khi giảm số lượng
    $('.btn-minus').on('click', function () {
        var quantityInput = $(this).closest('.quantity').find('.quantity-input');
        var quantity = parseInt(quantityInput.val()) - 1;
        if (quantity >= 1) {
            quantityInput.val(quantity);
            updatePrice($(this).closest('tr'));
            updateSummary();  // Cập nhật lại tổng tiền giỏ hàng
            updateQuantityInDB($(this).closest('tr'), quantity);  // Cập nhật số lượng trong MongoDB
        }
    });

    // Sự kiện khi tăng số lượng
    $('.btn-plus').on('click', function () {
        var quantityInput = $(this).closest('.quantity').find('.quantity-input');
        var quantity = parseInt(quantityInput.val()) + 1;
        if (quantity <= 10) { // Giới hạn số lượng tối đa
            quantityInput.val(quantity);
            updatePrice($(this).closest('tr'));
            updateSummary();  // Cập nhật lại tổng tiền giỏ hàng
            updateQuantityInDB($(this).closest('tr'), quantity);  // Cập nhật số lượng trong MongoDB
        }
    });

    // Cập nhật giá trị khi người dùng thay đổi trực tiếp số lượng
    $('.quantity-input').on('change', function () {
        var quantity = parseInt($(this).val());
        updatePrice($(this).closest('tr'));
        updateSummary();  // Cập nhật lại tổng tiền giỏ hàng
        updateQuantityInDB($(this).closest('tr'), quantity);  // Cập nhật số lượng trong MongoDB
    });

    // Cập nhật số lượng trong MongoDB
    function updateQuantityInDB(row, quantity) {
        var productId = row.data('product-id'); // Lấy ID của sản phẩm từ data attribute hoặc id
        $.ajax({
            url: '/Cart/UpdateQuantity', // URL của action backend
            method: 'POST',
            data: {
                productId: productId,
                quantity: quantity
            },
            success: function (response) {
                console.log('Cập nhật số lượng thành công');
            },
            error: function (error) {
                console.log('Có lỗi xảy ra khi cập nhật số lượng');
            }
        });
    }

    // Cập nhật tổng tiền giỏ hàng
    $(".select-item").change(function () {
        updateSummary();
    });

    $(".select-all").change(function () {
        $(".select-item").prop("checked", $(this).prop("checked"));
        updateSummary();
    });
});


////////////////


$(document).ready(function () {
    // Hàm kiểm tra trạng thái của các checkbox
    function checkSelectedProducts() {
        // Kiểm tra xem có checkbox nào được chọn không
        var isAnyItemChecked = $(".select-item:checked").length > 0;

        // Thay đổi nội dung khuyến mãi nếu không có sản phẩm nào được chọn
        if (isAnyItemChecked) {
            if (voucherDiscount) {
                $("#voucher-discount").html('<i class="fa-solid fa-tag"></i> Không có khuyến mãi.');
            }
        } else {
            // Nếu không có sản phẩm nào được chọn, hiển thị "Không có khuyến mãi"
            $("#voucher-discount").html('<i class="fa-solid fa-tag"></i> Không có khuyến mãi.');
        }
    }

    // Lắng nghe sự thay đổi của các checkbox
    $(".select-item").change(function () {
        checkSelectedProducts(); // Kiểm tra lại sau mỗi lần thay đổi checkbox
    });
    // Lắng nghe sự thay đổi của checkbox select-all
    $(".select-all").change(function () {
        // Kiểm tra lại sau khi chọn hoặc bỏ chọn "Chọn tất cả"
        checkSelectedProducts();
    });

    // Khởi chạy kiểm tra ban đầu khi trang được tải
    checkSelectedProducts();
});
