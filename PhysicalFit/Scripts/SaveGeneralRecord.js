$(document).ready(function () {
    $('#btn-rainingMonitoring').click(function (event) {
        event.preventDefault(); // 防止表單的默認提交行為

        var specialTechnicalTrainingItem = $('#SpecialTechnicalTrainingItem').val();
        // 檢查是否有選擇運動員
        var selectedAthlete = $('#AthletesName').find('option:selected').val();
        if (!selectedAthlete || selectedAthlete === "請選擇") {
            Swal.fire({
                icon: 'warning',
                title: '未選擇運動員',
                text: '請先選擇一位運動員才能存檔。',
            });
            return; // 終止後續動作
        }
        // 構建一個用於發送的數據對象
        var formData = {
            Coach: $('input[name="CoachName"]').val(),
            Athlete: $('#AthletesName').find('option:selected').text(),
            TrainingClassName: specialTechnicalTrainingItem,
            TrainingDate: $('input[name="TrainingDate"]').val(),
            TrainingItme: $('select[name="TrainingItme"]').val(),
            ActionName: $('select[name="ActionName"]').val(), // 根據實際表單項目進行調整
            TrainingParts: $('select[name="TrainingParts"]').val(), // 根據實際表單項目進行調整
            TrainingType: $('select[name="TrainingType"]').val(), // 根據實際表單項目進行調整
            TrainingOther: $('input[name="TrainingOther"]').val(), // 根據實際表單項目進行調整
            TrainingTime: $('select[name="TrainingTime"]').val(),
            RPEscore: $('input[name="RPEscore"]').val(), // 根據實際表單項目進行調整
            EachTrainingLoad: $('input[name="EachTrainingLoad"]').val(), // 根據實際表單項目進行調整
            DailyTrainingLoad: $('input[name="DailyTrainingLoad"]').val() // 根據實際表單項目進行調整
        };

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