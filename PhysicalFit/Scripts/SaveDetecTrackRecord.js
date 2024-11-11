// 田徑檢測
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('btn_Detec').addEventListener('click', function (event) {
        event.preventDefault(); // 防止表單提交

        // 收集教練和運動員資料
        var coachName = $('#identityCoach #CoachName').text().trim(); // 教練名字
        var coachID = $('#identityCoach #CoachID').val().trim(); // 教練ID

        var athleteID = $('#AthletesID').val() || $('input[name="AthleteID"]').val(); // 獲取運動員 ID
        var userRole = $('#userRole').val();
        var isAthlete = userRole === 'Athlete'; // 判斷是否為運動員

        var athleteName = isAthlete ?
            $('#athleteName').val().trim() :
            $('#AthletesID option:selected').text().trim();

        var selectedAthlete = $('#AthletesID option:selected').text().trim();

        var TrainingDate = document.getElementById('DetectionDateTime').value; // 訓練日期
        var deteItem = document.getElementById('DeteItem').value; // 運動項目

        if (!isAthlete && (!selectedAthlete || selectedAthlete === "請選擇")) {
            Swal.fire({
                icon: 'warning',
                title: '未選擇運動員',
                text: '請先選擇一位運動員才能存檔。',
            });
            return;
        }

        if (!TrainingDate) {
            Swal.fire({
                icon: 'warning',
                title: '訓練日期未選擇',
                text: '請選擇訓練日期！',
            });
            return; // 阻止提交
        }

        // 收集田徑場數據
        var criticalSpeed = document.getElementById('CriticalSpeed').value; // 臨界速度
        var anaerobicPower = document.getElementById('AnaerobicPower').value; // 最大無氧做功

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
            url: '/Record/SaveTrackFieldRecord',
            type: 'POST',
            data: JSON.stringify({
                criticalSpeed: criticalSpeed, // 臨界速度
                anaerobicPower: anaerobicPower, // 最大無氧做功
                distances: distances, // 距離
                forceDurations: forceDurations, // 力竭時間
                speeds: speeds, // 速度
                coach: coachName, // 教練名字
                athlete: athleteName, // 運動員名字
                detectionDate: TrainingDate, // 訓練日期
                sportItem: deteItem, // 運動項目
            }),
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: '成功',
                        text: '資料已儲存！',
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: '儲存失敗',
                        text: '儲存失敗：' + response.message,
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: '錯誤',
                    text: '資料儲存時出錯！',
                });
                // 處理錯誤情況
            }
        });
    });
});