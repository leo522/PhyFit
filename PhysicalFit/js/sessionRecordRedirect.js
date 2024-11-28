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
                // 如果是運動員，使用隱藏的 AthleteID
                athleteID = document.querySelector('input[name="AthleteID"]').value;
            } else if (userRole === "Coach") {
                // 如果是教練，使用下拉選單選擇的 AthleteID
                athleteID = document.getElementById("AthletesID").value;
            }

            // 驗證選擇的訓練項目和運動員 ID 是否有效
            if (selectedItem === "請選擇訓練項目") {
                alert("請先選擇訓練項目");
            } else if (!athleteID) {
                alert("請先選擇運動員!!");
            } else {
                // 動態建立表單並提交
                var form = document.createElement("form");
                form.method = "POST";
                form.action = "/Record/SessionRecord"; // 目標 URL

                var itemInput = document.createElement("input");
                itemInput.type = "hidden";
                itemInput.name = "item";
                itemInput.value = selectedItem;
                form.appendChild(itemInput);

                var athleteIDInput = document.createElement("input");
                athleteIDInput.type = "hidden";
                athleteIDInput.name = "AthleteID";
                athleteIDInput.value = athleteID;
                form.appendChild(athleteIDInput);

                document.body.appendChild(form);
                form.submit();
            }
        });
    }
});