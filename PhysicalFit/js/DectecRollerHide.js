// trainingSelection.js
class TrainingSelection {
    constructor(selectElementId) {
        this.selectElement = document.getElementById(selectElementId);
        this.selectedSportItem = document.getElementById('selectedSportItem');
        this.criticalTrainingVolumeSection = document.querySelector('.Criticaltraining-volume-section');
        this.anaerobicTrainingVolumeSection = document.querySelector('.Anaerobictraining-volume-section');

        if (this.selectElement) {
            this.init();
        } else {
            console.error("找不到運動項目選擇元素");
        }
    }

    init() {
        this.selectElement.addEventListener('change', () => {
            this.handleSelectionChange();
        });
    }

    handleSelectionChange() {
        const selectedValue = this.selectElement.value;
        this.selectedSportItem.textContent = selectedValue !== "" ? selectedValue : "";

        if (selectedValue === "滑輪溜冰") {
            this.criticalTrainingVolumeSection.style.display = "none";
            this.anaerobicTrainingVolumeSection.style.display = "none";
        } else {
            this.criticalTrainingVolumeSection.style.display = "block";
            this.anaerobicTrainingVolumeSection.style.display = "block";
        }
    }
}

// 使用時
document.addEventListener('DOMContentLoaded', () => {
    new TrainingSelection('DeteItem');
});