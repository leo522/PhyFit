$(document).ready(function () {
    $('#btn-rainingMonitoring').click(function (event) {
        event.preventDefault(); // 防止表單的默認提交行為

        var specialTechnicalTrainingItem = $('#SpecialTechnicalTrainingItem').val(); //課程名稱

        // 檢查是否有選擇運動員
        var selectedAthlete = $('#AthletesID').find('option:selected').val();

        // 檢查當前使用者是否為教練
        var isCoach = $('#identityCoach').length > 0; // 檢查教練身份區域是否存在

        // 如果使用者是教練，則檢查是否有選擇運動員
        if (isCoach && (!selectedAthlete || selectedAthlete === "請選擇")) {
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
            Coach: coachName, //教練名字
            CoachID: coachID, //教練ID
            Athlete: $('#AthletesID option:selected').text(), //運動員名字
            AthleteID: $('#AthletesID').val(), //運動員ID
            TrainingClassName: specialTechnicalTrainingItem, //課程名稱
            TrainingDate: $('input[name="TrainingDate"]').val(), //訓練日期
            TrainingItem: $('select[name="SpecialTech"]').val(), //運動種類
            ActionName: $('select[name="SpecialTechName"]').val(), //動作項目
            TrainingParts: $('select[name="TrainingParts"]').val(), //訓練部位
            TrainingType: $('select[name="TrainingType"]').val(), //訓練類型
            TrainingOther: $('input[name="TrainingOther"]').val(), // 根據實際表單項目進行調整
            TrainingTime: $('select[name="TrainingTime"]').val(), //訓練時間
            RPEscore: $('input[name="RPE"]').val(), //自覺費力程度
            EachTrainingLoad: $('input[name="DailyTL"]').val(), //單次訓練負荷量
            DailyTrainingLoad: $('input[name="DailyTrainingLoad"]').val() //每次訓練負荷量
        };
        debugger;
        $.ajax({
            type: 'POST',
            url: '/PhyFit/SaveGeneralTrainingRecord',
            data: JSON.stringify(formData),
            contentType: 'application/json; charset=utf-8', // 設置請求內容類型為 JSON
            success: function (response) {
                if (response.success) {
                    alert('存檔成功');
                    // 如果需要可以在這裡做更多的處理，比如刷新頁面或重定向
                } else {
                    alert('存檔失敗: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                alert('存檔失敗: ' + error);
            }
        });
    });

    $('.openRecordModalGeneral').click(function () {
        $('#recordModalGeneral').fadeIn(); // 顯示 Modal

        $.ajax({
            type: 'GET',
            url: '/PhyFit/LoadGeneralRecordPartial',
            success: function (response) {
                $('#recordModalBody').html(response); // 將部分視圖內容加載到 Modal 中
            },
            error: function (xhr, status, error) {
                alert('獲取訓練紀錄失敗: ' + error);
            }
        });
    });

    $('#closeRecordModal').click(function () {
        $('#recordModalGeneral').fadeOut(); // 隱藏 Modal
    });

    $(window).click(function (event) {
        if ($(event.target).is('#recordModalGeneral')) {
            $('#recordModalGeneral').fadeOut(); // 隱藏 Modal
        }
    });
});