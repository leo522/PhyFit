$(document).ready(function () {
    $('#btn-TrainingMonitoring').click(function (event) {
        event.preventDefault();

        const dateInputs = document.querySelectorAll('input[name="TrainingDate"]');
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

        $('#trainingMonitoringRows .training-group').each(function () {
            var formData = {
                Coach: coachName,
                CoachID: coachID,
                Athlete: athleteName,
                AthleteID: athleteID,
                TrainingClassName: $(this).find('select[name="SpecialTechnicalTrainingItem"]').val(),
                TrainingDate: $(this).find('input[name="TrainingDate"]').val(),
                TrainingItem: $(this).find('select[name="SpecialTech"]').val(),
                ActionName: $(this).find('select[name="SpecialTechName"]').val(),
                TrainingParts: $(this).find('select[name="TrainingMuscle"]').val(),
                TrainingType: $(this).find('select[name="TrainingType"]').val(),
                TrainingOther: $(this).find('input[name="SpecialTechOther"]').val(),
                TrainingTime: $(this).find('select[name="TrainingTime"]').val(),
                RPEscore: $(this).find('input[name="RPE"]').val(),
                EachTrainingLoad: $(this).find('input[name="SessionTL"]').val()
            };

            if (!formData.TrainingClassName || !formData.TrainingDate) {
                Swal.fire({
                    icon: 'warning',
                    title: '資料不完整',
                    text: '請完整填寫訓練資料。',
                    confirmButtonText: '確定'
                });
                return false;
            }

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

        var checkUrl = isAthlete
            ? '/Record/CheckDuplicateTrainingRecord'
            : '/PhyFit/CheckDuplicateGeneralTrainingRecord';

        var checkPromises = records.map(r => {
            return $.post(checkUrl, {
                athleteId: r.AthleteID,
                trainingDate: r.TrainingDate
            });
        });

        Promise.all(checkPromises).then(results => {
            const duplicatedTimes = [];

            // 先清除所有紅框
            document.querySelectorAll('input[name="TrainingDate"]').forEach(input =>
                input.classList.remove('border', 'border-danger')
            );

            results.forEach((res, idx) => {
                if (res.exists) {
                    const dt = new Date(records[idx].TrainingDate);
                    const formatted = dt.getFullYear() + '-' +
                        (dt.getMonth() + 1).toString().padStart(2, '0') + '-' +
                        dt.getDate().toString().padStart(2, '0') + ' ' +
                        dt.getHours().toString().padStart(2, '0') + ':' +
                        dt.getMinutes().toString().padStart(2, '0') + ':' +
                        dt.getSeconds().toString().padStart(2, '0');

                    duplicatedTimes.push(formatted);

                    // 找出對應欄位並加紅框
                    $(`#trainingMonitoringRows input[name="TrainingDate"]`).each(function () {
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
                        // 清除所有紅框
                        document.querySelectorAll('input[name="TrainingDate"]').forEach(input =>
                            input.classList.remove('border', 'border-danger')
                        );
                        sendRecords();
                    }
                });
            } else {
                // 清除所有紅框
                document.querySelectorAll('input[name="TrainingDate"]').forEach(input =>
                    input.classList.remove('border', 'border-danger')
                );
                sendRecords();
            }
        });

        function sendRecords() {
            var url = isAthlete
                ? '/Record/SaveAthleteTrainingRecord'
                : '/PhyFit/SaveGeneralTrainingRecord';

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
        }
    });
});