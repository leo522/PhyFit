document.addEventListener('DOMContentLoaded', function () {
    const selector = document.getElementById('sportItemSelector');
    const detectChartDiv = document.querySelector('.detectChart');
    const detectChartCanvas = document.getElementById('detectChartCanvas');
    const detectChartBody = document.querySelector('.detectChartBody');
    const detectTable = document.getElementById('detectBlock');
    let chartInstance = null;
    const athleteId = $('#AthleteID').val();

    if (selector) {
        selector.addEventListener('change', function () {
            const sportItem = this.value;

            if (sportItem && sportItem !== '請選擇') {
                detectChartDiv.style.display = 'block';
                detectChartCanvas.style.display = 'block';
                detectChartBody.style.display = 'block';
                detectTable.style.display = 'block';

                if (chartInstance) chartInstance.destroy();

                $.ajax({
                    url: '/Record/GetDetectionRegressionData',
                    method: 'GET',
                    data: {
                        athleteId: athleteId,
                        sportItem: sportItem
                    },
                    success: function (records) {
                        const datasets = [];

                        records.forEach(function (record) {
                            const xs = record.dataPoints.map(p => p.x);
                            const ys = record.dataPoints.map(p => p.y);
                            const result = calculateBestRegression(xs, ys);

                            if (result && result.regressionLine) {
                                const color = getDistinctColor(datasets.length); // 固定顏色供兩筆 dataset 使用
                                const sharedLabel = `${record.date} (R²: ${result.r2})`;
                                const groupId = `${record.date}_${record.id}`; // 唯一 groupId 用來分群 toggle

                                // 回歸線
                                datasets.push({
                                    label: sharedLabel,
                                    groupId: groupId,
                                    data: result.regressionLine,
                                    borderColor: color,
                                    borderWidth: 2,
                                    borderDash: [5, 5],
                                    fill: false,
                                    tension: 0,
                                    pointRadius: 0,
                                    showLine: true,
                                    customDate: record.date
                                });

                                // 三個資料點
                                datasets.push({
                                    label: sharedLabel,
                                    groupId: groupId,
                                    data: result.points,
                                    borderColor: color,
                                    backgroundColor: color,
                                    showLine: false,
                                    pointRadius: 5,
                                    type: 'scatter',
                                    customDate: record.date
                                });
                            }
                        });

                        chartInstance = new Chart(detectChartCanvas.getContext('2d'), {
                            type: 'line',
                            data: { datasets: datasets },
                            options: {
                                responsive: true,
                                scales: {
                                    x: {
                                        title: { display: true, text: '時間 (秒)' },
                                        type: 'linear'
                                    },
                                    y: {
                                        title: { display: true, text: '距離 (公尺)' }
                                    }
                                },
                                plugins: {
                                    legend: {
                                        display: true,
                                        labels: {
                                            generateLabels: function (chart) {
                                                const seen = new Set();
                                                const datasets = chart.data.datasets;

                                                return datasets
                                                    .map((ds, i) => ({
                                                        text: ds.label,
                                                        fillStyle: ds.borderColor,
                                                        strokeStyle: ds.borderColor,
                                                        hidden: !chart.isDatasetVisible(i),
                                                        datasetIndex: i,
                                                        groupId: ds.groupId
                                                    }))
                                                    .filter(item => {
                                                        if (seen.has(item.groupId)) return false;
                                                        seen.add(item.groupId);
                                                        return true;
                                                    });
                                            }
                                        },
                                        onClick: function (e, legendItem, legend) {
                                            const chart = legend.chart;
                                            const datasets = chart.data.datasets;
                                            const clickedGroupId = datasets[legendItem.datasetIndex].groupId;

                                            // 檢查該 groupId 當前是否顯示
                                            const anyVisible = datasets.some((ds, i) =>
                                                ds.groupId === clickedGroupId && chart.isDatasetVisible(i)
                                            );

                                            // 將該 groupId 所有 dataset 的 hidden 狀態統一設為「反向」
                                            datasets.forEach((ds, i) => {
                                                if (ds.groupId === clickedGroupId) {
                                                    chart.getDatasetMeta(i).hidden = anyVisible;
                                                }
                                            });

                                            chart.update();
                                        }
                                    },
                                    tooltip: {
                                        callbacks: {
                                            label: function (context) {
                                                const x = context.raw.x.toFixed(2);
                                                const y = context.raw.y.toFixed(2);
                                                const date = context.dataset.customDate || '';
                                                return `${date} (x: ${x}, y: ${y})`;
                                            }
                                        }
                                    }
                                }
                            }
                        });
                    },
                    error: function () {
                        alert('取得檢測資料失敗');
                    }
                });
            } else {
                detectChartDiv.style.display = 'none';
                detectChartCanvas.style.display = 'none';
                detectChartBody.style.display = 'none';
                if (chartInstance) chartInstance.destroy();
            }
        });
    }

    const distinctColors = [
        '#e6194b', '#3cb44b', '#ffe119', '#4363d8', '#f58231',
        '#911eb4', '#46f0f0', '#f032e6', '#bcf60c', '#fabebe',
        '#008080', '#e6beff'
    ];

    function getDistinctColor(index) {
        return distinctColors[index % distinctColors.length];
    }

    function calculateBestRegression(xs, ys) {
        if (xs.length < 3 || ys.length < 3) return null;

        let maxR2 = 0;
        let bestPoints = [];
        let bestA = 0, bestB = 0;

        for (let i = 0; i < xs.length - 2; i++) {
            for (let j = i + 1; j < xs.length - 1; j++) {
                for (let k = j + 1; k < xs.length; k++) {
                    const X = [xs[i], xs[j], xs[k]];
                    const Y = [ys[i], ys[j], ys[k]];
                    const avgX = X.reduce((a, b) => a + b, 0) / 3;
                    const avgY = Y.reduce((a, b) => a + b, 0) / 3;

                    let numerator = 0, denominator = 0;
                    for (let m = 0; m < 3; m++) {
                        numerator += (X[m] - avgX) * (Y[m] - avgY);
                        denominator += (X[m] - avgX) ** 2;
                    }

                    const b = numerator / denominator;
                    const a = avgY - b * avgX;

                    const yDistanceWithBar = Y.reduce((sum, y) => sum + (y - avgY) ** 2, 0);
                    const yDistanceWithLine = Y.reduce((sum, y, idx) => sum + (y - (b * X[idx] + a)) ** 2, 0);
                    const r2 = 1 - yDistanceWithLine / yDistanceWithBar;

                    if (r2 > maxR2) {
                        maxR2 = r2;
                        bestPoints = X.map((x, idx) => ({ x: x, y: Y[idx] }));
                        bestA = a;
                        bestB = b;
                    }
                }
            }
        }

        if (bestPoints.length === 0) return null;

        const minX = Math.min(...bestPoints.map(p => p.x));
        const maxX = Math.max(...bestPoints.map(p => p.x));
        return {
            points: bestPoints,
            regressionLine: [
                { x: minX, y: bestA + bestB * minX },
                { x: maxX, y: bestA + bestB * maxX }
            ],
            r2: maxR2.toFixed(4)
        };
    }
});
