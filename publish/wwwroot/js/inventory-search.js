document.addEventListener("DOMContentLoaded", function () {

    const searchBox = document.getElementById("searchBox");
    const statusFilter = document.getElementById("statusFilter");

    function filterTable() {

        let searchText = searchBox.value.toLowerCase();
        let statusValue = statusFilter.value;

        let rows = document.querySelectorAll("#inventoryTable tr");

        rows.forEach(row => {

            let medicine = row.querySelector(".medicineName").innerText.toLowerCase();
            let rowStatus = row.getAttribute("data-status");

            let matchSearch = medicine.includes(searchText);
            let matchStatus = statusValue === "" || rowStatus === statusValue;

            if (matchSearch && matchStatus)
                row.style.display = "";
            else
                row.style.display = "none";

        });
    }

    searchBox.addEventListener("keyup", filterTable);
    statusFilter.addEventListener("change", filterTable);

});