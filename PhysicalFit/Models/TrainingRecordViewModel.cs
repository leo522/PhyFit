using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class RPETrainingRecordViewModel
    {
        public DateTime TrainingDate { get; set; }
        public string AthleteName { get; set; }
        public int RPELevel { get; set; }
    }

    public class ArcheryTrainingRecordViewModel
    {
        public DateTime TrainingDate { get; set; }
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public int Poundage { get; set; }
        public int ArrowCount { get; set; }
        public int RPEscore { get; set; }
        public int EachTrainingLoad { get; set; }
    }

    public class ShootingTrainingRecordViewModel
    {
        public DateTime TrainingDate { get; set; }
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public string ShootingTool { get; set; }
        public int BulletCount { get; set; }
        public int RPEscore { get; set; }
        public int EachTrainingLoad { get; set; }
    }

    public class TrainingRecordViewModel
    {
        public string TrainingItem { get; set; }
        public IEnumerable<RPETrainingRecordViewModel> RPERecords { get; set; }
        public IEnumerable<ArcheryTrainingRecordViewModel> ArcheryRecords { get; set; }
        public IEnumerable<ShootingTrainingRecordViewModel> ShootingRecords { get; set; }
    }
}