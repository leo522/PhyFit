//document.addEventListener("DOMContentLoaded", function () {
//    var recordCheckButton = document.getElementById("recordCheck");
//    if (recordCheckButton) {
//        recordCheckButton.addEventListener("click", function (event) {
//            event.preventDefault();

//            // 獲取選中的訓練項目
//            var selectedItem = document.getElementById("Item").value;
//            var userRole = document.getElementById("userRole").value; // 取得使用者角色
//            var athleteID = null;

//            // 根據 userRole 判斷運動員或教練來取得 AthleteID
//            if (userRole === "Athlete") {
//                // 如果是運動員，使用隱藏的 AthleteID
//                athleteID = document.querySelector('input[name="AthleteID"]').value;
//            } else if (userRole === "Coach") {
//                // 如果是教練，使用下拉選單選擇的 AthleteID
//                athleteID = document.getElementById("AthletesID").value;
//            }

//            // 驗證選擇的訓練項目和運動員 ID 是否有效
//            if (selectedItem === "請選擇訓練項目") {
//                alert("請先選擇訓練項目");
//            } else if (!athleteID) {
//                alert("請先選擇運動員!!");
//            } else {
//                // 重定向到 SessionRecord 頁面，帶上選定的訓練項目和運動員ID
//                window.location.href = "/Record/SessionRecord?item=" + encodeURIComponent(selectedItem) + "&AthleteID=" + encodeURIComponent(athleteID);
//            }
//        });
//    }
//});

document.addEventListener("DOMContentLoaded", function () {
    var decCheckButton = document.getElementById("recordCheck");
    if (decCheckButton) {
        decCheckButton.addEventListener("click", function (event) {
            event.preventDefault();
            var selectedItem = document.getElementById("Item").value;
            var userRole = document.getElementById("userRole").value;
            var athleteID = null;

            if (userRole === "Athlete") {
                athleteID = document.querySelector('input[name="AthleteID"]').value;
            } else if (userRole === "Coach") {
                athleteID = document.getElementById("AthletesID").value;
            }

            if (selectedItem === "請選擇訓練項目") {
                alert("請先選擇訓練項目");
            } else if (!athleteID) {
                alert("請先選擇運動員!!");
            } else {
                // 使用AJAX發送POST請求
                $.post('/Record/SessionRecord', { item: selectedItem, AthleteID: athleteID })
                    .done(function () {
                        // 成功後重定向
                        window.location.href = '/Record/SessionRecord';
                    })
                    .fail(function () {
                        alert('請求失敗，請稍後再試。');
                    });
            }
        });
    }
});