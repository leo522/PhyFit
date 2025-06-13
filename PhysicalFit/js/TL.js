$(document).ready(function () {
    $('#btn_calculate').click(function (event) {
        event.preventDefault();

        var totalDailyTL = 0;

        $('#trainingMonitoring table tbody tr').each(function () {
            var rpeValue = $(this).find('input[name="RPE"]').val();
            var trainingTimeValue = $(this).find('select[name="TrainingTime"]').val();

            if (rpeValue && trainingTimeValue) {
                var rpe = parseFloat(rpeValue);
                var trainingTime = parseFloat(trainingTimeValue);

                var sessionTL = rpe * trainingTime;

                $(this).find('input[name="SessionTL"]').val(sessionTL);

                totalDailyTL += sessionTL;
            } else {
                $(this).find('input[name="SessionTL"]').val('');
            }
        });

        $('input[name="DailyTL"]').val(totalDailyTL);
    });

    $('#btn_calculate_archery').click(function (event) {
        event.preventDefault();

        var totalDailyTL = 0;

        $('#archeryMonitoring table tbody tr').each(function () {
            var ArcheryrpeValue = $(this).find('input[name="RPEArchery"]').val();
            var PoundsValue = $(this).find('input[name="Pounds"]').val();
            var ArrowsValue = $(this).find('input[name="Arrows"]').val();

            if (ArcheryrpeValue && PoundsValue && ArrowsValue) {
                var Archeryrpe = parseFloat(ArcheryrpeValue);
                var trainingPounds = parseFloat(PoundsValue);
                var trainingArrows = parseFloat(ArrowsValue);

                var ArcherysessionTL = Archeryrpe * trainingPounds * trainingArrows;

                $(this).find('input[name="SessionArcheryTL"]').val(ArcherysessionTL);

                totalDailyTL += ArcherysessionTL;
            } else {
                $(this).find('input[name="SessionArcheryTL"]').val('');
            }
        });

        $('input[name="ArcheryDailyTL"]').val(totalDailyTL);
    });

    $('#btn_calculate_shooting').click(function (event) {
        event.preventDefault();

        var totalDailyTL = 0;

        $('#shootingMonitoring table tbody tr').each(function () {
            var ShootingValue = $(this).find('input[name="RPEshooting"]').val();
            var BulletValue = $(this).find('input[name="Bullet"]').val();

            if (ShootingValue && BulletValue) {
                var Shootingrpe = parseFloat(ShootingValue);
                var trainingBullet = parseFloat(BulletValue);

                var ShootingsessionTL = Shootingrpe * trainingBullet;

                $(this).find('input[name="SessionShootingTL"]').val(ShootingsessionTL);

                totalDailyTL += ShootingsessionTL;
            } else {
                $(this).find('input[name="SessionShootingTL"]').val('');
            }
        });

        $('input[name="ShootingDailyTL"]').val(totalDailyTL);
    });

    $('#Item').change(function () {
        $('input[name="DailyTL"]').val('') && $('input[name="ArcheryDailyTL"]').val('') && $('input[name="ShootingDailyTL"]').val('');
    });
    $('#TrainingItem').change(function () {
        $('input[name="DailyTL"]').val('');
    });

    $(document).on('click', '.add-row-special', function () {
        var newRow = $(this).closest('tr').clone();
        newRow.find('input').val('');
        newRow.find('select').prop('selectedIndex', 0);
        $(this).closest('tbody').append(newRow);
    });

    $(document).on('click', '.add-row', function () {
        var newRow = $(this).closest('tr').clone();
        newRow.find('input').val('');
        newRow.find('select').prop('selectedIndex', 0);
        $(this).closest('tbody').append(newRow);
    });

    $(document).on('click', '.remove-row-special', function () {
        var rowCount = $(this).closest('tbody').find('tr').length;
        if (rowCount > 1) {
            $(this).closest('tr').remove();
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    $(document).on('click', '.remove-row', function () {
        var rowCount = $(this).closest('tbody').find('tr').length;
        if (rowCount > 1) {
            $(this).closest('tr').remove();
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    $(document).on('click', '.add-row-archery', function () {
        var newRow = $(this).closest('.Archerytraining-group').clone();
        newRow.find('input').val('');
        newRow.find('select').prop('selectedIndex', 0);
        $('#ArcherytrainingRows').append(newRow);
    });

    $(document).on('click', '.remove-row-archery', function () {
        var rowCount = $('#ArcherytrainingRows .Archerytraining-group').length;
        if (rowCount > 1) {
            $(this).closest('.Archerytraining-group').remove();
        } else {
            alert("至少需要一個訓練項目。");
        }
    });

    $(document).on('click', '.add-row-shooting', function () {
        var newRow = $(this).closest('.Shootingtraining-group').clone();
        newRow.find('input').val('');
        newRow.find('select').prop('selectedIndex', 0);
        $('#ShootingtrainingRows').append(newRow);
    });

    $(document).on('click', '.remove-row-shooting', function () {
        var rowCount = $('#ShootingtrainingRows .Shootingtraining-group').length;
        if (rowCount > 1) {
            $(this).closest('.Shootingtraining-group').remove();
        } else {
            alert("至少需要一個訓練項目。");
        }
    });
});