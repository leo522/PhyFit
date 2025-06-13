using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhysicalFit.Models
{
    public class GeneralTrainingRecordViewModel
    {
        public int ID { get; set; }
        public string TrainingName { get; set; }
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public Nullable<int> AthleteID { get; set; }
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
        public decimal EachTrainingLoad { get; set; }
        public string Source { get; set; }
    }

    public class ArcheryTrainingRecordViewModel
    {
        public int ID { get; set; }
        public DateTime TrainingDate { get; set; }
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public Nullable<int> AthleteID { get; set; }
        public int Poundage { get; set; }
        public int ArrowCount { get; set; }
        public int RPEscore { get; set; }
        public decimal EachTrainingLoad { get; set; }
        public string Source { get; set; }
    }

    public class ShootingTrainingRecordViewModel
    {
        public int ID { get; set; }
        public DateTime TrainingDate { get; set; }
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public Nullable<int> AthleteID { get; set; }
        public string ShootingTool { get; set; }
        public int BulletCount { get; set; }
        public int RPEscore { get; set; }
        public decimal EachTrainingLoad { get; set; }
        public string Source { get; set; }
    }

    public class TrainingRecordViewModel
    {
        public string TrainingItem { get; set; }
        public IEnumerable<GeneralTrainingRecordViewModel> GeneralTrainingRecord { get; set; }
        public IEnumerable<ArcheryTrainingRecordViewModel> ArcheryRecords { get; set; }
        public IEnumerable<ShootingTrainingRecordViewModel> ShootingRecords { get; set; }
        public IEnumerable<DetectionTrainingRecordViewModel> DetectionRecords { get; set; }
        public IEnumerable<PsychologicalViewModel> Psychological { get; set; }
        public string CoachName { get; set; }
        public string AthleteName { get; set; }
    }

    public class DetectionTrainingRecordViewModel
    {
        public int ID { get; set; }
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
        public double RollerSkill { get; set; }
    }

    public class PsychologicalViewModel
    {
        public List<string> Dates { get; set; }
        public List<int> UserID { get; set; }
        public List<string> TraitsStatuses { get; set; }
        public List<int> SleepQualityScores { get; set; }
        public List<int> FatigueScores { get; set; }
        public List<int> TrainingWillingnessScores { get; set; }
        public List<int> AppetiteScores { get; set; }
        public List<int> CompetitionWillingnessScores { get; set; }
    }
}