 document.addEventListener("DOMContentLoaded", function () {
    var decCheckButton = document.getElementById("DecCheck");
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
                window.location.href = "/Record/SessionRecord?item=" + encodeURIComponent(selectedItem) + "&AthleteID=" + encodeURIComponent(athleteID);
            }
        });
    }
});