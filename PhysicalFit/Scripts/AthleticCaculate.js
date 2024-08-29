//AthleticCaculate.js

var FailureTime200 = 0;
var FailureTime400 = 0;
var FailureTime800 = 0;
var FailureTime1200 = 0;
var TrackTotT = 0;
var TrackLimitSpeed = 0;
var TrackMaxWork = 0;
var TrackMaxR = 0;
var TrackMaxR_a;
var TrackMaxR_b;
var TrackLimitSpeed = 0; //臨界速度
var TrackMaxWork = 0; //最大無氧做功
var TreadmillTotT = 0; //訓練量
var TreadmillCurrentSets = 1; //訓練量預設值
//var TreadmillCurrentMode = "SingleSet"; //處方訓練模式，單組模式
var TreadmillCurrentSets = 1; //設定組數

function calculateSpeed(inputElement) {
    var row = $(inputElement).closest('tr');
    var distance = parseFloat(row.find('td').eq(0).text());
    var exhaustionTime = parseFloat($(inputElement).val());

    //if (!isNaN(distance) && !isNaN(exhaustionTime) && exhaustionTime > 0) {
    //    var speed = (distance / exhaustionTime) * 3.6;
    //    row.find('td').eq(2).text(speed.toFixed(2));
    //} else {
    //    row.find('td').eq(2).text('');
    //}
    if (!isNaN(distance) && !isNaN(exhaustionTime) && exhaustionTime > 0) {
        var speed = (distance / exhaustionTime) * 3.6;
        // 使用 id 來選擇元素
        row.find('#speed-result').val(speed.toFixed(2)); // 更新速度結果
    } else {
        row.find('#speed-result').val(''); // 清空速度
    }

    trackCalculateLinearRegression();
}

function trackCalculateLinearRegression() {
    var failureTimes = [];
    var distances = [];

    $('.exhaustion-time').each(function () {
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

    var trackMaxR = 0;
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
    TrackMaxR_a = maxRA;
    TrackMaxR_b = maxRB;

    TrackLimitSpeed = ((TrackMaxR_b * 3600) / 1000).toFixed(1); //臨界速度
    document.getElementById("CriticalSpeed").value = TrackLimitSpeed; //臨界速度

    TrackMaxWork = TrackMaxR_a.toFixed(2); //最大無氧做功
    document.getElementById("AnaerobicPower").value = TrackMaxWork; //最大無氧做功


    $('#calculationResult').val(Math.floor(maxR * 100) / 100);
}

//計算運動量
function TreadmillPrescriptionTable(Mode, SetNum) {
    TreadmillCurrentMode = Mode;
    TreadmillCurrentSets = SetNum;
    var xmlhttp;
    if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari
        xmlhttp = new XMLHttpRequest();
    }
    else {// code for IE6, IE5
        xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
    }
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            document.getElementById("div_PrescriptionInfo").innerHTML = xmlhttp.responseText;
            TreadmillFactor = document.getElementById("select_TM_Factor").value;
            TreadmillPrescriptionAttribute();
            TreadmillInitialPrescription();
        }
    }
    xmlhttp.open("GET", "Treadmill/Prescription.php?Mode=" + TreadmillCurrentMode + "&SetNum=" + TreadmillCurrentSets, true);
    xmlhttp.send();
}

function TreadmillTotalT() {
    var TotalTraining = 0;

    for (var i = 1; i <= TreadmillCurrentSets; i++)
    {
        TotalTraining = TotalTraining + 1 ;
        //TotT += parseFloat(document.getElementById("input_TM_PartT_S" + i).value);
    }
    
    document.getElementById("TrainingVol").value = TotT.toFixed(2);
    console.log(TotT);
    debugger;
}
//input_TM_PartT_S 訓練量小計
