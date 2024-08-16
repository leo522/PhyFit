$(document).ready(function () {
    // 初始化時隱藏所有監控項目
    $('#session_RPE, #arrow_Training, #shotting_Training, #Detection_Sys, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord, #sessionRPEresult, #btnModal, #btn_detection').hide();

        // 當選擇監控項目時
        $('#Item').change(function () {
            const selectItem = $(this).val();

            // 隱藏所有項目
            $('#session_RPE, #arrow_Training, #shotting_Training, #Detection_Sys, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord, #btnModal, #btn_detection').hide();

            // 根據選擇的項目顯示對應的內容
            if (selectItem === '一般訓練衝量監控 (session-RPE)') {
                $('#session_RPE, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord').show();
                $('#sessionRPEresult').show(); // 顯示 #sessionRPEresult
            } else if (selectItem === '專項訓練-射箭訓練衝量') {
                $('#arrow_Training, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord').show();
                $('#sessionRPEresult').show(); // 顯示 #sessionRPEresult
            } else if (selectItem === '專項訓練-射擊訓練衝量') {
                $('#shotting_Training, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord').show();
                $('#sessionRPEresult').show(); // 顯示 #sessionRPEresult
            } else if (selectItem === '檢測系統') {
                $('#Detection_Sys, #RPEtable, #btnModal,#btn_detection').show();
                $('#btn_result, #btn_save, #btnRecord, #sessionRPEresult').hide(); // 隱藏 #btn_result, #btn_save, #btnRecord 和 #sessionRPEresult
            }
        });
});