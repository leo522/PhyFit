using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace PhysicalFit.Models
{
    public class TrainingPurposeModel
    {
        public int pk { get; set; }
        public string ItemNumber { get; set; }
        public string TrainingObject { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateUser { get; set; }
        public bool IsActive { get; set; }
    }

    public class RPEModel //RPE項目
    {
        public int Id { get; set; }
        public string ItemNumber { get; set; }
        public int Score { get; set; }
        public string Description { get; set; }
        public string Explanation { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedUser { get; set; }
    }

    public class TrainingMonitoringItem //訓練監控項目
    {
        public int Id { get; set; }
        public string TrainingItem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ShottingItem //射擊用具項目
    {
        public int Id { get; set; }
        public string GunsItem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DetectionSysItem //檢測系統項目
    {
        public int Id { get; set; }
        public string DetectionItem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TrainingData //訓練量
    {
        public double TrainingTime { get; set; }
        public double RPEScore { get; set; }
        public DateTime Date { get; set; } //日期屬性
    }

    public class TrainingRecord
    {
        public double TrainingTime { get; set; }
        public double RPEScore { get; set; }
        public DateTime Date { get; set; } //日期屬性
    }

    public class SessionRPETrainingRecordsModel
    {
        public int Id { get; set; }
        public string UserAccount { get; set; }
        public string DifficultyCategory { get; set; }
        public string TrainingItem { get; set; }
        public string TrainingTime { get; set; }
        public int RPEscore { get; set; }
        public int TrainingLoad { get; set; }
        public int DailyTrainingLoad { get; set; }
        public int WeeklyTrainingLoad { get; set; }
        public int TrainingHomogeneity { get; set; }
        public int TrainingTension { get; set; }
        public int WeeklyTrainingChange { get; set; }
        public int ShortToLongTermTrainingLoadRatio { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TrainingActionName { get; set; }
        public DateTime TrainingDate { get; set; }
    }

    public class DistanceDetailModel //partialView檢測系統
    {
        public string Distance { get; set; }
        public string ExhaustionTime { get; set; }
        public string Speed { get; set; }
    }

    public class ResetPasswordViewModel //重置密碼
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    public class GeneralRecord
    {
        public int CoachID { get; set; }
        public int AthleteID { get; set; }
        public string Coach { get; set; }
        public string Athlete { get; set; }
        public string TariningClassName { get; set; }
        public DateTime TrainingDate { get; set; }
        public string TrainingItem { get; set; }
        public string ActionName { get; set; }
        public string TrainingParts { get; set; }
        public string TrainingType { get; set; }
        public string TrainingOther { get; set; }
        public string TrainingTime { get; set; }
        public int RPEscore { get; set; }
        public int DailyTrainingLoad { get; set; }
    }
}