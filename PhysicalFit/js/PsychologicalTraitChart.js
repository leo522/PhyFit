/*PsychologicalTraitChart.js*/

document.addEventListener('DOMContentLoaded', function () {
    function createChart(canvasId, label, data, borderColor) {
        var ctx = document.getElementById(canvasId).getContext('2d');
        new Chart(ctx, {
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
                scales: {
                    x: { title: { display: true, text: '日期' } },
                    y: { title: { display: true, text: '分數' }, min: 0, max: 10 }
                }
            }
        });
    }

    function initCharts(data) {
        createChart('sleepQualityChart', '睡眠品質', { dates: data.dates, scores: data.sleepQualityScores }, 'rgba(255, 99, 132, 1)');
        createChart('fatigueChart', '疲憊程度', { dates: data.dates, scores: data.fatigueScores }, 'rgba(54, 162, 235, 1)');
        createChart('trainingWillingnessChart', '訓練意願', { dates: data.dates, scores: data.trainingWillingnessScores }, 'rgba(75, 192, 192, 1)');
        createChart('appetiteChart', '胃口', { dates: data.dates, scores: data.appetiteScores }, 'rgba(153, 102, 255, 1)');
        createChart('competitionWillingnessChart', '比賽意願', { dates: data.dates, scores: data.competitionWillingnessScores }, 'rgba(255, 206, 86, 1)');
    }

    // 假設你會從伺服器端傳遞心理數據
    var psychologicalDataExists = document.getElementById('psychologicalDataExists').value === "true";

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
});