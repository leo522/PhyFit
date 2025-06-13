$(document).ready(function () {
    $('#btn-shootingMonitoring').click(function (event) {
        event.preventDefault();

        const dateInputs = document.querySelectorAll('input[name="shootingDate"]');
        const dateMap = new Map();
        let hasDuplicate = false;
        let duplicateValue = "";

        // 清除所有紅框
        dateInputs.forEach(input => input.classList.remove('border', 'border-danger'));

        for (let input of dateInputs) {
            const rawValue = input.value.trim();
            if (!rawValue) continue;

            const timestamp = new Date(rawValue).getTime();

            if (dateMap.has(timestamp)) {
                hasDuplicate = true;
                duplicateValue = rawValue;

                // 加紅框給重複的兩筆欄位
                input.classList.add('border', 'border-danger');
                dateMap.get(timestamp).classList.add('border', 'border-danger');
            } else {
                dateMap.set(timestamp, input);
            }
        }

        if (hasDuplicate) {
            Swal.fire({
                icon: 'warning',
                title: '日期重複',
                text: `有重複的日期時間：${duplicateValue}，請檢查再送出。`,
            });
            return;
        }

        var athleteID = $('#AthletesID').val() || $('input[name="AthleteID"]').val();
        var userRole = $('#userRole').val();
        var isAthlete = userRole === 'Athlete';
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

        $('#ShootingtrainingRows .Shootingtraining-group').each(function () {
            var formData = {
                TrainingDate: $(this).find('input[name="shootingDate"]').val(),
                Coach: coachName,
                CoachID: coachID,
                Athlete: athleteName,
                AthleteID: athleteID,
                ShootingTool: $(this).find('select[name="GunsItem"]').val(),
                BulletCount: $(this).find('input[name="Bullet"]').val(),
                RPEscore: $(this).find('input[name="RPEshooting"]').val(),
                EachTrainingLoad: $(this).find('input[name="SessionShootingTL"]').val()
            };
            records.push(formData);
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

        const checkUrl = isAthlete
            ? '/Record/CheckDuplicateShootingRecord'
            : '/PhyFit/CheckDuplicateCoachShootingRecord';

        const checkPromises = records.map(r => {
            const dt = new Date(r.TrainingDate);
            const formatted = `${dt.getFullYear()}-${(dt.getMonth() + 1).toString().padStart(2, '0')}-${dt.getDate().toString().padStart(2, '0')}T${dt.getHours().toString().padStart(2, '0')}:${dt.getMinutes().toString().padStart(2, '0')}:${dt.getSeconds().toString().padStart(2, '0')}`;
            return $.post(checkUrl, {
                athleteId: r.AthleteID,
                trainingDate: formatted
            });
        });

        Promise.all(checkPromises).then(results => {
            const duplicatedTimes = [];

            document.querySelectorAll('input[name="shootingDate"]').forEach(input =>
                input.classList.remove('border', 'border-danger')
            );

            results.forEach((res, idx) => {
                if (res.exists) {
                    const dt = new Date(records[idx].TrainingDate);
                    const formatted = `${dt.getFullYear()}-${(dt.getMonth() + 1).toString().padStart(2, '0')}-${dt.getDate().toString().padStart(2, '0')} ${dt.getHours().toString().padStart(2, '0')}:${dt.getMinutes().toString().padStart(2, '0')}:${dt.getSeconds().toString().padStart(2, '0')}`;

                    duplicatedTimes.push(formatted);

                    $('#ShootingtrainingRows input[name="shootingDate"]').each(function () {
                        if (this.value && new Date(this.value).getTime() === dt.getTime()) {
                            this.classList.add('border', 'border-danger');
                        }
                    });
                }
            });

            if (duplicatedTimes.length > 0) {
                Swal.fire({
                    title: '重複時間確認',
                    html: `以下時間已有紀錄：<br><br><strong>${duplicatedTimes.join('<br>')}</strong><br><br>是否要覆蓋？`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: '確定覆蓋',
                    cancelButtonText: '取消'
                }).then((result) => {
                    if (result.isConfirmed) {
                        document.querySelectorAll('input[name="shootingDate"]').forEach(input =>
                            input.classList.remove('border', 'border-danger')
                        );
                        sendShootingRecords();
                    }
                });
            } else {
                document.querySelectorAll('input[name="shootingDate"]').forEach(input =>
                    input.classList.remove('border', 'border-danger')
                );
                sendShootingRecords();
            }
        });

        function sendShootingRecords() {
            var url = isAthlete
                ? '/Record/SaveAthleteShootingRecord'
                : '/PhyFit/SaveShootingRecord';

            $.ajax({
                type: 'POST',
                url: url,
                data: JSON.stringify(records),
                contentType: 'application/json; charset=utf-8',
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: '存檔成功',
                            text: '射擊訓練紀錄已成功存檔。',
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
        }
    });
});