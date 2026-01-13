function formatVND(value) {
    return Math.round(value).toLocaleString('vi-VN') + ' ₫';
}

$(document).ready(function () {
    function updatePrice(row) {
        var discountedPrice = parseFloat(row.find('.price-column').data('price')) *
            (1 - parseFloat(row.find('.price-column').data('discount')) / 100);
        var quantity = parseInt(row.find('.quantity-input').val());

        var totalPrice = discountedPrice * quantity;

        row.find('.price-column2').text(formatVND(totalPrice));

        row.find('.price-column2').data('total', totalPrice);
    }

    function updateSummary() {
        let totalAmount = 0;

        $(".select-item:checked").each(function () {
            let itemTotal = $(this).closest("tr").find(".price-column2").data("total");
            totalAmount += parseFloat(itemTotal);
        });

        let voucherPercent = 0;
        let voucherText = $("#voucher-discount").text();
        let match = voucherText.match(/Giảm\s+(\d+)%/);
        if (match) {
            voucherPercent = parseFloat(match[1]);
        }

        let discountAmount = totalAmount * (voucherPercent / 100);

        let finalAmount = totalAmount - discountAmount;

        $(".summary-amount").text(formatVND(totalAmount));
        $(".summary-discount").text(formatVND(discountAmount));
        $(".summary-total").text(formatVND(finalAmount));
    }

    function updateCartItemCount() {
        var itemCount = $("tr.cart-item").length;
        $("#cart-item-count").text(itemCount);

        if (itemCount === 0) {
            $("tbody").html('<tr><td colspan="5" class="text-center">Giỏ hàng trống</td></tr>');
            $(".select-all").prop("disabled", true);
        } else {
            $(".select-all").prop("disabled", false);
        }
    }
    function updateQuantityInDB(row, quantity) {
        var productId = row.data('product-id');
        $.ajax({
            url: '/Cart/UpdateQuantity',
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

    function saveSelectedProducts() {
        const selectedProductIds = $(".select-item:checked")
            .map(function () {
                return $(this).data("id").toString();
            })
            .get();

        console.log('Selected Product IDs:', selectedProductIds);

        fetch('/Cart/SaveSelectedProducts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(selectedProductIds),
        })
            .then(response => response.json())
            .then(data => {
                console.log('Selected products saved successfully');
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    function updateSelectAllState() {
        const totalItems = $(".select-item").length;
        const selectedItems = $(".select-item:checked").length;
        $(".select-all").prop("checked", totalItems === selectedItems);
    }

    function checkIfProductSelected() {
        if ($(".select-item:checked").length === 0) {
            $("#error-message").text("Vui lòng chọn ít nhất một sản phẩm để áp dụng khuyến mãi.");
            $("#error-message").show();
            return false;
        }
        $("#error-message").hide();
        return true;
    }
    function checkSelectedProducts() {
        var isAnyItemChecked = $(".select-item:checked").length > 0;

        if (!isAnyItemChecked) {
            $("#voucher-discount").html('<i class="fa-solid fa-tag"></i> Không có khuyến mãi.');
        }
    }

    $('.btn-minus').on('click', function () {
        var quantityInput = $(this).closest('.quantity').find('.quantity-input');
        var quantity = parseInt(quantityInput.val()) - 1;
        if (quantity >= 1) {
            quantityInput.val(quantity);
            updatePrice($(this).closest('tr'));
            updateSummary();
            updateQuantityInDB($(this).closest('tr'), quantity);
        }
    });

    $('.btn-plus').on('click', function () {
        var quantityInput = $(this).closest('.quantity').find('.quantity-input');
        var quantity = parseInt(quantityInput.val()) + 1;
        if (quantity <= 10) {
            quantityInput.val(quantity);
            updatePrice($(this).closest('tr'));
            updateSummary();
            updateQuantityInDB($(this).closest('tr'), quantity);
        }
    });
    $('.quantity-input').on('change', function () {
        var quantity = parseInt($(this).val());
        if (isNaN(quantity) || quantity < 1) {
            quantity = 1;
            $(this).val(quantity);
        } else if (quantity > 10) {
            quantity = 10;
            $(this).val(quantity);
        }
        updatePrice($(this).closest('tr'));
        updateSummary();
        updateQuantityInDB($(this).closest('tr'), quantity);
    });
    $(".select-item").change(function () {
        updateSummary();
        saveSelectedProducts();
        updateSelectAllState();
        checkSelectedProducts();

        if ($(".select-item:checked").length > 0) {
            $("#error-message").hide();
        }
    });
    $(".select-all").change(function () {
        const isChecked = $(this).prop("checked");
        $(".select-item").prop("checked", isChecked);
        updateSummary();
        saveSelectedProducts();
        checkSelectedProducts();

        if (isChecked) {
            $("#error-message").hide();
        }
    });
    $(".delete-product").click(function () {
        var productId = $(this).closest("tr").find(".select-item").data("id");
        var rowToDelete = $(this).closest("tr");

        $.ajax({
            url: '/Cart/DeleteProduct',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    rowToDelete.remove();
                    updateCartItemCount();

                    if ($("tr.cart-item").length === 0) {
                        $("tbody").html('<tr><td colspan="5" class="text-center">Giỏ hàng trống</td></tr>');
                        $("#cart-item-count").text("0");
                        $(".select-all").prop("disabled", true);
                    }

                    updateSummary();
                }
            },
            error: function (error) {
                alert("Có lỗi xảy ra khi xóa sản phẩm.");
            }
        });
    });

    $("#btn-edit").click(function () {
        $("#btn-delete-selected").show();
        $("#btn-cancel-edit").show();
        $(this).hide();
        $(".btn-pay").hide();
    });

    $("#btn-cancel-edit").click(function () {
        $("#btn-delete-selected").hide();
        $("#btn-cancel-edit").hide();
        $("#btn-edit").show();
        $(".btn-pay").show();
        $(".select-item").prop("checked", false);
        $(".select-all").prop("checked", false);
    });

    $("#btn-delete-selected").click(function () {
        var selectedIds = [];
        $(".select-item:checked").each(function () {
            selectedIds.push($(this).data("id"));
        });

        if (selectedIds.length === 0) {
            alert("Vui lòng chọn sản phẩm cần xóa.");
            return;
        }

        $.ajax({
            url: '/Cart/DeleteMultiple',
            type: 'POST',
            data: JSON.stringify(selectedIds),
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    selectedIds.forEach(function (id) {
                        $('tr[data-product-id="' + id + '"]').remove();
                    });
                    alert("Đã xóa thành công!");
                    $("#cart-item-count").text($(".cart-item").length);
                    updateSummary();
                } else {
                    alert("Không thể xóa sản phẩm.");
                }
            }
        });
    });

    $(".btn-pay").click(function (e) {
        if ($(".select-item:checked").length === 0) {
            e.preventDefault();
            alert("Vui lòng chọn ít nhất một sản phẩm để thanh toán.");
        }
    });

    $(".apply-discount").click(function (e) {
        if (!checkIfProductSelected()) {
            e.preventDefault();
        }
    });

    updateSelectAllState();
    checkSelectedProducts();
});