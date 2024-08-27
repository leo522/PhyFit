$(document).ready(function () {
    $('#btn-shootingMonitoring').click(function (event) {
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
            TrainingDate: $('input[name="shootingDate"]').val(),
            Coach: $('input[name="CoachName"]').val(),
            Athlete: $('#AthletesName').val(),
            ShootingTool: $('select[name="GunItem"]').val(),
            BulletCount: $('input[name="Bullet"]').val(),
            RPEscore: $('input[name="RPEshooting"]').val(),
            EachTrainingLoad: $('input[name="SessionShootingTL"]').val(),
            DailyTrainingLoad: $('input[name="ShootingDailyTL"]').val()
        };

        $.ajax({
            type: 'POST',
            url: '/PhyFit/SaveShootingRecord',
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
