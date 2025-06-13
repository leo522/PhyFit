document.addEventListener('DOMContentLoaded', function () {
    window.psychCharts = {};

    function createChart(canvasId, label, data, borderColor, maxY) {
        if (window.psychCharts[canvasId]) {
            window.psychCharts[canvasId].destroy(); // 避免圖重疊
        }

        var ctx = document.getElementById(canvasId).getContext('2d');
        window.psychCharts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.dates,
                datasets: [{
                    label: label,
                    data: data.scores,
                    borderColor: borderColor,
                    fill: false
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { position: 'top' },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                        titleFont: {
                            size: 18
                        },
                        bodyFont: {
                            size: 18
                        }
                    }
                },
                scales: {
                    x: { title: { display: true, text: '日期' } },
                    y: {
                        title: { display: true, text: '分數' },
                        min: 1,
                        max: maxY,
                        ticks: {
                            stepSize: 1,
                            callback: function (value) {
                                return Number.isInteger(value) ? value : value.toFixed(1);
                            }
                        }
                    }
                }
            }
        });
    }

    function initCharts(data) {
        createChart('sleepQualityChart', '睡眠品質', { dates: data.dates, scores: data.sleepQualityScores }, 'rgba(255, 99, 132, 1)', 5);
        createChart('fatigueChart', '疲憊程度', { dates: data.dates, scores: data.fatigueScores }, 'rgba(54, 162, 235, 1)', 5);
        createChart('trainingWillingnessChart', '訓練意願', { dates: data.dates, scores: data.trainingWillingnessScores }, 'rgba(75, 192, 192, 1)', 5);
        createChart('appetiteChart', '胃口', { dates: data.dates, scores: data.appetiteScores }, 'rgba(153, 102, 255, 1)', 5);
        createChart('competitionWillingnessChart', '比賽意願', { dates: data.dates, scores: data.competitionWillingnessScores }, 'rgba(255, 206, 86, 1)', 4);
    }

    // 初始載入資料
    var psychologicalDataExists = document.getElementById('psychologicalDataExists')?.value === "true";
    if (psychologicalDataExists) {
        var psychologicalData = {
            dates: JSON.parse(document.getElementById('psychologicalDates').value),
            sleepQualityScores: JSON.parse(document.getElementById('psychologicalSleepQualityScores').value),
            fatigueScores: JSON.parse(document.getElementById('psychologicalFatigueScores').value),
            trainingWillingnessScores: JSON.parse(document.getElementById('psychologicalTrainingWillingnessScores').value),
            appetiteScores: JSON.parse(document.getElementById('psychologicalAppetiteScores').value),
            competitionWillingnessScores: JSON.parse(document.getElementById('psychologicalCompetitionWillingnessScores').value),
        };
        initCharts(psychologicalData);
    }

    // 篩選區間按鈕
    document.getElementById('filterChartsBtn')?.addEventListener('click', function () {
        var startDate = document.getElementById('startDate')?.value;
        var endDate = document.getElementById('endDate')?.value;
        var athleteId = document.getElementById('AthleteID')?.value;
        var chartContainer = document.getElementById('chartsContainer');

        if (!startDate || !endDate || !athleteId) {
            Swal.fire('錯誤', '請輸入起訖日期', 'error');
            return;
        }
        
        $.ajax({
            url: '/Record/GetPsychologicalData',
            type: 'GET',
            data: {
                AthleteID: athleteId,
                startDate: startDate,
                endDate: endDate
            },
            success: function (data) {
                if (!data.error) {
                    chartContainer.style.display = "block";

                    initCharts({
                        dates: data.dates,
                        sleepQualityScores: data.sleepQualityScores,
                        fatigueScores: data.fatigueScores,
                        trainingWillingnessScores: data.trainingWillingnessScores,
                        appetiteScores: data.appetiteScores,
                        competitionWillingnessScores: data.competitionWillingnessScores
                    });
                } else {
                    Swal.fire('錯誤', data.error, 'error');
                }
            },
            error: function () {
                Swal.fire('錯誤', '無法載入心理特質資料', 'error');
            }
        });
    });
});
