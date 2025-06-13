function initializeModal(modalId, btnClass) {
    var modal = document.getElementById(modalId);
    var span = modal.getElementsByClassName("close")[0];

    $(document).off('click', `.${btnClass}`).on('click', `.${btnClass}`, function (event) {
        event.preventDefault();
        modal.style.display = "block";
        var closestRow = $(this).closest('tr');
        $(modal).data('currentRow', closestRow);
    });

    span.onclick = function () {
        modal.style.display = "none";
    };

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

    if (currentRow && currentRow.length > 0) {

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

    $('.modal').hide();
});

$(document).ready(function () {
    initializeModal("RPEModalTraining", "openRPEModal");
    initializeModal("RPEModalArchery", "openRPEModalArchery");
    initializeModal("RPEModalShooting", "openRPEModalShooting");
});