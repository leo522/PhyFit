document.addEventListener("DOMContentLoaded", function () {
    function populateDateFields(data) {
        const yearSelect = document.getElementById("year");
        const monthSelect = document.getElementById("month");
        const daySelect = document.getElementById("day");

        // 填充年份選項
        data.Years.forEach(year => {
            const option = document.createElement("option");
            option.value = year;
            option.text = year;
            if (year === data.CurrentYear) {
                option.selected = true;
            }
            yearSelect.appendChild(option);
        });

        // 填充月份選項
        data.Months.forEach(month => {
            const option = document.createElement("option");
            option.value = month;
            option.text = month;
            if (month === data.CurrentMonth) {
                option.selected = true;
            }
            monthSelect.appendChild(option);
        });

        // 填充日期選項
        populateDays(data.CurrentYear, data.CurrentMonth);

        // 設定預設日期
        daySelect.value = data.CurrentDay;
    }

    function populateDays(year, month) {
        const daySelect = document.getElementById("day");
        daySelect.innerHTML = "";

        // 將月份轉換為JavaScript中的0基索引
        const daysInMonth = new Date(year, month, 0).getDate();
        for (let day = 1; day <= daysInMonth; day++) {
            const option = document.createElement("option");
            option.value = day;
            option.text = day;
            daySelect.appendChild(option);
        }
    }

    function updateDays() {
        const year = document.getElementById("year").value;
        const month = document.getElementById("month").value;
        populateDays(year, parseInt(month));
    }

    document.getElementById("year").addEventListener("change", updateDays);
    document.getElementById("month").addEventListener("change", updateDays);

    fetch(getDateDataUrl)
        .then(response => response.json())
        .then(data => populateDateFields(data));
});