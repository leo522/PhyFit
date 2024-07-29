document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('btn_save').addEventListener('click', function () {
        // 獲取表單資料
        var trainingItem = document.getElementById('TrainingItemSession_RPE').value;
        var difficultyCategory = document.getElementById('Intensity').value; // 確保名稱一致
        var trainingActionName = document.getElementById('ActionName').value;
        var trainingTime = document.getElementById('TrainingTime').value;
        var rpeScore = parseInt(document.getElementById('RPE_1').value); // 確保名稱一致

        // 獲取選擇的日期
        var year = document.getElementById('year').value;
        var month = document.getElementById('month').value;
        var day = document.getElementById('day').value;

        // 確保選擇的日期有效
        if (!year || !month || !day) {
            alert('請選擇有效的日期');
            return;
        }

        var trainingDate = `${year}-${month.toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}T00:00:00`;

        var data = {
            TrainingItem: trainingItem,
            DifficultyCategory: difficultyCategory,
            TrainingActionName: trainingActionName,
            TrainingTime: trainingTime,
            RPEscore: rpeScore,
            TrainingDate: trainingDate
        };

        $.ajax({
            url: '/PhyFit/SaveTrainingRecord',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (result) {
                if (result.success) {
                    alert('儲存成功');
                    location.reload(); // 重新載入頁面
                } else {
                    alert('儲存失敗');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error('錯誤:', textStatus, errorThrown);
            }
        });
    });
});