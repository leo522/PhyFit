function showModal(modalId) {
    var modals = document.querySelectorAll('.modal');
    modals.forEach(function (modal) {
        modal.style.display = "none";
    });

    var modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = "block";

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

function loadSessionRPETrainingRecords() {
    fetch('/PhyFit/LoadSessionRPETrainingRecords', {
        method: 'GET'
    })
        .then(response => response.text())
        .then(data => {
            var modalBody = document.querySelector('#RecordModal .modal-body');
            if (modalBody) {
                modalBody.innerHTML = data;
            }
        })
        .catch(error => console.error('錯誤:', error));
}

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