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