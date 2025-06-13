document.addEventListener('DOMContentLoaded', function () {
    window.dailyTrainingLoadCharts = {};  // 存圖表物件

    function createDailyTLChart(canvasId, labels, seriesData, isCoach) {
        var ctx = document.getElementById(canvasId).getContext('2d');

        if (window.dailyTrainingLoadCharts[canvasId]) {
            window.dailyTrainingLoadCharts[canvasId].destroy();
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
                let chartType = 'bar';
                let yAxisID = 'y'; // 使用左側
                let borderColor = backgroundColors[colorIndex % backgroundColors.length];
                let backgroundColor = borderColor;

                if (item.includes("張力值")) {
                    chartType = 'line';
                    yAxisID = 'y';
                    borderColor = 'rgba(255, 99, 132, 1)';
                    backgroundColor = 'rgba(255, 159, 64, 0.2)';
                } else if (item.includes("同質性")) {
                    chartType = 'line';
                    yAxisID = 'y1'; // 右側 Y 軸
                    borderColor = 'rgba(153, 102, 255, 1)';
                    backgroundColor = 'rgba(153, 102, 255, 0.2)';
                }

                datasets.push({
                    label: item,
                    data: dataArray,
                    type: chartType,
                    yAxisID: yAxisID,
                    backgroundColor: backgroundColor,
                    borderColor: borderColor,
                    borderWidth: 2,
                    fill: false,
                    tension: 0.4
                });

                if (chartType === 'bar') {
                    colorIndex++;
                }
            }
        }

        const allValues = datasets.flatMap(ds => ds.data);
        const maxY = Math.ceil(Math.max(allValues) * 1.1);

        window.dailyTrainingLoadCharts[canvasId] = new Chart(ctx, {
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
                                // 過濾掉 TM 和 TS 的 tooltip
                                const filteredItems = tooltipItems.filter(item => {
                                    const label = item.dataset?.label || '';
                                    return !label.includes('同質性') && !label.includes('張力值');
                                });

                                if (filteredItems.length <= 1) return '';

                                let sum = 0;
                                filteredItems.forEach(item => {
                                    if (typeof item.raw === 'number') {
                                        sum += item.raw;
                                    }
                                });

                                return '訓練衝量總和: ' + sum;
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
                    x: {
                        stacked: true,
                        title: { display: true, text: '日期' }
                    },
                    y: {
                        type: 'linear',
                        position: 'left',
                        stacked: false,
                        min: 0,
                        max: maxY,
                        title: { display: true, text: '每日訓練量 / 張力值' }
                    },
                    y1: {
                        type: 'linear',
                        position: 'right',
                        beginAtZero: true,
                        grid: { drawOnChartArea: false },
                        title: { display: true, text: '訓練同質性' }
                    }
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
        var chartContainer = document.getElementById('TLChartContainer');

        if (!startDate || !endDate) {
            Swal.fire('錯誤', '請選擇有效的起訖日期', 'error');
            return;
        }

        if (userRole === 'Coach') {
            // 兩組資料
            let coachDataAjax = $.get('/Record/GetDailyTrainingLoadData', {
                startDate, endDate, AthleteID: athleteId, isAthlete: false
            });
            let athleteDataAjax = $.get('/Record/GetDailyTrainingLoadData', {
                startDate, endDate, AthleteID: athleteId, isAthlete: true
            });

            $.when(coachDataAjax, athleteDataAjax).done(function (coachRes, athleteRes) {
                const coachData = coachRes[0];
                const athleteData = athleteRes[0];

                if (!coachData.error && !athleteData.error) {
                    chartContainer.style.display = "block";

                    createDailyTLChart('dailyTrainingLoadChartCoach', coachData.dates, coachData.series, true);
                    createDailyTLChart('dailyTrainingLoadChartAthlete', athleteData.dates, athleteData.series, false);
                }
            }).fail(function () {
                Swal.fire('錯誤', '無法載入資料', 'error');
            });

        } else {
            $.get('/Record/GetDailyTrainingLoadData', {
                startDate, endDate, AthleteID: athleteId, isAthlete: true
            }).done(function (data) {
                if (!data.error) {
                    chartContainer.style.display = "block";
                    createDailyTLChart('dailyTrainingLoadChart', data.dates, data.series, false);
                }
            }).fail(function () {
                Swal.fire('錯誤', '無法載入資料', 'error');
            });
        }
    });
});
