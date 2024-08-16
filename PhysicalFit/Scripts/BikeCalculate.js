var BickmillFailureTime95 = 0;
var BickmillFailureTime90 = 0;
var BickmillFailureTime85 = 0;
var BickmillFailureTime80 = 0;
var BickmillMaxSpeed = 0;
var BickmillCurrentSets = 1;
var BickmillTotT = 0;
var BickmillLimitSpeed = 0;
var BickmillMaxWork = 0;
var BickMillMaxR_a;
var BickMillMaxR_b;

function CaculatePowerSpeed() {
    var MaxSpeed = parseFloat(document.getElementById("MaxSpeed").value);

    if (isNaN(MaxSpeed) || MaxSpeed === '') {
        Swal.fire({
            title: '輸入錯誤',
            text: '請先輸入最大功率瓦數。',
            icon: 'warning',
            confirmButtonText: '確定'
        });
        return; // 終止函式的執行
    }

    var table = document.getElementById("dataTable");
    var rows = table.getElementsByTagName("tr");

    for (var i = 0; i < rows.length; i++) {
        var cells = rows[i].getElementsByTagName("td");
        if (cells.length > 0) {
            var intensityPercentage = parseFloat(cells[0].innerText.replace('%', '')); // 移除百分號
            var speed = (MaxSpeed * (intensityPercentage / 100)).toFixed(1);
            cells[2].innerText = speed;
        }
    }
}

function calculateBikeSpeed(input) {
    var MaxSpeed = parseFloat(document.getElementById("MaxSpeed").value);

    // 檢查 MaxSpeed 是否已設定
    if (isNaN(MaxSpeed) || MaxSpeed === '') {
        Swal.fire({
            title: '輸入錯誤',
            text: '請先執行最大速度計算。',
            icon: 'warning',
            confirmButtonText: '確定'
        });
        return; // 如果 MaxSpeed 尚未設定，則不執行後續代碼
    }

    var time = parseFloat(input.value);
    var Strong = parseFloat(input.getAttribute("data-distance")); // 動態獲取強度百分比

    BickMillMaxR = 0;

    switch (Strong) {
        case 95:
            BickmillFailureTime95 = time;
            break;
        case 90:
            BickmillFailureTime90 = time;
            break;
        case 85:
            BickmillFailureTime85 = time;
            break;
        case 80:
            BickmillFailureTime80 = time;
            break;
    }

    var MaxSpeedKMH = (MaxSpeed * 1000) / 3600;
    var BickmillDistance95 = (BickmillFailureTime95 * MaxSpeedKMH * 0.95).toFixed(1);
    var BickmillDistance90 = (BickmillFailureTime90 * MaxSpeedKMH * 0.90).toFixed(1);
    var BickmillDistance85 = (BickmillFailureTime85 * MaxSpeedKMH * 0.85).toFixed(1);
    var BickmillDistance80 = (BickmillFailureTime80 * MaxSpeedKMH * 0.80).toFixed(1);

    var MaxR = 0;
    var MaxR_a = 0;
    var MaxR_b = 0;

    for (var i = 0; i < 4; i++) {
        var X_1, X_2, X_3, Y_1, Y_2, Y_3;

        switch (i) {
            case 0:
                X_1 = BickmillFailureTime95;
                X_2 = BickmillFailureTime90;
                X_3 = BickmillFailureTime85;
                Y_1 = BickmillDistance95;
                Y_2 = BickmillDistance90;
                Y_3 = BickmillDistance85;
                break;
            case 1:
                X_1 = BickmillFailureTime95;
                X_2 = BickmillFailureTime85;
                X_3 = BickmillFailureTime80;
                Y_1 = BickmillDistance95;
                Y_2 = BickmillDistance85;
                Y_3 = BickmillDistance80;
                break;
            case 2:
                X_1 = BickmillFailureTime95;
                X_2 = BickmillFailureTime90;
                X_3 = BickmillFailureTime80;
                Y_1 = BickmillDistance95;
                Y_2 = BickmillDistance90;
                Y_3 = BickmillDistance80;
                break;
            case 3:
                X_1 = BickmillFailureTime90;
                X_2 = BickmillFailureTime85;
                X_3 = BickmillFailureTime80;
                Y_1 = BickmillDistance90;
                Y_2 = BickmillDistance85;
                Y_3 = BickmillDistance80;
                break;
        }

        var a, b, mxy, xx, yy, x2, x22;
        a = b = mxy = xx = yy = x2 = x22 = 0.0;

        mxy = 3 * (X_1 * Y_1 + X_2 * Y_2 + X_3 * Y_3);
        xx = X_1 + X_2 + X_3;
        yy = parseFloat(Y_1) + parseFloat(Y_2) + parseFloat(Y_3); //轉型
        x2 = 3 * (X_1 * X_1 + X_2 * X_2 + X_3 * X_3);
        x22 = X_1 + X_2 + X_3;

        b = (mxy - xx * yy) / (x2 - x22 * x22);
        a = yy / 3 - b * xx / 3;

        var Y_Average = (parseFloat(Y_1) + parseFloat(Y_2) + parseFloat(Y_3)) / 3; //轉型
        var Y_DistanceWithBar = (Y_1 - Y_Average) * (Y_1 - Y_Average) + (Y_2 - Y_Average) * (Y_2 - Y_Average) + (Y_3 - Y_Average) * (Y_3 - Y_Average);
        var Y_DistanceWithLine = (Y_1 - (b * X_1 + a)) * (Y_1 - (b * X_1 + a)) + (Y_2 - (b * X_2 + a)) * (Y_2 - (b * X_2 + a)) + (Y_3 - (b * X_3 + a)) * (Y_3 - (b * X_3 + a));
        var R2 = 1 - (Y_DistanceWithLine / Y_DistanceWithBar);

        if (R2 > MaxR) {
            MaxR = R2;
            MaxR_a = a;
            MaxR_b = b;
        }
    }

    BickMillMaxR_a = MaxR_a;
    BickMillMaxR_b = MaxR_b;

    BickmillLimitSpeed = BickMillMaxR_b.toFixed(1);
    document.getElementById("CriticalSpeed").value = BickmillLimitSpeed; //臨界速度

    BickmillMaxWork = (BickMillMaxR_a / 1000).toFixed(2);
    document.getElementById("AnaerobicPower").value = BickmillMaxWork; //最大無氧做功

    $('#calculationResult').val(Math.floor(MaxR * 100) / 100);

    //BickMillMaxR = Math.floor(MaxR * 100) / 100;
    //if (document.getElementById("calculationResult")) {
    //    document.getElementById("calculationResult").value = BickMillMaxR;
    //} else {
    //    console.error("Element with id 'calculationResult' not found.");
    //}
}
