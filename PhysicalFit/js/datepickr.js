function initializeDatePickers() {
    flatpickr("#DetectionDateTime", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        time_24hr: true,
        "locale": "zh_tw",
    });

    flatpickr(".archeryDate", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        time_24hr: true,
        "locale": "zh_tw",
    });

    flatpickr(".shootingDate", {
        enableTime: true,
        dateFormat: "Y-m-d H:i",
        time_24hr: true,
        "locale": "zh_tw",
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initializeDatePickers();
});