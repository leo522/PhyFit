$(document).ready(function () {
    $('#session_RPE, #arrow_Training, #shotting_Training, #Detection_Sys, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord, #sessionRPEresult, #btnModal, #btn_detection').hide();

        $('#Item').change(function () {
            const selectItem = $(this).val();

            $('#session_RPE, #arrow_Training, #shotting_Training, #Detection_Sys, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord, #btnModal, #btn_detection').hide();

            if (selectItem === '一般訓練衝量監控 (session-RPE)') {
                $('#session_RPE, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord').show();
                $('#sessionRPEresult').show();
            } else if (selectItem === '射箭訓練衝量') {
                $('#arrow_Training, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord').show();
                $('#sessionRPEresult').show();
            } else if (selectItem === '射擊訓練衝量') {
                $('#shotting_Training, #RPEtable, #btnModal, #btn_result, #btn_save, #btnRecord').show();
                $('#sessionRPEresult').show();
            } else if (selectItem === '檢測系統') {
                $('#Detection_Sys, #RPEtable, #btnModal,#btn_detection').show();
                $('#btn_result, #btn_save, #btnRecord, #sessionRPEresult').hide();
            }
        });
});