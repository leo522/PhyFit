function showFields() {
    var role = document.getElementById("Role").value;
    var coachFields = document.getElementById("CoachFields");
    var athleteFields = document.getElementById("AthleteFields");

    if (role === "Coach") {
        coachFields.style.display = "block";
        athleteFields.style.display = "none";
    } else if (role === "Athlete") {
        coachFields.style.display = "none";
        athleteFields.style.display = "block";
    } else {
        coachFields.style.display = "none";
        athleteFields.style.display = "none";
    }
}
