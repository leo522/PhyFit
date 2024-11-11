// 跑步機檢測
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

        // 收集跑步機數據
        var criticalSpeed = document.getElementById('CriticalSpeed').value; // 臨界速度
        var anaerobicPower = document.getElementById('AnaerobicPower').value; // 最大無氧作功
        var percen = [];
        var forceDurations = [];
        var speeds = [];
        var maxSpeed = document.getElementById('MaxSpeed').value; // 最大跑速

        document.querySelectorAll('#dataTable tr').forEach(function (row) {
            var Intenpercen = row.querySelector('td').innerText; // 強度百分比
            var forceDuration = row.querySelector('.Tread-time').value; // 力竭時間
            var speed = row.querySelector('.tread-speed').innerText; // 速度

            if (Intenpercen && forceDuration && speed) {
                percen.push(Intenpercen);
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
                anaerobicPower: anaerobicPower, // 最大無氧作功
                maxRunningSpeed: maxSpeed, // 最大跑速
                intenpercen: percen, // 強度百分比
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