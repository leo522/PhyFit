$(document).ready(function () {
    $('#btn_Detec').click(function () {

        var sportItem = $('#DeteItem').val();  // 選擇的運動項目

        var sportSpecificData = {}; //需要根據運動項目來收集相應的數據

        if (sportItem === "跑步機") {
            sportSpecificData = {
                Distance200: $('#dataTable tr').eq(0).find('.exhaustion-time').val(), // 第一行
                Distance400: $('#dataTable tr').eq(1).find('.exhaustion-time').val(), // 第二行
                Distance800: $('#dataTable tr').eq(2).find('.exhaustion-time').val(), // 第三行
                Distance1200: $('#dataTable tr').eq(3).find('.exhaustion-time').val(), // 第四行
                CoefficientOfDetermination: $('#calculationResult').val() // 計算結果
            };
        } else if (sportItem === "田徑場") {
            var distanceData = {};
            $('#dataTable tr').each(function () {
                var distance = $(this).find('td').first().text().trim(); // 距離
                var exhaustionTime = $(this).find('.exhaustion-time').val(); // 力竭時間
                if (distance) {
                    distanceData[`Distance${distance}`] = exhaustionTime;
                }
            });
            sportSpecificData = {
                Distance200: distanceData['Distance200'] || '',
                Distance400: distanceData['Distance400'] || '',
                Distance800: distanceData['Distance800'] || '',
                Distance1200: distanceData['Distance1200'] || '',
                CoefficientOfDetermination: $('#calculationResult').val()
            };


        } else if (sportItem === "游泳") {
            sportSpecificData = {
                Distance100: $('#IPercen95').val(),
                Distance200: $('#IPercen90').val(),
                Distance400: $('#IPercen85').val(),
                Distance800: $('#IPercen80').val(),
                CoefficientOfDetermination: $('#calculationResult').val()
            };
        } else if (sportItem === "自由車") {
            sportSpecificData = {
                MaxPower: $('#MaxPower').val(),
                IPercen95: $('#IPercen95').val(),
                IPercen90: $('#IPercen90').val(),
                IPercen85: $('#IPercen85').val(),
                IPercen80: $('#IPercen80').val(),
                CoefficientOfDetermination: $('#calculationResult').val()
            };
        } else if (sportItem === "滑輪溜冰") {
            sportSpecificData = {
                Distance200: $('#IPercen95').val(),
                Distance500: $('#IPercen90').val(),
                Distance1000: $('#IPercen85').val(),
                Distance2000: $('#IPercen80').val(),
                CoefficientOfDetermination: $('#calculationResult').val()
            };
        }

        var detectionData = {
            // 填寫 DetectionTrainingRecord 表需要的數據
            CriticalSpeed: $('#CriticalSpeed').val(),
            AnaerobicPower: $('#AnaerobicPower').val(),
            TrainingVol: $('#TrainingVol').val(),
            TrainingPrescription: $('#TrainingPrescription').val(),
            // 添加其他字段...
        };

        $.ajax({
            type: "POST",
            url: '/PhyFit/SaveDetecRecord',
            data: JSON.stringify({
                sportItem: sportItem,
                record: detectionData,
                sportSpecificData: sportSpecificData
            }),
            contentType: 'application/json',
            success: function (response) {
                if (response.success) {
                    alert('存檔成功！');
                } else {
                    alert('存檔失敗：' + response.error);
                }
            },
            error: function (xhr, status, error) {
                alert('請求失敗: ' + error);
            }
        });
    });
});