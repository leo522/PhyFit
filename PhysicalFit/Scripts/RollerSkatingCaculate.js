////RollerSkatingCaculate.js
//var FailureTime200 = 0;
//var FailureTime500 = 0;
//var FailureTime1000 = 0;
//var FailureTime2000 = 0;
//var RollerTotT = 0;
//var RollerLimitSpeed = 0;
//var RollerMaxWork = 0;
//var RollerMaxR = 0;
//var RollerMaxR_a;
//var RollerMaxR_b;
//var RollerLimitSpeed = 0; // 臨界速度
//var RollerMaxWork = 0; // 最大無氧做功
//var RollermillTotT = 0; // 訓練量

//function calculateSpeed(inputElement) {
//    var row = $(inputElement).closest('tr');
//    var distance = parseFloat(row.find('td').eq(0).text());
//    var rollertime = parseFloat($(inputElement).val());

//    //減去100公尺所需的時間
//    if (!isNaN(distance) && !isNaN(rollertime) && rollertime > 0) {
//        if (distance === 200 || distance === 500 || distance === 1000 || distance === 2000) {
//            rollertime -= calculate100mTime(); //計算並減去100公尺的時間
//        }

//        // 確保 rollertime 不為負數
//        if (rollertime > 0) {
//            var speed = (distance / rollertime) * 3.6;
//            row.find('.roller-result').val(speed.toFixed(2)); // 更新速度結果
//        } else {
//            row.find('.roller-result').val(''); // 清空速度
//        }
//    } else {
//        row.find('.roller-result').val(''); // 清空速度
//    }

//    rollerCalculateLinearRegression(); // 每次更新速度時也呼叫線性回歸計算
//}

//// 計算100公尺的時間
//function calculate100mTime() {
//    return parseFloat(timeFor100m) || 0; //以100公尺的資料庫值
//}

//    if (!isNaN(distance) && !isNaN(rollertime) && rollertime > 0) {
//        var speed = (distance / rollertime) * 3.6;

//        row.find('.roller-result').val(speed.toFixed(2)); //更新速度結果
//    } else {
//        row.find('.roller-result').val(''); //清空速度
//    }

//    rollerCalculateLinearRegression();  //每次更新速度時也呼叫線性回歸計算


//function rollerCalculateLinearRegression() {
//    var failureTimes = [];
//    var distances = [];

//    $('.roller-time').each(function () {
//        var time = parseFloat($(this).val());
//        if (!isNaN(time)) {
//            failureTimes.push(time);
//            distances.push(parseFloat($(this).data('distance')));
//        }
//    });

//    if (failureTimes.length < 3) {
//        $('#calculationResult').val('');
//        return;
//    }

//    var RollerMaxR = 0;
//    var maxR = 0;
//    var maxRA = 0;
//    var maxRB = 0;

//    for (var i = 0; i < 4; i++) {
//        var X1, X2, X3, Y1, Y2, Y3;
//        switch (i) {
//            case 0:
//                X1 = failureTimes[0];
//                X2 = failureTimes[1];
//                X3 = failureTimes[2];
//                Y1 = distances[0];
//                Y2 = distances[1];
//                Y3 = distances[2];
//                break;
//            case 1:
//                if (failureTimes.length > 3) {
//                    X1 = failureTimes[0];
//                    X2 = failureTimes[1];
//                    X3 = failureTimes[3];
//                    Y1 = distances[0];
//                    Y2 = distances[1];
//                    Y3 = distances[3];
//                } else {
//                    continue;
//                }
//                break;
//            case 2:
//                if (failureTimes.length > 3) {
//                    X1 = failureTimes[0];
//                    X2 = failureTimes[2];
//                    X3 = failureTimes[3];
//                    Y1 = distances[0];
//                    Y2 = distances[2];
//                    Y3 = distances[3];
//                } else {
//                    continue;
//                }
//                break;
//            case 3:
//                if (failureTimes.length > 3) {
//                    X1 = failureTimes[1];
//                    X2 = failureTimes[2];
//                    X3 = failureTimes[3];
//                    Y1 = distances[1];
//                    Y2 = distances[2];
//                    Y3 = distances[3];
//                } else {
//                    continue;
//                }
//                break;
//        }

