let lastStatusCheck = "";
function checkRequestStatus() {

    fetch('/Pharmacist/GetLatestRequestStatus')
        .then(response => response.json())
        .then(data => {

            if (data.status !== lastStatusCheck) {

                lastStatusCheck = data.status;

                if (data.status === "approved" || data.status === "rejected") {

                    alert("Your medicine request was " + data.status);

                    const audio = new Audio('/sounds/tring.mp3');
                    audio.play();
                }
            }

        })
        .catch(error => console.error("Status check error:", error));
}

// run every 5 seconds
setInterval(checkRequestStatus, 5000);