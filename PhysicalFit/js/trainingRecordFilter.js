var TrainingRecordFilter = (function () {
    function init(options) {
        var trainingType = options.trainingType || '';

        if (!trainingType || trainingType === "請選擇訓練項目") {
            console.warn('請選擇有效的訓練項目');
            return;
        }

        document.getElementById(options.dateInputId).addEventListener("change", function () {
            var selectedDate = this.value;
            if (selectedDate) {
                fetchRecords({
                    date: selectedDate,
                    trainingType: trainingType,
                    showAllMonth: false,
                    onSuccess: options.onSuccess,
                    onError: options.onError
                });
            } else {
                showAlert('warning', '無效的輸入', '請選擇有效的日期');
            }
        });

        document.getElementById(options.showAllButtonId).addEventListener("click", function () {
            fetchRecords({
                trainingType: trainingType,
                showAllMonth: true,
                onSuccess: options.onSuccess,
                onError: options.onError
            });
        });
    }

    function fetchRecords(params) {
        var data = {
            item: params.trainingType,
            showAllMonth: params.showAllMonth
        };

        if (params.date) {
            data.date = params.date;
        }

        $.ajax({
            url: '/Record/SessionRecord',
            type: 'GET',
            data: data,
            success: function (data) {
                if (params.onSuccess) {
                    params.onSuccess(data);
                }
            },
            error: function () {
                if (params.onError) {
                    params.onError();
                } else {
                    showAlert('error', '加載失敗', '無法獲取訓練紀錄，請稍後再試。');
                }
            }
        });
    }

    function showAlert(type, title, text) {
        Swal.fire({
            icon: type,
            title: title,
            text: text,
        });
    }

    return {
        init: init
    };
})();