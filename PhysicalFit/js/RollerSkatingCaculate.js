var FailureTime100 = 0;
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

    var distanceText = row.find('td').eq(0).text();
    var distanceMatch = distanceText.match(/^(\d+)/);
    var distance = distanceMatch ? parseFloat(distanceMatch[1]) : NaN;

    var rollertime = parseFloat($(inputElement).val());

    if (!isNaN(distance) && !isNaN(rollertime) && rollertime > 0) {
        var speed = (distance / rollertime) * 3.6;
        row.find('.roller-result').text(speed.toFixed(1));
    } else {
        row.find('.roller-result').text('');
    }

    rollerCalculateLinearRegression();
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
        var label = $(this).closest('tr').find('td').eq(0).text();

        if (label === '100m(穿溜冰鞋)' && !isNaN(time)) {
            FailureTime100 = time;
        }

        // 排除溜冰鞋的資料
        if (label.includes('溜冰鞋')) return;

        var match = label.match(/^(\d+)/);
        var distance = match ? parseFloat(match[1]) : NaN;

        if (!isNaN(time) && !isNaN(distance)) {
            if ([200, 500, 1000, 2000].includes(distance)) {
                time -= FailureTime100;
                distance -= 100;
            }
            failureTimes.push(time);
            distances.push(distance);
        }
    });

    var timeWithSkates = null;
    var timeWithoutSkates = null;

    $('.roller-time').each(function () {
        var time = parseFloat($(this).val());
        var label = $(this).closest('tr').find('td').eq(0).text().trim();

        if (label === '100m(穿溜冰鞋)') {
            timeWithSkates = time;
        }
        if (label === '100m(未穿溜冰鞋)') {
            timeWithoutSkates = time;
        }
    });

    if (!isNaN(timeWithSkates) && !isNaN(timeWithoutSkates) && timeWithoutSkates > 0) {
        var diff = Math.abs(timeWithoutSkates - timeWithSkates);
        var percent = (diff / timeWithoutSkates) * 100;
        $('#RollerSkill').val(percent.toFixed(2));
    } else {
        $('#RollerSkill').val('');
    }

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

    maxR = Math.round(maxR * 10000) / 10000;
    $('#calculationResult').val(maxR);

    RollerMaxR_a = maxRA;
    RollerMaxR_b = maxRB;

    RollerLimitSpeed = ((RollerMaxR_b * 3600) / 1000).toFixed(1);
    document.getElementById("CriticalSpeed").value = RollerLimitSpeed;

    RollerMaxWork = formatFiveDigits(RollerMaxR_a);
    $('#AnaerobicPower').val(RollerMaxWork);
}

function formatFiveDigits(value) {
    return Number.parseFloat(value).toPrecision(5);
}

function RollerSkatingTotalT() {
    var TotalTraining = 0;
    for (var i = 1; i <= TreadmillCurrentSets; i++) {
        TotalTraining += 1;
    }
    document.getElementById("TrainingVol").value = TotT.toFixed(2);
}