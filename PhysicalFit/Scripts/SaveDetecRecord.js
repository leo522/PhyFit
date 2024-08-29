document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('btn_Detec').addEventListener('click', function (event) {
        event.preventDefault(); // 防止表單提交

        // 收集教練和運動員資料
        var coachName = document.getElementById('coachName').value;
        var athleteName = document.getElementById('AthletesName').value;
        var TrainingDate = document.getElementById('DetectionDate').value;
        // 收集數據
        var criticalSpeed = document.getElementById('CriticalSpeed').value;
        var anaerobicPower = document.getElementById('AnaerobicPower').value;
        var distances = [];
        var forceDurations = [];
        var speeds = [];

        document.querySelectorAll('#dataTable tr').forEach(function (row) {
            var distance = row.querySelector('td').innerText;
            var forceDuration = row.querySelector('.exhaustion-time').value;
            var speed = row.querySelector('.speed-result').value;


            // 如果 speed 元素的值是從 JavaScript 設定的，確認它是否已正確賦值
/*            var speed = speedElement.value || speedElement.innerText;*/

            if (distance && forceDuration && speed) {
                distances.push(distance);
                forceDurations.push(forceDuration);
                speeds.push(speed);
            }
        });
        // 發送 AJAX 請求
        $.ajax({
            url: '/PhyFit/SaveTrackFieldRecord',
            type: 'POST',
            data: JSON.stringify({
                criticalSpeed: criticalSpeed,
                anaerobicPower: anaerobicPower,
                distances: distances,
                forceDurations: forceDurations,
                speeds: speeds,
                coach: coachName,
                athlete: athleteName,
                trainingDate: TrainingDate,
            }),
            contentType: 'application/json',
            success: function (response) {
                alert('數據已儲存！');
                // 處理成功後的邏輯
            },
            error: function (xhr, status, error) {
                alert('儲存數據時出錯！');
                // 處理錯誤情況
            }
        });
    });
});