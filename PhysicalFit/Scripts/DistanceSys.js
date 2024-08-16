$(function () {
    // 監控項目選擇
    $('#Item').change(function () {
        var selectedItem = $(this).val();
        $('.content').hide();
        if (selectedItem === '一般訓練衝量監控 (session-RPE)') {
            $('#trainingMonitoring').show();
        }
        else if (selectedItem === '專項訓練-射箭訓練衝量') {
            $('#archeryMonitoring').show();
        }
        else if (selectedItem === '專項訓練-射擊訓練衝量') {
            $('#shootingMonitoring').show();
        }
        else if (selectedItem === '檢測系統') {
            $('#distanceDetails').show();
        }
    });

    $('#DeteItem').change(function () {

        var selectedItem = $(this).val();

        $('#selectedSportItem').text(selectedItem); //更新選擇的運動項目名稱
        $('#btn_result_container').hide(); // 初始隱藏按鈕容器

        if (selectedItem !== "請選擇運動項目") {
            $.ajax({
                url: '/PhyFit/LoadDistanceDetails',
                data: { itemName: selectedItem },
                success: function (data) {
                    $('#distanceDetails').html(data);
                    $('#distanceDetails').find('#selectedSportItem').text(selectedItem); // 更新 partial view 中的選項
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
        // 根據選擇項目顯示按鈕
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
    //當 #Item 下拉選單改變時觸發
    $('#Item').change(function () {
        var selectedValue = $(this).val();
        if (selectedValue === '檢測系統') {
            $('#distanceDetails').show();
        } else {
            $('#distanceDetails').hide();
        }
    });
});