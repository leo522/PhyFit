document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('btn_save').addEventListener('click', function () {
        // 獲取表單資料
        var trainingItem = document.getElementById('TrainingItemSession_RPE').value;
        var intensity = document.getElementById('Intensity').value;
        var actionName = document.getElementById('ActionName').value;
        var trainingTime = document.getElementById('TrainingTime').value;
        var rpe = document.getElementById('RPE_1').value;

        // 獲取選擇的日期
        var year = document.getElementById('year').value;
        var month = document.getElementById('month').value;
        var day = document.getElementById('day').value;

        // 確保選擇的日期有效
        if (!year || !month || !day) {
            alert('請選擇有效的日期');
            return;
        }

        var data = {
            TrainingItem: trainingItem,
            Intensity: intensity,
            ActionName: actionName,
            Year: parseInt(year),
            Month: parseInt(month),
            Day: parseInt(day),
            TrainingTime: trainingTime,
            RPE: parseInt(rpe),
        };

        fetch('/PhyFit/SaveTrainingRecord', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    alert('儲存成功');
                    // 可以選擇重新載入資料或顯示資料
                    location.reload(); // 重新載入頁面
                } else {
                    alert('儲存失敗');
                }
            })
            .catch(error => console.error('錯誤:', error));
    });
});
