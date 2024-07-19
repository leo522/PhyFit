using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
}