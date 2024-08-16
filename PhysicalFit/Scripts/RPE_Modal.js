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

//    span.onclick = function () {
//        modal.style.display = "none";
//    };

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

//    var currentRow = $('#RPEModalTraining').data('currentRow');
//    currentRow.find('input[name="RPE"]').val(score);  // 將隱藏的 input 設置為 RPE 分數
//    currentRow.find('input[name="RPE_Description"]').val(description);  // 更新描述
//    $('#RPEModalTraining').hide();
//});

//// 初始化模態窗口（此處可以根據需要初始化其他模態窗口）
//$(document).ready(function () {
//    initializeModal("RPEModalTraining", "openRPEModal");
//});

// 初始化模態窗口
// 初始化模態窗口
function initializeModal(modalId, btnClass) {
    var modal = document.getElementById(modalId);
    var span = modal.getElementsByClassName("close")[0];

    $(document).on('click', `.${btnClass}`, function (event) {
        event.preventDefault();
        modal.style.display = "block";
        var closestRow = $(this).closest('tr');
        $(modal).data('currentRow', closestRow);
    });

    span.onclick = function () {
        modal.style.display = "none";
    };

    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    };
}

// RPE項目點擊事件
$(document).on('click', '.rpe-item', function () {
    var score = $(this).data('score');
    var description = $(this).data('description');
    
    var currentRow = $('#RPEModalTraining').data('currentRow') || $('#RPEModalArchery').data('currentRow') || $('#RPEModalShooting').data('currentRow');

    if (currentRow.length > 0) {
        currentRow.find('input[name="RPE"]').val(score);  // 更新隱藏的 input
        currentRow.find('input[name="RPE_Description"]').val(description);  // 更新描述
    }

    $('.modal').hide(); // 隱藏所有模態窗口
});

// 監控射箭和射擊計算按鈕點擊事件
$(document).ready(function () {
    initializeModal("RPEModalTraining", "openRPEModal");
    initializeModal("RPEModalArchery", "openRPEModalArchery");
    initializeModal("RPEModalShooting", "openRPEModalShooting");

    //$('#btn_calculate_archery').on('click', function () {
    //    // 撰寫射箭訓練量(TL)的計算邏輯
    //});

    //$('#btn_calculate_shooting').on('click', function () {
    //    // 撰寫射擊訓練量(TL)的計算邏輯
    //});
});