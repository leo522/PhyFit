//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace PhysicalFit.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SessionTrainingRecords
    {
        public int ID { get; set; }
        public int CoachID { get; set; }
        public int AthleteID { get; set; }
        public System.DateTime TrainingDate { get; set; }
        public string ClassName { get; set; }
        public string TrainingItem { get; set; }
        public string TrainingParts { get; set; }
        public string TrainingType { get; set; }
        public System.DateTime TrainingTime { get; set; }
        public int RPEScore { get; set; }
        public int TrainingLoad { get; set; }
        public string OtherItem { get; set; }
        public string CreatedUser { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        public virtual Athletes Athletes { get; set; }
        public virtual Coaches Coaches { get; set; }
    }
}