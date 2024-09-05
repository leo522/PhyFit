document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('btn_Detec').addEventListener('click', function (event) {
        event.preventDefault(); // 防止表單提交

        // 收集教練和運動員資料
        var coachName = document.getElementById('coachName').value; //教練名字
        var athleteName = document.getElementById('AthletesName').value; //運動員名字
        var TrainingDate = document.getElementById('DetectionDateTime').value; //訓練日期
        var deteItem = document.getElementById('DeteItem').value;

        // 防呆機制：確認運動員是否已選擇
        if (!athleteName) {
            alert('請選擇運動員！');
            return; // 阻止提交
        }

        if (!TrainingDate) {
            alert('請選擇訓練日期！');
            return; // 阻止提交
        }

        // 收集田徑場數據
        var criticalSpeed = document.getElementById('CriticalSpeed').value;
        var anaerobicPower = document.getElementById('AnaerobicPower').value;
        var distances = [];
        var forceDurations = [];
        var speeds = [];


        document.querySelectorAll('#dataTable tr').forEach(function (row) {
            var distance = row.querySelector('td').innerText;
            var forceDuration = row.querySelector('.exhaustion-time').value;
            var speed = row.querySelector('.speed-result').value;

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
                coach: coachName, //教練名字
                athlete: athleteName, //運動員名字
                detectionDate: TrainingDate, //訓練日期
                sportItem: deteItem, //運動項目
            }),
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    alert('資料已儲存！');
                } else {
                    alert('儲存失敗：' + response.message);
                }
            },
            error: function (xhr, status, error) {
                alert('資料儲存時出錯！');
                // 處理錯誤情況
            }
        });
    });
});