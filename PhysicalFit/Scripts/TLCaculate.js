//// TLCalculation.js

//$(document).ready(function () {
//    // 一般訓練TL計算
//    $('#btn_calculate').click(function (event) {
//        event.preventDefault();

//        var totalDailyTL = 0;  // 用來存儲每日TL的總和

//        $('#trainingMonitoring table tbody tr').each(function () {
//            var rpeValue = $(this).find('input[name="RPE"]').val();  // 從隱藏的 input 取得分數
//            var trainingTimeValue = $(this).find('select[name="TrainingTime"]').val();

//            if (rpeValue && trainingTimeValue) {
//                var rpe = parseFloat(rpeValue);
//                var trainingTime = parseFloat(trainingTimeValue);

//                var sessionTL = rpe * trainingTime;

//                $(this).find('input[name="SessionTL"]').val(sessionTL);

//                totalDailyTL += sessionTL;  // 累加到每日TL的總和
//            } else {
//                $(this).find('input[name="SessionTL"]').val('');
//            }
//        });

//        // 更新每日TL欄位的值
//        $('input[name="DailyTL"]').val(totalDailyTL);
//    });

//    //射箭訓練TL計算
//    $('#btn_calculate_archery').click(function (event) {
//        event.preventDefault();

//        var totalDailyTL = 0;  // 用來存儲每日TL的總和

//        $('#archeryMonitoring table tbody tr').each(function () {
//            var ArcheryrpeValue = $(this).find('input[name="RPEArchery"]').val();  // 從隱藏的 input 取得分數
//            var PoundsValue = $(this).find('input[name="Pounds"]').val();
//            var ArrowsValue = $(this).find('input[name="Arrows"]').val();

//            if (ArcheryrpeValue && PoundsValue && ArrowsValue) {
//                var Archeryrpe = parseFloat(ArcheryrpeValue);
//                var trainingPounds = parseFloat(PoundsValue);
//                var trainingArrows = parseFloat(ArrowsValue);

//                var ArcherysessionTL = Archeryrpe * trainingPounds * trainingArrows;

//                $(this).find('input[name="SessionArcheryTL"]').val(ArcherysessionTL);

//                totalDailyTL += ArcherysessionTL;  // 累加到每日TL的總和
//            } else {
//                $(this).find('input[name="SessionArcheryTL"]').val('');
//            }
//        });
//        // 更新每日TL欄位的值
//        $('input[name="ArcheryDailyTL"]').val(totalDailyTL);
//    });

//    //射擊訓練TL計算
//    $('#btn_calculate_shooting').click(function (event) {
//        event.preventDefault();

//        var totalDailyTL = 0;  // 用來存儲每日TL的總和

//        $('#shootingMonitoring table tbody tr').each(function () {
//            var ShootingValue = $(this).find('input[name="RPEshooting"]').val();  // 從隱藏的 input 取得分數
//            var BulletValue = $(this).find('input[name="Bullet"]').val();

//            if (ShootingValue && BulletValue) {
//                var Shootingrpe = parseFloat(ShootingValue);
//                var trainingBullet = parseFloat(BulletValue);

//                var ShootingsessionTL = Shootingrpe * trainingBullet;

//                $(this).find('input[name="SessionShootingTL"]').val(ShootingsessionTL);

//                totalDailyTL += ShootingsessionTL;  // 累加到每日TL的總和
//            } else {
//                $(this).find('input[name="SessionShootingTL"]').val('');
//            }
//        });
//        // 更新每日TL欄位的值
//        $('input[name="ShootingDailyTL"]').val(totalDailyTL);
//    });

//    //切換項目就清空每日TL
//    $('#Item').change(function () {
//        $('input[name="DailyTL"]').val('') && $('input[name="ArcheryDailyTL"]').val('') && $('input[name="ShootingDailyTL"]').val('');
//    });
//    $('#TrainingItem').change(function () {
//        $('input[name="DailyTL"]').val('');
//    });

//    // 新增一般訓練衝量監控
//    var maxArcheryRows = 7; //欄位增加上限7筆

//    $(document).on('click', '.add-row', function () {
//        var rowCount = $('#trainingMonitoringRows .training-group').length;
//        if (rowCount < maxArcheryRows) {
//            var newRow = $(this).closest('tr').clone();
//            newRow.find('input').val('');
//            newRow.find('select').prop('selectedIndex', 0);
//            $(this).closest('tbody').prepend(newRow); // 新增行到最上方
//        } else
//        {
//            alert("已達到最大行數，無法新增更多。");
//        }
//    });

//    // 刪除一般訓練衝量監控
//    $(document).on('click', '.remove-row', function () {
//        var rowCount = $(this).closest('tbody').find('tr').length;
//        if (rowCount > 1) {
//            $(this).closest('tr').remove();
//        } else {
//            alert("至少需要一個訓練項目。");
//        }
//    });

//    // 新增射箭訓練

//    $(document).on('click', '.add-row-archery', function () {
//        var rowCount = $('#ArcherytrainingRows .Archerytraining-group').length;

