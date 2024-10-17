///*SaveShootingRecord.js 射擊*/
$(document).ready(function () {
    $('#btn-shootingMonitoring').click(function (event) {
        event.preventDefault(); // 防止表單的默認提交行為

        var athleteID = $('#AthletesID').val() || $('input[name="AthleteID"]').val(); // 獲取運動員 ID
        var userRole = $('#userRole').val();
        var isAthlete = userRole === 'Athlete'; // 判斷是否為運動員
        var coachName = $('#identityCoach #CoachName').text().trim();
        var coachID = $('#identityCoach #CoachID').val().trim();

        var athleteName = isAthlete ?
            $('#athleteName').val().trim() :
            $('#AthletesID option:selected').text().trim();

        var selectedAthlete = $('#AthletesID option:selected').text().trim();

        if (!isAthlete && (!selectedAthlete || selectedAthlete === "請選擇")) {
            Swal.fire({
                icon: 'warning',
                title: '未選擇運動員',
                text: '請先選擇一位運動員才能存檔。',
            });
            return;
        }

        // 構建多筆資料
        var records = [];

        // 遍歷所有的表格行，使用 class .Shootingtraining-group
        $('#ShootingtrainingRows .Shootingtraining-group').each(function () {
            var formData = {
                TrainingDate: $(this).find('input[name="shootingDate"]').val(), // 訓練日期
                Coach: coachName, // 教練名字
                CoachID: coachID, // 教練ID
                Athlete: athleteName, // 運動員名字
                AthleteID: athleteID, // 運動員ID
                ShootingTool: $(this).find('select[name="GunsItem"]').val(), // 訓練用具
                BulletCount: $(this).find('input[name="Bullet"]').val(), // 子彈數量
                RPEscore: $(this).find('input[name="RPEshooting"]').val(), // 自覺量表
                EachTrainingLoad: $(this).find('input[name="ShootingDailyTL"]').val() // 單次運動負荷量
            };
            records.push(formData); // 將每筆資料加入陣列中
        });

        // 檢查是否有資料
        if (records.length === 0) {
            Swal.fire({
                icon: 'warning',
                title: '無資料',
                text: '請先輸入至少一筆資料。',
                confirmButtonText: '確定'
            });
            return;
        }

        // 根據身份選擇不同的 URL
        var url = isAthlete ? '/Record/SaveAthleteShootingRecord' : '/PhyFit/SaveShootingRecord';

        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(records), // 發送多筆資料
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.success) {
                    Swal.fire({
                        icon: 'success',
                        title: '存檔成功',
                        text: '射擊訓練記錄已成功存檔。',
                        confirmButtonText: '確定'
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: '存檔失敗',
                        text: '存檔失敗: ' + response.message,
                        confirmButtonText: '確定'
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.fire({
                    icon: 'error',
                    title: '存檔失敗',
                    text: '存檔失敗，請聯絡管理員。錯誤資訊：' + error,
                    confirmButtonText: '確定'
                });
            }
        });
    });
});