//// 初始化模態窗口
//function initializeModal(modalId, btnClass) {
//    var modal = document.getElementById(modalId);
//    var span = modal.getElementsByClassName("close")[0];

//    $(document).on('click', `.${btnClass}`, function (event) {
//        event.preventDefault();
//        modal.style.display = "block";
//        var closestRow = $(this).closest('tr');
//        $(modal).data('currentRow', closestRow);
//    });

//    // 綁定點擊關閉按鈕事件以關閉模態窗口
//    span.onclick = function () {
//        modal.style.display = "none";
//    };

//    // 綁定點擊模態窗口外部事件以關閉模態窗口
//    window.onclick = function (event) {
//        if (event.target == modal) {
//            modal.style.display = "none";
//        }
//    };
//}

//// RPE項目點擊事件
//$(document).on('click', '.rpe-item', function () {
//    var score = $(this).data('score');
//    var description = $(this).data('description');

//    var scoreArchery = $(this).data('score'); //射箭
//    var descriptionArchery = $(this).data('description');//射箭

//    var scoreShooting = $(this).data('score'); //射擊
//    var descriptionShooting = $(this).data('description');//射擊

//    var currentRow = $('#RPEModalTraining').data('currentRow') || $('#RPEModalArchery').data('currentRow') || $('#RPEModalShooting').data('currentRow');

//    if (currentRow.length > 0) {
//        currentRow.find('input[name="RPE"]').val(score);  // 更新隱藏的自覺 input
//        currentRow.find('input[name="RPE_Description"]').val(description);  // 更新自覺描述

//        currentRow.find('input[name="RPEArchery"]').val(scoreArchery);  // 更新隱藏的射箭 input
//        currentRow.find('input[name="RPE_ArcheryDescription"]').val(descriptionArchery);  // 更新射箭自覺描述

//        currentRow.find('input[name="RPEshooting"]').val(scoreShooting);  // 更新隱藏的射擊 input
//        currentRow.find('input[name="RPE_shootingDescription"]').val(descriptionShooting);  // 更新射擊自覺描述
//    }

//    $('.modal').hide(); // 隱藏所有模態窗口
//});

//// 文檔加載完成後初始化所有模態窗口
//$(document).ready(function () {
//    initializeModal("RPEModalTraining", "openRPEModal");
//    initializeModal("RPEModalArchery", "openRPEModalArchery");
//    initializeModal("RPEModalShooting", "openRPEModalShooting");
//});
// 初始化模態窗口
function initializeModal(modalId, btnClass) {
    var modal = document.getElementById(modalId);
    var span = modal.getElementsByClassName("close")[0];
    console.log("Initializing modal with ID:", modalId);
    // 避免重複綁定事件處理器
    $(document).off('click', `.${btnClass}`).on('click', `.${btnClass}`, function (event) {
        event.preventDefault();
        modal.style.display = "block";
        var closestRow = $(this).closest('tr');
        $(modal).data('currentRow', closestRow);

        console.log("Modal opened for row:", closestRow);
    });

    // 綁定點擊關閉按鈕事件以關閉模態窗口
    span.onclick = function () {
        modal.style.display = "none";
    };

    // 綁定點擊模態窗口外部事件以關閉模態窗口
    $(window).off('click').on('click', function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    });
}

// RPE項目點擊事件
//$(document).on('click', '.rpe-item', function () {
//    var score = $(this).data('score');
//    var description = $(this).data('description');

//    var currentRow = $('#RPEModalTraining').data('currentRow') ||
//        $('#RPEModalArchery').data('currentRow') ||
//        $('#RPEModalShooting').data('currentRow');

//    if (currentRow && currentRow.length > 0) {
//        if (currentRow.find('input[name="RPE"]').length) {
//            currentRow.find('input[name="RPE"]').val(score);
//            currentRow.find('input[name="RPE_Description"]').val(description);
//        }
//        if (currentRow.find('input[name="RPEArchery"]').length) {
//            currentRow.find('input[name="RPEArchery"]').val(score);
//            currentRow.find('input[name="RPE_ArcheryDescription"]').val(description);
//        }
//        if (currentRow.find('input[name="RPEshooting"]').length) {
//            currentRow.find('input[name="RPEshooting"]').val(score);
//            currentRow.find('input[name="RPE_shootingDescription"]').val(description);
//        }
//    }

//    $('.modal').hide(); // 隱藏所有模態窗口
//});
$(document).on('click', '.rpe-item', function () {
    var score = $(this).data('score');
    var description = $(this).data('description');

    var currentRow = $('#RPEModalTraining').data('currentRow') ||
        $('#RPEModalArchery').data('currentRow') ||
        $('#RPEModalShooting').data('currentRow');

    console.log("Current row:", currentRow);

    if (currentRow && currentRow.length > 0) {
        // 根據具體的表單元素名稱來回填數據
        if (currentRow.find('input[name="RPE"]').length) {
            currentRow.find('input[name="RPE"]').val(score);
            currentRow.find('input[name="RPE_Description"]').val(description);
        }
        if (currentRow.find('input[name="RPEArchery"]').length) {
            currentRow.find('input[name="RPEArchery"]').val(score);
            currentRow.find('input[name="RPE_ArcheryDescription"]').val(description);
        }
        if (currentRow.find('input[name="RPEshooting"]').length) {
            currentRow.find('input[name="RPEshooting"]').val(score);
            currentRow.find('input[name="RPE_shootingDescription"]').val(description);
        }
    }

    $('.modal').hide(); // 隱藏所有模態窗口
});
// 文檔加載完成後初始化所有模態窗口
$(document).ready(function () {
    initializeModal("RPEModalTraining", "openRPEModal");
    initializeModal("RPEModalArchery", "openRPEModalArchery");
    initializeModal("RPEModalShooting", "openRPEModalShooting");
});