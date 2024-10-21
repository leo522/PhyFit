/*sessionRecordRedirect.js*/
document.addEventListener("DOMContentLoaded", function () {
    var recordCheckButton = document.getElementById("recordCheck");
    if (recordCheckButton) {
        recordCheckButton.addEventListener("click", function (event) {
            event.preventDefault();

            // 獲取選中的訓練項目和運動員 ID
            var selectedItem = document.getElementById("Item").value;
            var athleteID = document.getElementById("AthletesID") ? document.getElementById("AthletesID").value : null;

            if (selectedItem === "請選擇訓練項目") {
                alert("請先選擇訓練項目");
            } else if (!athleteID) {
                alert("請先選擇運動員");
            } else {
                // 重定向到 SessionRecord 頁面，帶上選定的訓練項目和運動員ID
                window.location.href = "/Record/SessionRecord?item=" + encodeURIComponent(selectedItem) + "&AthleteID=" + encodeURIComponent(athleteID);
            }
        });
    }
});