//        var a, b, mxy, xx, yy, x2, x22;
//        a = b = mxy = xx = yy = x2 = x22 = 0.0;

//        mxy = 3 * (X1 * Y1 + X2 * Y2 + X3 * Y3);
//        xx = X1 + X2 + X3;
//        yy = Y1 + Y2 + Y3;
//        x2 = 3 * (X1 * X1 + X2 * X2 + X3 * X3);
//        x22 = X1 + X2 + X3;

//        b = (mxy - xx * yy) / (x2 - x22 * x22);
//        a = yy / 3 - b * xx / 3;

//        var yAverage = (Y1 + Y2 + Y3) / 3;
//        var yDistanceWithBar = Math.pow(Y1 - yAverage, 2) + Math.pow(Y2 - yAverage, 2) + Math.pow(Y3 - yAverage, 2);
//        var yDistanceWithLine = Math.pow(Y1 - (b * X1 + a), 2) + Math.pow(Y2 - (b * X2 + a), 2) + Math.pow(Y3 - (b * X3 + a), 2);
//        var r2 = 1 - (yDistanceWithLine / yDistanceWithBar);

//        if (r2 > maxR) {
//            maxR = r2;
//            maxRA = a;
//            maxRB = b;
//        }
//    }

//    $('#calculationResult').val(Math.floor(maxR * 100) / 100); //r^2，決定係數

//    RollerMaxR_a = maxRA;
//    RollerMaxR_b = maxRB;

//    RollerLimitSpeed = ((RollerMaxR_b * 3600) / 1000).toFixed(1); //臨界速度
//    document.getElementById("CriticalSpeed").value = RollerLimitSpeed; //臨界速度

//    RollerMaxWork = RollerMaxR_a.toFixed(2); //最大無氧做功
//    $('#AnaerobicPower').val(RollerMaxWork); //最大無氧做功

//    //RollerMaxWork = RollerMaxR_a.toFixed(2); //最大無氧做功
//    //document.getElementById("AnaerobicPower").value = RollerMaxWork; //最大無氧做功
//}

//    function RollerSkatingTotalT() {
//        var TotalTraining = 0;

//        for (var i = 1; i <= TreadmillCurrentSets; i++) {
//            TotalTraining = TotalTraining + 1;
//        }

//        document.getElementById("TrainingVol").value = TotT.toFixed(2);
//    }


var FailureTime100 = 0; // 新增 100m 力竭時間
var FailureTime200 = 0;
var FailureTime500 = 0;
var FailureTime1000 = 0;
var FailureTime2000 = 0;
var RollerTotT = 0;
var RollerLimitSpeed = 0;
var RollerMaxWork = 0;
var RollerMaxR = 0;
var RollerMaxR_a;
var RollerMaxR_b;

function calculateSpeed(inputElement) {
    var row = $(inputElement).closest('tr');
    var distance = parseFloat(row.find('td').eq(0).text());
    var rollertime = parseFloat($(inputElement).val());

    // 如果距離是 200m, 500m, 1000m, 2000m，先減去 100m 的力竭時間
    if (distance === 200 || distance === 500 || distance === 1000 || distance === 2000) {
        rollertime -= getFailureTime(100);  // 使用 getFailureTime 函數取得 100m 的力竭時間
    }

    if (!isNaN(distance) && !isNaN(rollertime) && rollertime > 0) {
        var speed = (distance / rollertime) * 3.6;
        row.find('.roller-result').val(speed.toFixed(2)); // 更新速度結果
    } else {
        row.find('.roller-result').val(''); // 清空速度
    }

    rollerCalculateLinearRegression();  // 每次更新速度時也呼叫線性回歸計算
}

