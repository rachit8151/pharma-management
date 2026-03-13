document.addEventListener("DOMContentLoaded", function () {

    fetch('/Home/GetUserRoleStats')
        .then(response => response.json())
        .then(data => {

            const ctx = document.getElementById('userRoleChart');

            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['Admin', 'Pharmacist'],
                    datasets: [{
                        label: 'Active Users',
                        data: [data.admin, data.pharmacist],
                        backgroundColor: [
                            '#2F80ED',
                            '#EB5757',
                        ],
                        borderRadius: 6,
                        barThickness: 50
                    }]
                },
                options: {
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    }
                }
            });

        })
        .catch(error => console.error(error));

});