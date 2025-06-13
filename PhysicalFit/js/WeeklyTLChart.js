document.addEventListener('DOMContentLoaded', function () {
    window.weeklyTrainingLoadCharts = {};  // 存圖表物件

    function createWeeklyTLChart(canvasId, labels, seriesData, isCoach) {
        var ctx = document.getElementById(canvasId).getContext('2d');

        if (window.weeklyTrainingLoadCharts[canvasId]) {
            window.weeklyTrainingLoadCharts[canvasId].destroy();
        }

        const backgroundColors = isCoach
            ? ['#FFCC80', '#FFB74D', '#FFA726'] // 教練
            : ['#90CAF9', '#42A5F5', '#1E88E5']; // 學生

        const datasets = [];
        let colorIndex = 0;

        for (const item in seriesData) {
            const dataArray = seriesData[item];
            const hasData = dataArray.some(value => value !== 0 && value !== null && value !== undefined);

            if (hasData) {
                datasets.push({
                    label: item,
                    data: dataArray,
                    backgroundColor: backgroundColors[colorIndex % backgroundColors.length],
                    borderColor: backgroundColors[colorIndex % backgroundColors.length],
                    borderWidth: 1
                });
                colorIndex++;
            }
        }

        const allValues = datasets.flatMap(ds => ds.data);
        const maxY = Math.ceil(Math.max(allValues) * 1.1);

        window.weeklyTrainingLoadCharts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { position: 'top' },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        callbacks: {
                            footer: function (tooltipItems) {
                                if (tooltipItems.length <= 1) return '';

                                let sum = 0;
                                tooltipItems.forEach(item => {
                                    if (item.dataset && item.dataset.data && typeof item.raw === 'number') {
                                        sum += item.raw;
                                    }
                                });
                                return '總和: ' + sum;
                            }
                        },
                        titleFont: {
                            size: 18
                        },
                        bodyFont: {
                            size: 18
                        },
                        footerFont: {
                            size: 18
                        }
                    }
                },
                scales: {
                    x: { stacked: true, title: { display: true, text: '日期' } },
                    y: { stacked: false, min: 0, max: maxY, title: { display: true, text: '每週訓練量' } }
                }
            }
        });
    }

    // 篩選按鈕事件處理
    document.getElementById('filterChartsBtn')?.addEventListener('click', function () {
        var startDate = document.getElementById('startDate')?.value;
        var endDate = document.getElementById('endDate')?.value;
        var athleteId = document.getElementById('AthleteID')?.value || "";
        var userRole = document.getElementById('UserRole')?.value || "";
        var chartContainer = document.getElementById('WeeklyTLChartContainer');

        if (!startDate || !endDate) {
            Swal.fire('錯誤', '請選擇有效的起訖日期', 'error');
            return;
        }

        if (userRole === 'Coach') {
            // 兩組資料
            let coachDataAjax = $.get('/Record/GetWeeklyTrainingLoadData', {
                startDate, endDate, AthleteID: athleteId, isAthlete: false
            });
            let athleteDataAjax = $.get('/Record/GetWeeklyTrainingLoadData', {
                startDate, endDate, AthleteID: athleteId, isAthlete: true
            });

            $.when(coachDataAjax, athleteDataAjax).done(function (coachRes, athleteRes) {
                const coachData = coachRes[0];
                const athleteData = athleteRes[0];

                if (!coachData.error && !athleteData.error) {
                    chartContainer.style.display = "block";

                    createWeeklyTLChart('weeklyTrainingLoadChartCoach', coachData.dates, coachData.series, true);
                    createWeeklyTLChart('weeklyTrainingLoadChartAthlete', athleteData.dates, athleteData.series, false);
                }
            }).fail(function () {
                Swal.fire('錯誤', '無法載入資料', 'error');
            });

        } else {
            $.get('/Record/GetWeeklyTrainingLoadData', {
                startDate, endDate, AthleteID: athleteId, isAthlete: true
            }).done(function (data) {
                if (!data.error) {
                    chartContainer.style.display = "block";
                    createWeeklyTLChart('weeklyTrainingLoadChart', data.dates, data.series, false);
                }
            }).fail(function () {
                Swal.fire('錯誤', '無法載入資料', 'error');
            });
        }
    });
});
