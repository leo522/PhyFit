// 顯示指定的 Modal 並加載其內容
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

        // 如果是 RecordModal，則加載其內容
        if (modalId === 'RecordModal') {
            loadSessionRPETrainingRecords();
        }
    }
}

function closeModal() {
    var modals = document.querySelectorAll('.modal');
    modals.forEach(function (modal) {
        modal.style.display = "none";
    });
}

// 加載 Session RPE Training Records 部分視圖
function loadSessionRPETrainingRecords() {
    fetch('/PhyFit/LoadSessionRPETrainingRecords', {
        method: 'GET'
    })
        .then(response => response.text())
        .then(data => {
            // 將部分視圖的 HTML 加載到模態框的內容中
            var modalBody = document.querySelector('#RecordModal .modal-body');
            if (modalBody) {
                modalBody.innerHTML = data;
            }
        })
        .catch(error => console.error('錯誤:', error));
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