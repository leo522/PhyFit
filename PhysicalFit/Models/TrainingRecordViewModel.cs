using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class GeneralTrainingRecordViewModel
    {
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public DateTime TrainingDate { get; set; }
        public string TrainingClassName { get; set; }
        public string AthleteName { get; set; }
        public string TrainingItem { get; set; }
        public string ActionName { get; set; }
        public string TrainingParts { get; set; }
        public string TrainingType { get; set; }
        public string TrainingOther { get; set; }
        public string TrainingTime { get; set; }
        public int RPEscore { get; set; }
        public int EachTrainingLoad { get; set; }
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
        public IEnumerable<GeneralTrainingRecordViewModel> GeneralTrainingRecord { get; set; }
        public IEnumerable<ArcheryTrainingRecordViewModel> ArcheryRecords { get; set; }
        public IEnumerable<ShootingTrainingRecordViewModel> ShootingRecords { get; set; }
        public IEnumerable<DetectionTrainingRecordViewModel> DetectionRecords { get; set; }
    }

    public class DetectionTrainingRecordViewModel
    {
        public DateTime TrainingDate { get; set; }
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public string DetectionItem { get; set; }
        public string SportItem { get; set; }
        public double CriticalSpeed { get; set; }
        public double MaxAnaerobicWork { get; set; }
        public double TrainingVolume { get; set; }
        public double TrainingPrescription { get; set; }
        public double CoefficientOfDetermination { get; set; }
    }

}