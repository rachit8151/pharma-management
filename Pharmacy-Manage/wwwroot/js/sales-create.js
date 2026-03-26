document.addEventListener("DOMContentLoaded", function () {

    const medicineDropdown = document.getElementById("medicineDropdown");
    const priceInput = document.getElementById("price");
    const quantityInput = document.getElementById("quantity");
    const totalInput = document.getElementById("total");

    // Medicine change
    if (medicineDropdown) {
        medicineDropdown.addEventListener("change", function () {

            var id = this.value;

            if (!id) return;

            fetch('/Sales/GetMedicinePrice?id=' + id)
                .then(res => res.json())
                .then(data => {
                    priceInput.value = data;
                    calculateTotal();
                });
        });
    }

    // Quantity change
    if (quantityInput) {
        quantityInput.addEventListener("input", calculateTotal);
    }

    function calculateTotal() {
        let price = parseFloat(priceInput.value) || 0;
        let qty = parseInt(quantityInput.value) || 0;

        totalInput.value = (price * qty).toFixed(2);
    }

});