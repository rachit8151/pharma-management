function loadMedicineRequestNotifications() {

    fetch('/Admin/GetPendingMedicineRequestCount')
        .then(response => response.json())
        .then(data => {

            const badge = document.getElementById("medicineRequestBadge");

            if (!badge) return;

            if (data > 0) {
                badge.innerText = data;
                badge.style.display = "inline-block";
            }
            else {
                badge.style.display = "none";
            }

        })
        .catch(error => console.error("Medicine notification error:", error));
}

document.addEventListener("DOMContentLoaded", function () {

    loadMedicineRequestNotifications();

    setInterval(loadMedicineRequestNotifications, 5000);

});