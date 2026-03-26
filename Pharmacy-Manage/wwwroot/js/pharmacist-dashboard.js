document.addEventListener("DOMContentLoaded", function () {

    const dateInput = document.getElementById("chartDate");
    const ctx = document.getElementById("salesChart");

    let chart;

    let today = new Date().toISOString().split('T')[0];
    dateInput.value = today;

    loadChart(today);

    dateInput.addEventListener("change", function () {
        loadChart(this.value);
    });

    function loadChart(date) {

        fetch('/Pharmacist/GetSalesChartData?date=' + date)
            .then(res => res.json())
            .then(data => {

                let labels = data.map(x => x.name);
                let values = data.map(x => x.qty);

                if (chart) {
                    chart.destroy();
                }

                chart = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Quantity Sold',
                            data: values
                        }]
                    },
                    options: {
                        responsive: true
                    }
                });

            });
    }

});