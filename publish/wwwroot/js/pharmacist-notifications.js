function loadPharmacistNotifications() {

    fetch('/Admin/GetPendingPharmacistCount')
        .then(response => response.json())
        .then(data => {

            const badge = document.getElementById("pharmacistBadge");

            if (!badge) return;

            if (data > 0) {
                badge.innerText = data;
                badge.style.display = "inline-block";
            }
            else {
                badge.style.display = "none";
            }

        })
        .catch(error => console.error("Pharmacist notification error:", error));
}

document.addEventListener("DOMContentLoaded", function () {

    loadPharmacistNotifications();

    setInterval(loadPharmacistNotifications, 5000);

});