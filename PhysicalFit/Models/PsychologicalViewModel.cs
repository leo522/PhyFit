using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class PsychologicalViewModel
    {
        public List<string> Dates { get; set; } // 日期
        public List<int> UserID { get; set; } //運動員ID
        public List<string> TraitsStatuses { get; set; } // 心理特質狀態名稱
        public List<int> SleepQualityScores { get; set; } // 睡眠品質分數
        public List<int> FatigueScores { get; set; } // 疲憊程度分數
        public List<int> TrainingWillingnessScores { get; set; } // 訓練意願分數
        public List<int> AppetiteScores { get; set; } // 胃口分數
        public List<int> CompetitionWillingnessScores { get; set; } // 比賽意願分數
    }
}