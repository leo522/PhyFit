document.addEventListener('DOMContentLoaded', function () {
    // 獲取 DOM 元素
    var trainingTimeSelect = document.getElementById('TrainingTime');
    var rpeScoreSelect = document.getElementById('RPE_1');
    var calculationResultInput = document.getElementById('calculationResult');

    // 檢查元素是否存在
    if (trainingTimeSelect && rpeScoreSelect && calculationResultInput) {
        // 當運動時間或 RPE 分數改變時計算結果
        function calculateResult() {
            var exerciseTime = parseFloat(trainingTimeSelect.value) || 0;
            var rpeScore = parseFloat(rpeScoreSelect.value) || 0;
            var result = exerciseTime * rpeScore;
            calculationResultInput.value = result.toFixed(2); // 計算結果顯示到小數點後兩位
        }

        // 當用戶選擇時觸發計算
        rpeScoreSelect.addEventListener('change', calculateResult);
        trainingTimeSelect.addEventListener('change', calculateResult);
    } else {
        console.error("資料錯誤");
    }
});