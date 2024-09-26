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

        // 構建一個用於發送的數據對象
        var formData = {
            TrainingDate: $('input[name="archeryDate"]').val(), //訓練日
            Coach: coachName, //教練名字
            CoachID: coachID, //教練ID
            Athlete: athleteName, //運動員名字
            AthleteID: athleteID, //運動員ID
            Poundage: $('input[name="Pounds"]').val(), //磅數
            ArrowCount: $('input[name="Arrows"]').val(), //箭數
            RPEscore: $('input[name="RPEArchery"]').val(), //自覺程度
            EachTrainingLoad: $('input[name="SessionArcheryTL"]').val(), //單次運動負荷
            DailyTrainingLoad: $('input[name="ArcheryDailyTL"]').val()
        };
        debugger;
        // 根據身份選擇不同的 URL
        var url = isAthlete ? '/Record/SaveAthleteArcheryRecord' : '/PhyFit/SaveShootingRecord';

        $.ajax({
            type: 'POST',
            url: url,
            data: JSON.stringify(formData),
            contentType: 'application/json; charset=utf-8', // 設置請求內容類型為 JSON
            success: function (response) {
                if (response.success) {
                    alert('存檔成功');
                } else {
                    alert('存檔失敗! ');
                }
            },
            error: function (xhr, status, error) {
                alert('存檔失敗，請聯絡管理員 ' + error);
            }
        });
    });
});