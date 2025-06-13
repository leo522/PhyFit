//document.addEventListener("DOMContentLoaded", function () {
//    var recordCheckButton = document.getElementById("recordCheck");
//    var detectionCheckButton = document.getElementById("DecCheck");

//    if (recordCheckButton) {
//        recordCheckButton.addEventListener("click", handleRedirect);
//    }

//    if (detectionCheckButton) {
//        detectionCheckButton.addEventListener("click", handleRedirect);
//    }

//    function handleRedirect(event) {
//        event.preventDefault();

//        var selectedItem = document.getElementById("Item").value;
//        var userRole = document.getElementById("userRole").value;
//        var athleteID = 10;

//        if (userRole === "Athlete") {
//            athleteID = document.querySelector('input[name="AthleteID"]').value;
//        } else if (userRole === "Coach") {
//            athleteID = document.getElementById("AthletesID").value;
//        }

//        if (!selectedItem || selectedItem === "請選擇訓練項目") {
//            alert("請先選擇訓練項目");
//            return;
//        }

//        if (!athleteID) {
//            alert("請先選擇運動員!!");
//            return;
//        }

//        var data = utf8ToBase64(JSON.stringify({
//            item: selectedItem,
//            AthleteID: athleteID
//        }));

//        window.location.href = "/Record/SessionRecord?data=" + encodeURIComponent(data);
//    }

//    function utf8ToBase64(str) {
//        var utf8Bytes = new TextEncoder().encode(str);
//        return btoa(String.fromCharCode.apply(null, utf8Bytes));
//    }
//});