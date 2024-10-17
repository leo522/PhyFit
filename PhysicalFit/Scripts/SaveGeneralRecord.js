///*一般訓練紀錄*/
$(document).ready(function () {
    $('#btn-TrainingMonitoring').click(function (event) {
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

        var records = [];
        // 遍歷每一行訓練資料行，使用 class .training-group
        $('#trainingMonitoringRows .training-group').each(function () {
            var formData = {
                Coach: coachName, //教練名字
                CoachID: coachID, //教練ID
                Athlete: athleteName, //運動員名字
                AthleteID: athleteID, //運動員ID
                TrainingClassName: $(this).find('select[name="SpecialTechnicalTrainingItem"]').val(), // 課程名稱
                TrainingDate: $(this).find('input[name="TrainingDate"]').val(), // 訓練日期
                TrainingItem: $(this).find('select[name="SpecialTech"]').val(), // 運動種類
                ActionName: $(this).find('select[name="SpecialTechName"]').val(), // 動作項目
                TrainingParts: $(this).find('select[name="TrainingMuscle"]').val(), // 訓練部位
                TrainingType: $(this).find('select[name="TrainingType"]').val(), // 訓練類型
                TrainingOther: $(this).find('input[name="SpecialTechOther"]').val(), // 其他訓練項目
                TrainingTime: $(this).find('select[name="TrainingTime"]').val(), // 訓練時間
                RPEscore: $(this).find('input[name="RPE"]').val(), // 自覺費力程度
                EachTrainingLoad: $(this).find('input[name="DailyTL"]').val() // 每日運動負荷
            };

            // 檢查表單資料是否齊全
            if (!formData.TrainingClassName || !formData.TrainingDate) {
                Swal.fire({
                    icon: 'warning',
                    title: '資料不完整',
                    text: '請完整填寫訓練資料。',
                    confirmButtonText: '確定'
                });
                return false; // 終止迴圈
            }

            records.push(formData); // 將每筆資料加入陣列中
        });

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
        var url = isAthlete ? '/Record/SaveAthleteTrainingRecord' : '/PhyFit/SaveGeneralTrainingRecord';

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
                        text: '訓練紀錄已成功存檔。',
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