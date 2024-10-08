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
    
    public partial class ShottingSessionRPERecord
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ShottingSessionRPERecord()
        {
            this.ShootingRecord = new HashSet<ShootingRecord>();
        }
    
        public int ID { get; set; }
        public string Coach { get; set; }
        public Nullable<int> CoachID { get; set; }
        public string Athlete { get; set; }
        public Nullable<int> AthleteID { get; set; }
        public System.DateTime TrainingDate { get; set; }
        public string ShootingTool { get; set; }
        public Nullable<int> BulletCount { get; set; }
        public Nullable<int> RPEScore { get; set; }
        public Nullable<int> EachTrainingLoad { get; set; }
        public Nullable<int> DailyTrainingLoad { get; set; }
        public System.DateTime CreatedDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShootingRecord> ShootingRecord { get; set; }
    }
}
