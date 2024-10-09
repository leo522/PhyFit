using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class CombinedViewModel
    {
        public TrainingRecordViewModel TrainingRecord { get; set; }
        public PsychologicalViewModel PsychologicalRecord { get; set; }
    }
}