function getFailureTime(distance) {
    switch (distance) {
        case 100:
            return FailureTime100;
        case 200:
            return FailureTime200;
        case 500:
            return FailureTime500;
        case 1000:
            return FailureTime1000;
        case 2000:
            return FailureTime2000;
        default:
            return 0;
    }
}

function rollerCalculateLinearRegression() {
    var failureTimes = [];
    var distances = [];

    $('.roller-time').each(function () {
        var time = parseFloat($(this).val());
        if (!isNaN(time)) {
            failureTimes.push(time);
            distances.push(parseFloat($(this).data('distance')));
        }
    });

    if (failureTimes.length < 3) {
        $('#calculationResult').val('');
        return;
    }

    var RollerMaxR = 0;
    var maxR = 0;
    var maxRA = 0;
    var maxRB = 0;

    for (var i = 0; i < 4; i++) {
        var X1, X2, X3, Y1, Y2, Y3;
        switch (i) {
            case 0:
                X1 = failureTimes[0];
                X2 = failureTimes[1];
                X3 = failureTimes[2];
                Y1 = distances[0];
                Y2 = distances[1];
                Y3 = distances[2];
                break;
            case 1:
                if (failureTimes.length > 3) {
                    X1 = failureTimes[0];
                    X2 = failureTimes[1];
                    X3 = failureTimes[3];
                    Y1 = distances[0];
                    Y2 = distances[1];
                    Y3 = distances[3];
                } else {
                    continue;
                }
                break;
            case 2:
                if (failureTimes.length > 3) {
                    X1 = failureTimes[0];
                    X2 = failureTimes[2];
                    X3 = failureTimes[3];
                    Y1 = distances[0];
                    Y2 = distances[2];
                    Y3 = distances[3];
                } else {
                    continue;
                }
                break;
            case 3:
                if (failureTimes.length > 3) {
                    X1 = failureTimes[1];
                    X2 = failureTimes[2];
                    X3 = failureTimes[3];
                    Y1 = distances[1];
                    Y2 = distances[2];
                    Y3 = distances[3];
                } else {
                    continue;
                }
                break;
        }

        var a, b, mxy, xx, yy, x2, x22;
        a = b = mxy = xx = yy = x2 = x22 = 0.0;

        mxy = 3 * (X1 * Y1 + X2 * Y2 + X3 * Y3);
        xx = X1 + X2 + X3;
        yy = Y1 + Y2 + Y3;
        x2 = 3 * (X1 * X1 + X2 * X2 + X3 * X3);
        x22 = X1 + X2 + X3;

        b = (mxy - xx * yy) / (x2 - x22 * x22);
        a = yy / 3 - b * xx / 3;

        var yAverage = (Y1 + Y2 + Y3) / 3;
        var yDistanceWithBar = Math.pow(Y1 - yAverage, 2) + Math.pow(Y2 - yAverage, 2) + Math.pow(Y3 - yAverage, 2);
        var yDistanceWithLine = Math.pow(Y1 - (b * X1 + a), 2) + Math.pow(Y2 - (b * X2 + a), 2) + Math.pow(Y3 - (b * X3 + a), 2);
        var r2 = 1 - (yDistanceWithLine / yDistanceWithBar);

        if (r2 > maxR) {
            maxR = r2;
            maxRA = a;
            maxRB = b;
        }
    }

    $('#calculationResult').val(Math.floor(maxR * 100) / 100); // r^2，決定係數

    RollerMaxR_a = maxRA;
    RollerMaxR_b = maxRB;

    RollerLimitSpeed = ((RollerMaxR_b * 3600) / 1000).toFixed(1); // 臨界速度
    document.getElementById("CriticalSpeed").value = RollerLimitSpeed;

    RollerMaxWork = RollerMaxR_a.toFixed(2); // 最大無氧做功
    $('#AnaerobicPower').val(RollerMaxWork);
}

function RollerSkatingTotalT() {
    var TotalTraining = 0;
    for (var i = 1; i <= TreadmillCurrentSets; i++) {
        TotalTraining = TotalTraining + 1;
    }
    document.getElementById("TrainingVol").value = TotT.toFixed(2);
}