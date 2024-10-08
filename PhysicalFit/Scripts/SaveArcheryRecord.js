//SaveArcheryRecord.js
$(document).ready(function () {
    $('#btn-archeryMonitoring').click(function (event) {
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

        // 遍歷表格中的每一行資料，使用 .Archerytraining-group 來選擇
        $('#ArcherytrainingRows .Archerytraining-group').each(function () {
            var formData = {
                TrainingDate: $(this).find('input[name="archeryDate"]').val(), // 訓練日
                Coach: coachName, // 教練名字
                CoachID: coachID, // 教練ID
                Athlete: athleteName, // 運動員名字
                AthleteID: athleteID, // 運動員ID
                Poundage: $(this).find('input[name="Pounds"]').val(), // 磅數
                ArrowCount: $(this).find('input[name="Arrows"]').val(), // 箭數
                RPEscore: $(this).find('input[name="RPEArchery"]').val(), // 自覺程度
                EachTrainingLoad: $(this).find('input[name="SessionArcheryTL"]').val(), // 單次運動負荷
                DailyTrainingLoad: $(this).find('input[name="ArcheryDailyTL"]').val() // 每日運動負荷
            };
            records.push(formData); // 將每筆資料加入陣列中
        });

        // 檢查是否有資料
        if (records.length === 0) {
            alert('請先輸入至少一筆資料。');
            return;
        }

        // 根據身份選擇不同的 URL
        var url = isAthlete ? '/Record/SaveAthleteArcheryRecord' : '/PhyFit/SaveArcheryRecord';

        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(records), // 發送多筆資料
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (response.success) {
                    alert('存檔成功');
                } else {
                    alert('存檔失敗!');
                }
            },
            error: function (xhr, status, error) {
                alert('存檔失敗，請聯絡管理員 ' + error);
            }
        });
    });
});