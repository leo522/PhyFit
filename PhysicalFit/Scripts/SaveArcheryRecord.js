$(document).ready(function () {
    $('#btn-archeryMonitoring').click(function (event) {
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
            TrainingDate: $('input[name="archeryDate"]').val(),
            Coach: $('input[name="CoachName"]').val(),
            Athlete: $('#AthletesName').val(),
            Poundage: $('input[name="Pounds"]').val(),
            ArrowCount: $('input[name="Arrows"]').val(),
            RPEscore: $('input[name="RPEArchery"]').val(),
            EachTrainingLoad: $('input[name="SessionArcheryTL"]').val(),
            DailyTrainingLoad: $('input[name="ArcheryDailyTL"]').val()
        };

        $.ajax({
            type: 'POST',
            url: '/PhyFit/SaveArcheryRecord',
            data: JSON.stringify(formData),
            contentType: 'application/json; charset=utf-8', // 設置請求內容類型為 JSON
            success: function (response) {
                if (response.success) {
                    alert('存檔成功');
                } else {
                    alert('存檔失敗: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                alert('存檔失敗: ' + error);
            }
        });
    });
});