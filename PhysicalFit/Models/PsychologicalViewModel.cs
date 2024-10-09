using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class PsychologicalViewModel
    {
        public List<string> Dates { get; set; } // 日期
        public List<string> TraitsStatuses { get; set; } // 心理特質狀態名稱
        public List<int> AppetiteScores { get; set; } // 感受分數
    }
}