document.addEventListener("DOMContentLoaded", function () {
    var recordCheckButton = document.getElementById("recordCheck");
    if (recordCheckButton) {
        recordCheckButton.addEventListener("click", function (event) {
            event.preventDefault();

            // 獲取選中的訓練項目
            var selectedItem = document.getElementById("Item").value;
            var userRole = document.getElementById("userRole").value; // 取得使用者角色
            var athleteID = null;

            // 根據 userRole 判斷運動員或教練來取得 AthleteID
            if (userRole === "Athlete") {
                athleteID = document.querySelector('input[name="AthleteID"]').value; // 運動員
            } else if (userRole === "Coach") {
                athleteID = document.getElementById("AthletesID").value; // 教練
            }

            // 驗證選擇的訓練項目和運動員 ID 是否有效
            if (selectedItem === "請選擇訓練項目") {
                alert("請先選擇訓練項目");
            } else if (!athleteID) {
                alert("請先選擇運動員!!");
            } else {
                // 發送 AJAX 請求到控制器，獲取加密後的 URL
                fetch('/Record/GetEncryptedUrl', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ item: selectedItem, athleteID: athleteID })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            window.location.href = data.encryptedUrl; // 重導向到加密後的 URL
                        } else {
                            alert(data.error || "無法生成加密的 URL");
                        }
                    })
                    .catch(error => {
                        console.error("Error:", error);
                        alert("發生錯誤，請稍後再試！");
                    });
            }
        });
    }
});