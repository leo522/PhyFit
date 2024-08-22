var MaxSpeed = 0;
var TreadmillLimitSpeed = 0; //臨界速度
var TreadmillMaxWork = 0 //最大無氧做功
var FailureTime95 = 0, FailureTime90 = 0, FailureTime85 = 0, FailureTime80 = 0;
var TreadMillMaxR_a;
var TreadMillMaxR_b;
var TreadmillDistance95 = 0;
var TreadmillDistance90 = 0;
var TreadmillDistance85 = 0;
var TreadmillDistance80 = 0;

function CaculateTreadmillSpeed() {

    var maxSpeed = parseFloat(document.getElementById("MaxSpeed").value);

    if (isNaN(maxSpeed)) {
        return;
    }

    var table = document.getElementById("dataTable");
    var rows = table.getElementsByTagName("tr");

    for (var i = 0; i < rows.length; i++) {

        var cells = rows[i].getElementsByTagName("td");

        if (cells.length > 0) {

            var intensityPercentage = parseFloat(cells[0].innerText.replace('%', '')); // 移除百分號
            var speed = (maxSpeed * (intensityPercentage / 100)).toFixed(1);
            cells[2].innerText = speed;
        }
    }
}

function calculateSpeed(input) {

    MaxSpeed = parseFloat(document.getElementById("MaxSpeed").value);
    var time = parseFloat(input.value);

    var intensity = parseFloat(input.getAttribute("data-distance")); // 動態獲取強度百分比

    TreadMillMaxR = 0;

    switch (intensity) {
        case 95:
            FailureTime95 = time;
            break;
        case 90:
            FailureTime90 = time;
            break;
        case 85:
            FailureTime85 = time;
            break;
        case 80:
            FailureTime80 = time;
            break;
    }

    var MaxSpeedInKMH = (MaxSpeed * 1000) / 3600;

    var TreadmillDistance95 = (FailureTime95 * MaxSpeedInKMH * 0.95).toFixed(1);
    var TreadmillDistance90 = (FailureTime90 * MaxSpeedInKMH * 0.90).toFixed(1);
    var TreadmillDistance85 = (FailureTime85 * MaxSpeedInKMH * 0.85).toFixed(1);
    var TreadmillDistance80 = (FailureTime80 * MaxSpeedInKMH * 0.80).toFixed(1);

    var MaxR = 0;
    var MaxR_a = 0;
    var MaxR_b = 0;

    for (var i = 0; i < 4; i++) {
        var X_1, X_2, X_3, Y_1, Y_2, Y_3;

        switch (i) {
            case 0:
                X_1 = FailureTime95;
                X_2 = FailureTime90;
                X_3 = FailureTime85;
                Y_1 = TreadmillDistance95;
                Y_2 = TreadmillDistance90;
                Y_3 = TreadmillDistance85;
                break;
            case 1:
                X_1 = FailureTime95;
                X_2 = FailureTime85;
                X_3 = FailureTime80;
                Y_1 = TreadmillDistance95;
                Y_2 = TreadmillDistance85;
                Y_3 = TreadmillDistance80;
                break;
            case 2:
                X_1 = FailureTime95;
                X_2 = FailureTime90;
                X_3 = FailureTime80;
                Y_1 = TreadmillDistance95;
                Y_2 = TreadmillDistance90;
                Y_3 = TreadmillDistance80;
                break;
            case 3:
                X_1 = FailureTime90;
                X_2 = FailureTime85;
                X_3 = FailureTime80;
                Y_1 = TreadmillDistance90;
                Y_2 = TreadmillDistance85;
                Y_3 = TreadmillDistance80;
                break;
        }

        var a, b, mxy, xx, yy, x2, x22;
        a = b = mxy = xx = yy = x2 = x22 = 0.0;

        mxy = 3 * (X_1 * Y_1 + X_2 * Y_2 + X_3 * Y_3);
        xx = 1 * X_1 + 1 * X_2 + 1 * X_3;
        yy = 1 * Y_1 + 1 * Y_2 + 1 * Y_3;
        x2 = 3 * (X_1 * X_1 + X_2 * X_2 + X_3 * X_3);
        x22 = 1 * X_1 + 1 * X_2 + 1 * X_3;

        b = (mxy - xx * yy) / (x2 - x22 * x22);
        a = yy / 3 - b * xx / 3;

        var Y_Average = (Y_1 * 1 + Y_2 * 1 + Y_3 * 1) / 3;

        var Y_DistanceWithBar = (Y_1 - Y_Average) * (Y_1 - Y_Average) + (Y_2 - Y_Average) * (Y_2 - Y_Average) + (Y_3 - Y_Average) * (Y_3 - Y_Average);

        var Y_DistanceWithLine = (Y_1 - (b * X_1 + a * 1)) * (Y_1 - (b * X_1 + a * 1)) + (Y_2 - (b * X_2 + a * 1)) * (Y_2 - (b * X_2 + a * 1)) + (Y_3 - (b * X_3 + a * 1)) * (Y_3 - (b * X_3 + a * 1));

        var R2 = 1 - (Y_DistanceWithLine / Y_DistanceWithBar);

        if (R2 > MaxR) {
            MaxR = R2;
            MaxR_a = a;
            MaxR_b = b;
        }
    }

    TreadMillMaxR_a = MaxR_a;
    TreadMillMaxR_b = MaxR_b;

    TreadmillLimitSpeed = ((TreadMillMaxR_b * 3600) / 1000).toFixed(1); //臨界速度
    document.getElementById("CriticalSpeed").value = TreadmillLimitSpeed; //臨界速度

    TreadmillMaxWork = TreadMillMaxR_a.toFixed(2); //最大無氧做功
    document.getElementById("AnaerobicPower").value = TreadmillMaxWork; //最大無氧做功

    document.getElementById("calculationResult").value = TreadMillMaxR = (Math.floor(MaxR * 100) / 100).toFixed(2);
}
