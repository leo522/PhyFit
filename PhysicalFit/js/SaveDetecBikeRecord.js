document.addEventListener('DOMContentLoaded', function () {
    const sportSelector = document.getElementById('DeteItem');

    sportSelector.addEventListener('change', function () {
        const selected = this.value;
        document.getElementById('selectedSportItem').textContent = selected;

        if (selected === '自由車') {
            document.getElementById('btn_Detec').addEventListener('click', function (event) {
                event.preventDefault();

                var coachName = $('#identityCoach #CoachName').text().trim();
                var coachID = $('#identityCoach #CoachID').val().trim();

                var athleteID = $('#AthletesID').val() || $('input[name="AthleteID"]').val();
                var userRole = $('#userRole').val();
                var isAthlete = userRole === 'Athlete';

                var athleteName = isAthlete ?
                    $('#athleteName').val().trim() :
                    $('#AthletesID option:selected').text().trim();

                var selectedAthlete = $('#AthletesID option:selected').text().trim();

                var TrainingDate = document.getElementById('DetectionDateTime').value;
                var deteItem = document.getElementById('DeteItem').value;

                if (!isAthlete && (!selectedAthlete || selectedAthlete === "請選擇")) {
                    Swal.fire({
                        icon: 'warning',
                        title: '未選擇運動員',
                        text: '請先選擇一位運動員才能存檔。',
                    });
                    return;
                }

                if (!TrainingDate) {
                    Swal.fire({
                        icon: 'warning',
                        title: '訓練日期未選擇',
                        text: '請選擇訓練日期！',
                    });
                    return;
                }

                var criticalSpeed = document.getElementById('CriticalSpeed').value;
                var anaerobicPower = document.getElementById('AnaerobicPower').value;
                var percen = [];
                var forceDurations = [];
                var speeds = [];
                var maxPower = document.getElementById('MaxSpeed').value;

                document.querySelectorAll('#dataTable tr').forEach(function (row) {
                    var Intenpercen = row.querySelector('td').innerText;
                    var forceDuration = row.querySelector('.bike-time').value;
                    var speed = row.querySelector('.bike-speed').innerText;

                    if (Intenpercen && forceDuration && speed) {
                        percen.push(Intenpercen);
                        forceDurations.push(forceDuration);
                        speeds.push(speed);
                    }
                });

                $.ajax({
                    url: '/Record/SaveTrackFieldRecord',
                    type: 'POST',
                    data: JSON.stringify({
                        criticalSpeed: criticalSpeed,
                        anaerobicPower: anaerobicPower,
                        Maxpower: maxPower,
                        intenpercen: percen,
                        forceDurations: forceDurations,
                        speeds: speeds,
                        coach: coachName,
                        athlete: athleteName,
                        detectionDate: TrainingDate,
                        sportItem: deteItem,
                    }),
                    contentType: 'application/json',
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: '成功',
                                text: '資料已儲存！',
                            });
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: '儲存失敗',
                                text: '儲存失敗：' + response.message,
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        Swal.fire({
                            icon: 'error',
                            title: '錯誤',
                            text: '資料儲存時出錯！',
                        });
                    }
                });
            });
        }
    });
});