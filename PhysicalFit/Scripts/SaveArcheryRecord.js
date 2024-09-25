//SaveArcheryRecord.js
$(document).ready(function () {
    $('#btn-archeryMonitoring').click(function (event) {
        event.preventDefault(); // 防止表單的默認提交行為

        var athleteID = $('#AthletesID').val();
        var athleteName = $('#AthleteNameDisplay').text().trim();

        // 假設變數 isAthlete 用來判斷是否為運動員身份
        var isAthlete = '@ViewBag.UserRole' === 'Athlete';

        // 檢查當前使用者是否為教練
        /*var isCoach = $('#identityCoach').length > 0;*/ // 檢查教練身份區域是否存在
        // 假設有一個變數 isAthlete 來判斷使用者身份
        var isAthlete = $('#identity_Athletes').length > 0;

        // 如果使用者是教練，則檢查是否有選擇運動員
        if (!isAthlete && (!selectedAthlete || selectedAthlete === "請選擇")) {
            Swal.fire({
                icon: 'warning',
                title: '未選擇運動員',
                text: '請先選擇一位運動員才能存檔。',
            });
            return; // 終止後續動作
        }

        var coachName = $('#identityCoach #CoachName').text().trim();
        var coachID = $('#identityCoach #CoachID').val().trim();

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