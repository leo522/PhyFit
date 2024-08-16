$(document).ready(function () {
    // 確保按鈕存在後再添加事件監聽器
    $('#btn_result').click(function () {
        // 取得選擇的日期
        var year = $('#year').val();
        var month = $('#month').val();
        var day = $('#day').val();

        // 構造日期字串
        var formattedDate = `${year}-${month}-${day}`;


        // 發送 AJAX 請求到控制器
        $.ajax({
            url: '/PhyFit/CalculateTrainingLoad',
            type: 'GET',
            data: { date: formattedDate },
            dataType: 'json',
            success: function (data) {
                // 更新前端顯示
                $('#calculationResult').val(data.TrainingLoad || 'NA'); //運動訓練量
                $('#DailyResult').val(data.DailyTrainingLoadSum || 'NA'); //每日訓練量
                $('#WeeklyResult').val(data.WeeklyTrainingLoadSum || 'NA'); //每週訓練量
                $('#TrainingHomogeneity').val(data.TrainingMonotony || 'NA'); //訓練同質性
                $('#TensionValue').val(data.TrainingStrain || 'NA'); //訓練張力值
                $('#WeeklyTrainingChanges').val(data.WeekToWeekChange || 'NA'); //週間訓練變化
                $('#ShortLongRatio').val(data.ACWR || 'NA'); //短長期訓練量比值
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
            }
        });
    });
});