$(document).ready(function () {
    var categoryId = '@Html.Raw(ViewBag.CategoryId)'; ;

    if (categoryId) {
        console.log("Category ID:", categoryId);
    } else {
        console.log("Category ID is not set or is empty.");
    }

    $('#sortDiscountBtn').click(function () {
        var sortOrder = $('#sortPriceSelect').val();
        loadProducts(categoryId, true, sortOrder);
    });

    $('#sortPriceSelect').change(function () {
        var sortOrder = $(this).val();
        loadProducts(categoryId, true, sortOrder);
    });

    function loadProducts(categoryId, filterByDiscount, sortOrder) {
        console.log("CategoryId trong hàm loadProducts:", categoryId); 

        $.ajax({
            url: '@Url.Action("GetProductsByCategoryId", "Product")',
            type: 'GET',
            data: {
                categoryId: categoryId,
                filterByDiscount: filterByDiscount,
                sortOrder: sortOrder
            },
            success: function (response) {
                $('#productRow').html(response);
            },
            error: function () {
                alert("Lỗi khi tải sản phẩm.");
            }
        });
    }
});