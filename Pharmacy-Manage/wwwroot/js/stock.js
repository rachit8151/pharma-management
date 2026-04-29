document.getElementById("btnStock").addEventListener("click", async () => {

    const res = await fetch('/Pharmacist/GetStockSuggestions');
    const data = await res.json();

    const tbody = document.querySelector("#stockTable tbody");
    tbody.innerHTML = "";

    data.forEach(item => {

        let row = `<tr>
            <td>${item.medicine}</td>
            <td>${item.stock}</td>
            <td>${item.predicted}</td>
            <td>${item.order}</td>
            <td style="color:${item.status === 'LOW' ? 'red' : 'green'}">
                ${item.status}
            </td>
        </tr>`;

        tbody.innerHTML += row;
    });

});