// TLCalculation.js

$(document).ready(function () {
    // 計算每次TL
    $('#btn_calculate').click(function (event) {
        event.preventDefault();

        var totalDailyTL = 0;  // 用來存儲每日TL的總和

        $('#trainingMonitoring table tbody tr').each(function () {
            var rpeValue = $(this).find('input[name="RPE"]').val();  // 從隱藏的 input 取得分數
            var trainingTimeValue = $(this).find('select[name="TrainingTime"]').val();

            if (rpeValue && trainingTimeValue) {
                var rpe = parseFloat(rpeValue);
                var trainingTime = parseFloat(trainingTimeValue);

                var sessionTL = rpe * trainingTime;

                $(this).find('input[name="SessionTL"]').val(sessionTL);

                totalDailyTL += sessionTL;  // 累加到每日TL的總和
            } else {
                $(this).find('input[name="SessionTL"]').val('');
            }
        });

        // 更新每日TL欄位的值
        $('input[name="DailyTL"]').val(totalDailyTL);
    });

    // 新增一行訓練項目
    $(document).on('click', '.add-row', function () {
        var newRow = $(this).closest('.training-group').clone();
        // 清空所有輸入欄位和選擇框
        newRow.find('input').val('');
        newRow.find('select').prop('selectedIndex', 0); // 重設為預設選項
        newRow.find('input[name="RPE"]').attr('placeholder', '請選擇訓練感受');
        newRow.find('input[name="SessionTL"]').attr('placeholder', '每次運動訓練量(TL)');
        newRow.find('input[name="DailyTL"]').attr('placeholder', '每日運動訓練量(TL)');
        $('#trainingRows').append(newRow); // 將新行添加到表格中
    });

    // 刪除一行訓練項目
    $(document).on('click', '.remove-row', function () {
        var rowCount = $('#trainingRows .training-group').length;
        if (rowCount > 1) {
            $(this).closest('.training-group').remove();
        } else {
            alert("至少需要一個訓練項目。");
        }
    });
});