//        // 檢查行數是否超過上限
//        if (rowCount < maxArcheryRows) {
//            var newRow = $(this).closest('.Archerytraining-group').clone();
//            newRow.find('input').val('');
//            newRow.find('select').prop('selectedIndex', 0);
//            $('#ArcherytrainingRows').prepend(newRow); // 新增行到最上方
//        } else {
//            alert("已達到最大行數，無法新增更多。");
//        }
//    });

//    // 刪除射箭訓練
//    $(document).on('click', '.remove-row-archery', function () {
//        var rowCount = $('#ArcherytrainingRows .Archerytraining-group').length;
//        if (rowCount > 1) {
//            $(this).closest('.Archerytraining-group').remove();
//        } else {
//            alert("至少需要一個訓練項目。");
//        }
//    });

//    // 新增射擊訓練
//    $(document).on('click', '.add-row-shooting', function () {
//        var rowCount = $('#ShootingtrainingRows .Shootingtraining-group').length;

//        // 檢查行數是否超過上限
//        if (rowCount < maxArcheryRows) {
//            var newRow = $(this).closest('.Shootingtraining-group').clone();
//            newRow.find('input').val('');
//            newRow.find('select').prop('selectedIndex', 0);
//            $('#ShootingtrainingRows').prepend(newRow); // 新增行到最上方
//        } else {
//            alert("已達到最大行數，無法新增更多。");
//        }
//    });

//    // 刪除射擊訓練
//    $(document).on('click', '.remove-row-shooting', function () {
//        var rowCount = $('#ShootingtrainingRows .Shootingtraining-group').length;
//        if (rowCount > 1) {
//            $(this).closest('.Shootingtraining-group').remove();
//        } else {
//            alert("至少需要一個訓練項目。");
//        }
//    });
//});
$(document).ready(function () {
    // 計算每日運動負荷的通用函數
    function calculateDailyTLForDate(dateSelector, tlInputSelector, calculationFn) {
        var dateToTL = {};  // 用來存儲每個日期的TL總和

        $(dateSelector).each(function () {
            var date = $(this).val();  // 取得日期
            var tl = calculationFn($(this).closest('tr'));

            if (date && tl !== null) {
                if (!dateToTL[date]) {
                    dateToTL[date] = 0;
                }
                dateToTL[date] += tl;
            }
        });

        // 將每個日期的TL結果顯示在對應的輸入框中
        $(tlInputSelector).each(function () {
            var date = $(this).closest('tr').find(dateSelector).val();
            if (date && dateToTL[date] !== undefined) {
                $(this).val(dateToTL[date]);
            } else {
                $(this).val('');  // 若沒有值則清空
            }
        });
    }

    // 一般訓練TL計算
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
    // 射箭訓練TL計算
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

    // 射擊訓練TL計算
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

    // 綁定計算按鈕
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

    // 新增行處理
    function addRow(selector, maxRows) {
        var rowCount = $(selector).find('tr').length;
        if (rowCount < maxRows) {
            var newRow = $(selector).find('tr:last').clone();
            newRow.find('input, select').val('');
            $(selector).prepend(newRow);  // 新增行到最上方
        } else {
            alert("已達到最大行數，無法新增更多。");
        }
    }

    // 刪除行處理
    function removeRow($row, dateSelector, tlInputSelector, calculationFn) {
        var date = $row.find(dateSelector).val();
        var tl = calculationFn($row);

        $row.remove();
        if (date) {
            calculateDailyTLForDate(dateSelector, tlInputSelector, calculationFn);
        }
    }

    // 新增一般訓練行
    $(document).on('click', '.add-row', function () {
        addRow('#trainingMonitoringRows', 7);
    });

    // 刪除一般訓練行
    $(document).on('click', '.remove-row', function () {
        var $row = $(this).closest('tr');
        var rowCount = $('#trainingMonitoringRows').find('tr').length;
        if (rowCount > 1) {
            removeRow($row, 'input[name="TrainingDate"]', 'input[name="DailyTL"]', calculateDailyTL);
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    // 新增射箭訓練行
    $(document).on('click', '.add-row-archery', function () {
        addRow('#ArcherytrainingRows', 7);
    });

    // 刪除射箭訓練行
    $(document).on('click', '.remove-row-archery', function () {
        var $row = $(this).closest('.Archerytraining-group');
        var rowCount = $('#ArcherytrainingRows').find('.Archerytraining-group').length;
        if (rowCount > 1) {
            removeRow($row, 'input[name="archeryDate"]', 'input[name="ArcheryDailyTL"]', calculateArcheryTL);
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    // 新增射擊訓練行
    $(document).on('click', '.add-row-shooting', function () {
        addRow('#ShootingtrainingRows', 7);
    });

    // 刪除射擊訓練行
    $(document).on('click', '.remove-row-shooting', function () {
        var $row = $(this).closest('.Shootingtraining-group');
        var rowCount = $('#ShootingtrainingRows').find('.Shootingtraining-group').length;
        if (rowCount > 1) {
            removeRow($row, 'input[name="shootingDate"]', 'input[name="ShootingDailyTL"]', calculateShootingTL);
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    // 切換項目就清空每日TL
    $('#Item').change(function () {
        $('input[name="DailyTL"], input[name="ArcheryDailyTL"], input[name="ShootingDailyTL"]').val('');
    });

    $('#TrainingItem').change(function () {
        $('input[name="DailyTL"]').val('');
    });
});