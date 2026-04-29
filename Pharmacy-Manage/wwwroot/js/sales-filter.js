document.addEventListener("DOMContentLoaded", function () {

    const dateInput = document.getElementById("filterDate");
    const tableBody = document.getElementById("salesTableBody");

    dateInput.addEventListener("change", function () {

        const selectedDate = this.value;

        if (!selectedDate) return;

        fetch('/Sales/GetSalesByDate?date=' + selectedDate)
            .then(res => res.json())
            .then(data => {

                tableBody.innerHTML = "";

                if (data.length === 0) {
                    tableBody.innerHTML = `
                        <tr>
                            <td colspan="4" class="text-center text-danger">
                                No sales found
                            </td>
                        </tr>`;
                    return;
                }

                data.forEach(item => {

                    const row = `
                        <tr>
                            <td>${item.date}</td>
                            <td>${item.medicine}</td>
                            <td>${item.qty}</td>
                            <td>₹ ${item.total}</td>
                        </tr>
                    `;

                    tableBody.innerHTML += row;
                });
            });
    });

});