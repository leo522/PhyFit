$(document).ready(function () {
    initializeModal("RPEModalTraining", "openRPEModal");
    initializeModal("RPEModalArchery", "openRPEModalArchery");
    initializeModal("RPEModalShooting", "openRPEModalShooting");

    $('#Item').change(function () {
        var selectedItem = $(this).val();
        $('.content').hide();
        if (selectedItem === '一般訓練衝量監控 (session-RPE)') {
            $('#trainingMonitoring').show();
        }
        else if (selectedItem === '射箭訓練衝量') {
            $('#archeryMonitoring').show();
        }
        else if (selectedItem === '射擊訓練衝量') {
            $('#shootingMonitoring').show();
        }
        else if (selectedItem === '檢測系統') {
            $('#distanceDetails').show();
        }
        else if (selectedItem === '心理特質與食慾圖量表') {
            $('#PsychologicalTraitsArea').show();
        }
    });

    $('#Item').on('change', function () {
        $('.modal').hide();
        $('.modal').removeData('currentRow');
    });

    $('#DeteItem').change(function () {

        var selectedItem = $(this).val();

        $('#selectedSportItem').text(selectedItem);
        $('#btn_result_container').hide();

        if (selectedItem !== "請選擇運動項目") {
            $.ajax({
                url: '/PhyFit/LoadDistanceDetails',
                data: { itemName: selectedItem },
                success: function (data) {
                    $('#distanceDetails').html(data);
                    $('#distanceDetails').find('#selectedSportItem').text(selectedItem);
                },
                error: function () {
                    alert("資料錯誤.");
                }
            });
        } else {
            $('#distanceDetails').html('');
            $('#CriticalSpeed').val('');
            $('#AnaerobicPower').val('');
        }

        $('#btn_result_container').show();

        if (selectedItem === "跑步機") {
            $('#CriticalSpeed').val('');
            $('#AnaerobicPower').val('');
            CaculateTreadmillSpeed();
        }
        else if (selectedItem === "田徑場") {
            $('#CriticalSpeed').val('');
            $('#AnaerobicPower').val('');
            trackCalculateLinearRegression();
        }
        else if (selectedItem === "游泳") {
            $('#CriticalSpeed').val('');
            $('#AnaerobicPower').val('');
            PoolCaculateLinearRegression();
        }
        else if (selectedItem === "自由車") {
            $('#CriticalSpeed').val('');
            $('#AnaerobicPower').val('');
            calculateBikeSpeed();
        }
        else if (selectedItem === "滑輪溜冰") {
            $('#CriticalSpeed').val('');
            $('#AnaerobicPower').val('');
        }
    });

    $('#Item').change(function () {
        var selectedValue = $(this).val();
        if (selectedValue === '檢測系統') {
            $('#distanceDetails').show();
        } else {
            $('#distanceDetails').hide();
        }
    });
});