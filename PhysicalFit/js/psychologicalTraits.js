function collectPsychologicalTraits(dateValue, userId) {
    var athleteId = document.getElementById("athleteId").value;
    var traitsData = [];

    $('#psychologicalRows tr').each(function () {
        var traitName = $(this).find('td:first-child label').text().trim();
        var selectedFeeling = $(this).find('input[type="radio"]:checked').val();

        if (selectedFeeling) {
            var traitType = '';

            if (traitName.includes('睡眠品質')) {
                traitType = '睡眠品質';
            } else if (traitName.includes('疲憊程度')) {
                traitType = '疲憊程度';
            } else if (traitName.includes('訓練意願')) {
                traitType = '訓練意願';
            } else if (traitName.includes('胃口')) {
                traitType = '胃口';
            } else if (traitName.includes('比賽意願')) {
                traitType = '比賽意願';
            }

            traitsData.push({
                Trait: traitName,
                Feeling: selectedFeeling,
                UserID: userId,
                PsychologicalDate: dateValue,
                Score: calculateScore(selectedFeeling, traitType)
            });
        }
    });

    return traitsData;
}

function sendTraitsToServer(traitsData) {
    $.ajax({
        url: '/PhyFit/SubmitTraits',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(traitsData),
        success: function (response) {
            if (response.success) {
                Swal.fire({
                    title: '成功',
                    text: response.message,
                    icon: 'success',
                    confirmButtonText: '確定'
                });
            } else {
                Swal.fire({
                    title: '錯誤',
                    text: response.message,
                    icon: 'error',
                    confirmButtonText: '確定'
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                title: '錯誤',
                text: '資料儲存失敗！',
                icon: 'error',
                confirmButtonText: '確定'
            });
        }
    });
}

function calculateScore(feeling, traitType) {
    const scores = {
        '睡眠品質': {
            '沒有睡': 1,
            '有睡但不覺得休息到': 2,
            '一般睡眠': 3,
            '深層睡眠': 4
        },
        '疲憊程度': {
            '疲憊到感到痛苦': 1,
            '非常疲憊': 2,
            '疲憊': 3,
            '一般': 4,
            '完全消除': 5
        },
        '訓練意願': {
            '沒有訓練': 1,
            '不願意': 2,
            '不怎麼願意': 3,
            '一般': 4,
            '非常高': 5
        },
        '胃口': {
            '沒有進食': 1,
            '因為該進食才吃': 2,
            '不好': 3,
            '一般': 4,
            '很好': 5
        },
        '比賽意願': {
            '全無': 1,
            '低': 2,
            '一般': 3,
            '非常高': 4
        }
    };

    return scores[traitType][feeling] || 0;
}