document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.btn-minus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = this.nextElementSibling; 
            let value = parseInt(input.value) || 1;
            if (value > 1) {
                input.value = value - 1; 
            }
            let updatedQuantity = parseInt(input.value);
            console.log("Số lượng sau khi thay đổi: " + updatedQuantity); 
        });
    });

    document.querySelectorAll('.btn-plus').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = this.previousElementSibling; 
            let value = parseInt(input.value) || 1;
            input.value = value + 1; 
            let updatedQuantity = parseInt(input.value);
            console.log("Số lượng sau khi thay đổi: " + updatedQuantity); 
        });
    });
});
