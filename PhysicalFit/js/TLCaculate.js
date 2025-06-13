$(document).ready(function () {
    function initializeFlatpickr() {
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

    initializeFlatpickr();

    function calculateDailyTLForDate(dateSelector, tlInputSelector, calculationFn) {
        var dateToTL = {};

        $(dateSelector).each(function () {
            var date = $(this).val();
            var tl = calculationFn($(this).closest('tr'));

            if (date && tl !== null) {
                if (!dateToTL[date]) {
                    dateToTL[date] = 0;
                }
                dateToTL[date] += tl;
            }
        });

        $(tlInputSelector).each(function () {
            var date = $(this).closest('tr').find(dateSelector).val();
            if (date && dateToTL[date] !== undefined) {
                $(this).val(dateToTL[date]);
            } else {
                $(this).val('');
            }
        });
    }

    function calculateDailyTL($row) {
        var rpeValue = $row.find('input[name="RPE"]').val();
        var trainingTimeValue = $row.find('select[name="TrainingTime"]').val();

        if (rpeValue && trainingTimeValue) {
            var rpe = parseFloat(rpeValue);
            var trainingTime = parseFloat(trainingTimeValue);
            var sessionTL = rpe * trainingTime;
            $row.find('input[name="SessionTL"]').val(sessionTL);
            return sessionTL;
        } else {
            $row.find('input[name="SessionTL"]').val('');
            return null;
        }
    }

    function calculateArcheryTL($row) {
        var rpeValue = $row.find('input[name="RPEArchery"]').val();
        var poundsValue = $row.find('input[name="Pounds"]').val();
        var arrowsValue = $row.find('input[name="Arrows"]').val();

        if (rpeValue && poundsValue && arrowsValue) {
            var rpe = parseFloat(rpeValue);
            var pounds = parseFloat(poundsValue);
            var arrows = parseFloat(arrowsValue);
            var sessionTL = rpe * pounds * arrows;
            $row.find('input[name="SessionArcheryTL"]').val(sessionTL);
            return sessionTL;
        } else {
            $row.find('input[name="SessionArcheryTL"]').val('');
            return 0;
        }
    }

    function calculateShootingTL($row) {
        var rpeValue = $row.find('input[name="RPEshooting"]').val();
        var bulletValue = $row.find('input[name="Bullet"]').val();

        if (rpeValue && bulletValue) {
            var rpe = parseFloat(rpeValue);
            var bullets = parseFloat(bulletValue);
            var sessionTL = rpe * bullets;
            $row.find('input[name="SessionShootingTL"]').val(sessionTL);
            return sessionTL;
        } else {
            $row.find('input[name="SessionShootingTL"]').val('');
            return 0;
        }
    }

    $('#btn_calculate').click(function (event) {
        event.preventDefault();
        calculateDailyTLForDate('input[name="TrainingDate"]', 'input[name="DailyTL"]', calculateDailyTL);
    });

    $('#btn_calculate_archery').click(function (event) {
        event.preventDefault();
        calculateDailyTLForDate('input[name="archeryDate"]', 'input[name="ArcheryDailyTL"]', calculateArcheryTL);
    });

    $('#btn_calculate_shooting').click(function (event) {
        event.preventDefault();
        calculateDailyTLForDate('input[name="shootingDate"]', 'input[name="ShootingDailyTL"]', calculateShootingTL);
    });

    function addRow(selector, maxRows) {
        var rowCount = $(selector).find('tr').length;
        if (rowCount < maxRows) {
            var newRow = $(selector).find('tr:last').clone();
            newRow.find('input, select').val('');
            $(selector).append(newRow);
            initializeFlatpickr();
        } else {
            alert("已達到最大行數，無法新增更多。");
        }
    }

    function removeRow($row, dateSelector, tlInputSelector, calculationFn) {
        var date = $row.find(dateSelector).val();
        var tl = calculationFn($row);

        $row.remove();
        if (date) {
            calculateDailyTLForDate(dateSelector, tlInputSelector, calculationFn);
        }
    }

    $(document).on('click', '.add-row', function () {
        addRow('#trainingMonitoringRows', 7);
    });

    $(document).on('click', '.remove-row', function () {
        var $row = $(this).closest('tr');
        var rowCount = $('#trainingMonitoringRows').find('tr').length;
        if (rowCount > 1) {
            removeRow($row, 'input[name="TrainingDate"]', 'input[name="DailyTL"]', calculateDailyTL);
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    $(document).on('click', '.add-row-archery', function () {
        addRow('#ArcherytrainingRows', 7);
    });

    $(document).on('click', '.remove-row-archery', function () {
        var $row = $(this).closest('.Archerytraining-group');
        var rowCount = $('#ArcherytrainingRows').find('.Archerytraining-group').length;
        if (rowCount > 1) {
            removeRow($row, 'input[name="archeryDate"]', 'input[name="ArcheryDailyTL"]', calculateArcheryTL);
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    $(document).on('click', '.add-row-shooting', function () {
        addRow('#ShootingtrainingRows', 7);
    });

    $(document).on('click', '.remove-row-shooting', function () {
        var $row = $(this).closest('.Shootingtraining-group');
        var rowCount = $('#ShootingtrainingRows').find('.Shootingtraining-group').length;
        if (rowCount > 1) {
            removeRow($row, 'input[name="shootingDate"]', 'input[name="ShootingDailyTL"]', calculateShootingTL);
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    $('#Item').change(function () {
        $('input[name="DailyTL"], input[name="ArcheryDailyTL"], input[name="ShootingDailyTL"]').val('');
    });

    $('#TrainingItem').change(function () {
        $('input[name="DailyTL"]').val('');
    });
});