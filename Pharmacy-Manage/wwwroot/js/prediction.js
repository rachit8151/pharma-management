document.getElementById("btnPredict").addEventListener("click", async () => {

    const res = await fetch('/Pharmacist/GetPredictionData');
    const data = await res.json();

    const rows = data.data;

    const labels = rows.map(x => x.date);

    const medicines = Object.keys(rows[0]).filter(k => k !== "date");

    const datasets = medicines.map(med => ({
        label: med,
        data: rows.map(x => x[med]),
        borderWidth: 2
    }));

    const ctx = document.getElementById('predictionChart').getContext('2d');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: datasets
        }
    });

});