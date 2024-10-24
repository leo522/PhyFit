// datepickr.js

// 封裝初始化日期選擇器的功能
/*datepickr.js*/
function initializeDatePickers() {
    // 檢測系統日期選擇器初始化
    flatpickr("#DetectionDateTime", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        time_24hr: true,
        "locale": "zh_tw",
    });

    // 射箭日期選擇器初始化
    flatpickr(".archeryDate", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        time_24hr: true,
        "locale": "zh_tw",
    });

    // 射擊日期選擇器初始化
    flatpickr(".shootingDate", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        time_24hr: true,
        "locale": "zh_tw",
    });
}

// 自動初始化日期選擇器，當 DOM 完全加載後執行
document.addEventListener('DOMContentLoaded', function () {
    initializeDatePickers();
});