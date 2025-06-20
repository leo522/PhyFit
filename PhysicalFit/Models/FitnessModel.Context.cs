﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PhFitnessEntities : DbContext
    {
        public PhFitnessEntities()
            : base("name=PhFitnessEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AbilityDetermination> AbilityDetermination { get; set; }
        public virtual DbSet<Appetite> Appetite { get; set; }
        public virtual DbSet<ArcheryRecord> ArcheryRecord { get; set; }
        public virtual DbSet<ArcherySessionRPERecord> ArcherySessionRPERecord { get; set; }
        public virtual DbSet<ArcheryTraining> ArcheryTraining { get; set; }
        public virtual DbSet<AthleteArcheryRPERecord> AthleteArcheryRPERecord { get; set; }
        public virtual DbSet<AthleteArcheryTrainingRecord> AthleteArcheryTrainingRecord { get; set; }
        public virtual DbSet<AthleteGeneralTrainingRecord> AthleteGeneralTrainingRecord { get; set; }
        public virtual DbSet<Athletes> Athletes { get; set; }
        public virtual DbSet<AthleteShootingRecord> AthleteShootingRecord { get; set; }
        public virtual DbSet<AthleteShootingSessionRPERecord> AthleteShootingSessionRPERecord { get; set; }
        public virtual DbSet<BikeRecordDetails> BikeRecordDetails { get; set; }
        public virtual DbSet<Coaches> Coaches { get; set; }
        public virtual DbSet<CompetitionMotivation> CompetitionMotivation { get; set; }
        public virtual DbSet<ComprehensiveHighSchoolList> ComprehensiveHighSchoolList { get; set; }
        public virtual DbSet<DetectionSys> DetectionSys { get; set; }
        public virtual DbSet<DetectionTraining> DetectionTraining { get; set; }
        public virtual DbSet<DetectionTrainingRecord> DetectionTrainingRecord { get; set; }
        public virtual DbSet<FatigueLevel> FatigueLevel { get; set; }
        public virtual DbSet<GeneralHighSchoolList> GeneralHighSchoolList { get; set; }
        public virtual DbSet<GeneralTrainingRecord> GeneralTrainingRecord { get; set; }
        public virtual DbSet<IntensityClassification> IntensityClassification { get; set; }
        public virtual DbSet<JuniorHighSchoolList> JuniorHighSchoolList { get; set; }
        public virtual DbSet<MixedStrength> MixedStrength { get; set; }
        public virtual DbSet<MusclePhysical> MusclePhysical { get; set; }
        public virtual DbSet<MuscleStrength> MuscleStrength { get; set; }
        public virtual DbSet<PasswordResetRequests> PasswordResetRequests { get; set; }
        public virtual DbSet<Permissions> Permissions { get; set; }
        public virtual DbSet<PhysicalFitness> PhysicalFitness { get; set; }
        public virtual DbSet<PrimarySchoolList> PrimarySchoolList { get; set; }
        public virtual DbSet<PsychologicalTraitsChart> PsychologicalTraitsChart { get; set; }
        public virtual DbSet<PsychologicalTraitsResults> PsychologicalTraitsResults { get; set; }
        public virtual DbSet<RollerSkatingRecordDetails> RollerSkatingRecordDetails { get; set; }
        public virtual DbSet<RPE> RPE { get; set; }
        public virtual DbSet<SessionRPETrainingRecords> SessionRPETrainingRecords { get; set; }
        public virtual DbSet<SessionTrainingRecords> SessionTrainingRecords { get; set; }
        public virtual DbSet<ShootingRecord> ShootingRecord { get; set; }
        public virtual DbSet<ShootingTraining> ShootingTraining { get; set; }
        public virtual DbSet<ShottingItems> ShottingItems { get; set; }
        public virtual DbSet<ShottingSessionRPERecord> ShottingSessionRPERecord { get; set; }
        public virtual DbSet<SleepQuality> SleepQuality { get; set; }
        public virtual DbSet<SpecializedTraining> SpecializedTraining { get; set; }
        public virtual DbSet<SpecialTechnical> SpecialTechnical { get; set; }
        public virtual DbSet<SpecialTechnicalAction> SpecialTechnicalAction { get; set; }
        public virtual DbSet<SwimmingRecordDetails> SwimmingRecordDetails { get; set; }
        public virtual DbSet<TechnologyCategory> TechnologyCategory { get; set; }
        public virtual DbSet<TrackFieldRecordDetails> TrackFieldRecordDetails { get; set; }
        public virtual DbSet<TrainingItems> TrainingItems { get; set; }
        public virtual DbSet<TrainingMonitoringItems> TrainingMonitoringItems { get; set; }
        public virtual DbSet<TrainingMotivation> TrainingMotivation { get; set; }
        public virtual DbSet<TrainingPurpose> TrainingPurpose { get; set; }
        public virtual DbSet<TrainingTimes> TrainingTimes { get; set; }
        public virtual DbSet<TreadmillRecordDetails> TreadmillRecordDetails { get; set; }
        public virtual DbSet<UserPermissions> UserPermissions { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<SystemLogs> SystemLogs { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<UniversitySchoolList> UniversitySchoolList { get; set; }
        public virtual DbSet<Organization> Organization { get; set; }
        public virtual DbSet<AthleteCoachRelations> AthleteCoachRelations { get; set; }
    }
}
