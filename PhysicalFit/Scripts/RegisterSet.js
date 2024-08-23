//function showFields() {
//    var role = document.getElementById("Role").value;
//    var coachFields = document.getElementById("CoachFields");
//    var athleteFields = document.getElementById("AthleteFields");

//    if (role === "Coach") {
//        coachFields.style.display = "block";
//        athleteFields.style.display = "none";
//    } else if (role === "Athlete") {
//        coachFields.style.display = "none";
//        athleteFields.style.display = "block";
//    } else {
//        coachFields.style.display = "none";
//        athleteFields.style.display = "none";
//    }
//}
function showFields() {
    var role = document.getElementById("Role").value;
    var coachFields = document.getElementById("CoachFields");
    var athleteFields = document.getElementById("AthleteFields");

    if (role === "Coach") {
        coachFields.style.display = "block";
        athleteFields.style.display = "none";
        toggleRequired('#AthleteFields input', false);
        toggleRequired('#CoachFields input', true);
        document.getElementById("SchoolID").disabled = false; // 啟用 SchoolID 欄位
    } else if (role === "Athlete") {
        coachFields.style.display = "none";
        athleteFields.style.display = "block";
        toggleRequired('#CoachFields input', false);
        toggleRequired('#AthleteFields input', true);
        document.getElementById("SchoolID").disabled = true; // 禁用 SchoolID 欄位
    } else {
        coachFields.style.display = "none";
        athleteFields.style.display = "none";
        toggleRequired('#CoachFields input, #AthleteFields input', false);
        document.getElementById("SchoolID").disabled = true; // 禁用 SchoolID 欄位
    }
}

