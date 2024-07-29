//$(document).ready(function () {
//    localStorage.clear();

//    // 初始化當天的運動量和訓練記錄
//    var today = new Date();
//    var todayDate = today.toISOString().split('T')[0]; // 格式化為 YYYY-MM-DD
//    var dailyTotal = parseFloat(localStorage.getItem('dailyTotal')) || 0;
//    var trainingSessions = JSON.parse(localStorage.getItem('trainingSessions')) || [];
//    var savedDate = localStorage.getItem('savedDate');

//    // 設置每日運動量欄位為空
//    $('#DailyResult').val('');

//    // 檢查是否需要重置數據
//    if (savedDate !== todayDate) {
//        // 重置數據
//        localStorage.setItem('dailyTotal', 0);
//        localStorage.setItem('trainingSessions', JSON.stringify([]));
//        localStorage.setItem('savedDate', todayDate);
//        dailyTotal = 0;
//        $('#DailyResult').val(''); // 確保每日運動量欄位清空
//    } else {
//        // 顯示當前每日運動量
//        $('#DailyResult').val(dailyTotal);
//    }

//    // 計算每週的開始和結束日期
//    function getStartAndEndOfWeek(date) {
//        var startOfWeek = new Date(date);
//        startOfWeek.setDate(date.getDate() - date.getDay() + 1); // 本週一
//        var endOfWeek = new Date(date);
//        endOfWeek.setDate(startOfWeek.getDate() + 6); // 本週日
//        return {
//            startOfWeek: startOfWeek.toISOString().split('T')[0],
//            endOfWeek: endOfWeek.toISOString().split('T')[0]
//        };
//    }

//    var weekDates = getStartAndEndOfWeek(today);

//    function updateWeeklyTotal() {
//        var weeklyTotal = trainingSessions.reduce(function (total, session) {
//            var sessionDate = new Date(session.date).toISOString().split('T')[0];
//            if (sessionDate >= weekDates.startOfWeek && sessionDate <= weekDates.endOfWeek) {
//                return total + session.result;
//            }
//            return total;
//        }, 0);

//        $('#WeeklyResult').val(weeklyTotal);
//        localStorage.setItem('weeklyTotal', weeklyTotal);
//    }

//    $('#btn_result').click(function () {
//        // 獲取訓練時間和RPE分數
//        var trainingTimeText = $('#TrainingTime').val();
//        var rpeScore = $('#RPE_1').val();

//        // 處理訓練時間字串，只保留數字部分
//        var trainingTime = parseFloat(trainingTimeText.replace('小時', '').trim());

//        $.ajax({
//            url: '/PhyFit/CalculateRPE', // 修改為你的後端方法的正確路徑
//            type: 'POST',
//            data: {
//                trainingTime: trainingTime,
//                rpeScore: rpeScore
//            },
//            success: function (result) {
//                // 計算單次運動量
//                var singleSessionResult = trainingTime * rpeScore;

//                // 添加到當天的訓練次數列表
//                trainingSessions.push({
//                    time: trainingTime,
//                    score: rpeScore,
//                    result: singleSessionResult,
//                    date: todayDate
//                });

//                // 更新每日總運動量
//                dailyTotal += singleSessionResult;

//                // 儲存至 localStorage
//                localStorage.setItem('dailyTotal', dailyTotal);
//                localStorage.setItem('trainingSessions', JSON.stringify(trainingSessions));
//                localStorage.setItem('savedDate', todayDate); // 儲存今天的日期

//                // 更新顯示
//                $('#calculationResult').val(singleSessionResult); // 運動訓練量
//                $('#DailyResult').val(dailyTotal); // 每日運動訓練量

//                // 更新每週總運動量
//                updateWeeklyTotal();
//            }
//        });
//    });
//});
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