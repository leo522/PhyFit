﻿/*游泳計算公式*/
var PoolFailureTime100 = 0;
var PoolFailureTime200 = 0;
var PoolFailureTime400 = 0;
var PoolFailureTime800 = 0;
var PoolMaxR = 0;
var PoolMaxR_a = 0;
var PoolMaxR_b = 0;
var PoolTotT = 0;
var PoolLimitSpeed = 0; //臨界速度
var PoolMaxWork = 0; //最大無氧做功

function calculateSpeed(inputElement) {
    var row = $(inputElement).closest('tr');
    var distance = parseFloat(row.find('td').eq(0).text());
    var failureTime = parseFloat($(inputElement).val());

    if (!isNaN(distance) && !isNaN(failureTime) && failureTime > 0) {
        var speed = (distance / failureTime);
        row.find('.speed-result').text(speed.toFixed(1)); //正確地更新速度顯示

    } else {
        row.find('.speed-result').text(''); //清空顯示
    }

    CaculatePoolSpeed(distance, failureTime);
}
function CaculatePoolSpeed(distance, failureTime) {
    // 更新對應距離的力竭時間
    switch (distance) {
        case 100:
            PoolFailureTime100 = failureTime;
            break;
        case 200:
            PoolFailureTime200 = failureTime;
            break;
        case 400:
            PoolFailureTime400 = failureTime;
            break;
        case 800:
            PoolFailureTime800 = failureTime;
            break;
    }

    var timeElement = document.querySelector(`input[data-distance="${distance}"]`);
    if (timeElement) {
        var timeValue = parseFloat(timeElement.value);
        if (isNaN(timeValue) || timeValue === 0) {
            return;
        }
        var speed = (distance / timeValue * 3.6).toFixed(1); // 轉換為 km/h
        // 找到對應的速度<td>元素並更新內容
        var speedElement = timeElement.parentElement.nextElementSibling;
        if (speedElement && speedElement.classList.contains('speed')) {
            speedElement.textContent = speed;
        }
    }

    PoolCaculateLinearRegression();
}

function PoolCaculateLinearRegression() {
    PoolMaxR = 0;
    var MaxR = 0;
    var MaxR_a = 0;
    var MaxR_b = 0;

    for (var i = 0; i < 4; i++) {
        var X_1, X_2, X_3, Y_1, Y_2, Y_3;

        switch (i) {
            case 0:
                X_1 = PoolFailureTime100;
                X_2 = PoolFailureTime200;
                X_3 = PoolFailureTime400;
                Y_1 = 100;
                Y_2 = 200;
                Y_3 = 400;
                break;
            case 1:
                X_1 = PoolFailureTime100;
                X_2 = PoolFailureTime200;
                X_3 = PoolFailureTime800;
                Y_1 = 100;
                Y_2 = 200;
                Y_3 = 800;
                break;
            case 2:
                X_1 = PoolFailureTime100;
                X_2 = PoolFailureTime400;
                X_3 = PoolFailureTime800;
                Y_1 = 100;
                Y_2 = 400;
                Y_3 = 800;
                break;
            case 3:
                X_1 = PoolFailureTime200;
                X_2 = PoolFailureTime400;
                X_3 = PoolFailureTime800;
                Y_1 = 200;
                Y_2 = 400;
                Y_3 = 800;
                break;
        }

        var a = 0, b = 0, mxy = 0, xx = 0, yy = 0, x2 = 0, x22 = 0;

        mxy = 3 * (X_1 * Y_1 + X_2 * Y_2 + X_3 * Y_3);
        xx = X_1 + X_2 + X_3;
        yy = Y_1 + Y_2 + Y_3;
        x2 = 3 * (X_1 * X_1 + X_2 * X_2 + X_3 * X_3);
        x22 = X_1 + X_2 + X_3;

        b = (mxy - xx * yy) / (x2 - x22 * x22);
        a = yy / 3 - b * xx / 3;

        var Y_Average = (Y_1 + Y_2 + Y_3) / 3;
        var Y_DistanceWithBar = (Y_1 - Y_Average) * (Y_1 - Y_Average) + (Y_2 - Y_Average) * (Y_2 - Y_Average) + (Y_3 - Y_Average) * (Y_3 - Y_Average);
        var Y_DistanceWithLine = (Y_1 - (b * X_1 + a)) * (Y_1 - (b * X_1 + a)) + (Y_2 - (b * X_2 + a)) * (Y_2 - (b * X_2 + a)) + (Y_3 - (b * X_3 + a)) * (Y_3 - (b * X_3 + a));
        var R2 = 1 - (Y_DistanceWithLine / Y_DistanceWithBar);

        if (R2 > MaxR) {
            MaxR = R2;
            MaxR_a = a;
            MaxR_b = b;
        }
    }

    PoolMaxR_a = MaxR_a;
    PoolMaxR_b = MaxR_b;

    PoolLimitSpeed = PoolMaxR_b.toFixed(1); //臨界速度
    document.getElementById("CriticalSpeed").value = PoolLimitSpeed; //臨界速度

    PoolMaxWork = PoolMaxR_a.toFixed(2); //臨界速度
    document.getElementById("AnaerobicPower").value = PoolMaxWork; //最大無氧做功

    //document.getElementById("calculationResult").value = (Math.floor(MaxR * 100) / 100).toFixed(2); //r^2，決定係數
    $('#calculationResult').val(Math.floor(MaxR * 100) / 100);
}