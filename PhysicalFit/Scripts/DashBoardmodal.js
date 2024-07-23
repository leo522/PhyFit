function showModal(modalId) {
    // 隱藏所有 Modal
    var modals = document.querySelectorAll('.modal');
    modals.forEach(function (modal) {
        modal.style.display = "none";
    });

    // 顯示指定的 Modal
    var modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = "block";
    }
}

function closeModal() {
    var modals = document.querySelectorAll('.modal');
    modals.forEach(function (modal) {
        modal.style.display = "none";
    });
}

// 監聽所有按鈕點擊事件以顯示相應的 Modal
document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('btnRecord').addEventListener('click', function () {
        showModal('RecordModal');
    });

    document.getElementById('btnModal').addEventListener('click', function () {
        showModal('myModal');
    });

    var closeButtons = document.querySelectorAll('.close');
    closeButtons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            closeModal();
        });
    });

    window.addEventListener('click', function (event) {
        if (event.target.classList.contains('modal')) {
            closeModal();
        }
    });
});