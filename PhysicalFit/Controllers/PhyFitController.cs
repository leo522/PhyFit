using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using System.Data.Entity;

namespace PhysicalFit.Controllers
{
    public class PhyFitController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        #region 訓練監控主視圖
        public ActionResult dashboard()
        {
            try
            {
                if (!User.Identity.IsAuthenticated || Session["UserID"] == null)
                {
                    LogHelper.LogToDb("Dashboard", "尚未登入，導向登入頁", "WARN");

                    return RedirectToAction("Login", "Account");
                }

                if (User.Identity.IsAuthenticated)
                {
                    var userRole = Session["UserRole"]?.ToString();
                    ViewBag.UserRole = userRole;

                    string userName = User.Identity.Name;
                    var user = _db.Users.FirstOrDefault(u => u.Name == userName);

                    if (user != null)
                    {
                        if (userRole == "Athlete" && user.AthleteID.HasValue)
                        {
                            var athlete = _db.Athletes.FirstOrDefault(a => a.ID == user.AthleteID.Value);
                            if (athlete != null)
                            {
                                ViewBag.AthleteName = athlete.AthleteName;
                                ViewBag.AthleteID = athlete.ID.ToString();
                                var coachNames = _db.AthleteCoachRelations
                                    .Where(r => r.AthleteID == athlete.ID)
                                    .Select(r => r.Coaches.CoachName)
                                    .ToList();

                                ViewBag.CoachName = coachNames.Any()
                                    ? string.Join("、", coachNames)
                                    : "未設定教練";

                                LogHelper.LogToDb("Dashboard", $"學生 {athlete.AthleteName} 開啟訓練監控主頁");
                            }
                        }
                        else if (userRole == "Coach" && user.CoachID.HasValue)
                        {
                            var coach = _db.Coaches.FirstOrDefault(c => c.ID == user.CoachID.Value);
                            ViewBag.CoachName = coach?.CoachName ?? "未設定教練";
                            ViewBag.CoachID = coach?.ID.ToString() ?? string.Empty;
                            ViewBag.Athletes = _db.AthleteCoachRelations
                                .Where(r => r.CoachID == user.CoachID.Value)
                                .Select(r => r.Athletes)
                                .Distinct()
                                .ToList();


                            LogHelper.LogToDb("Dashboard", $"教練 {coach?.CoachName} 開啟訓練監控主頁");
                        }

                        ViewBag.MonitoringItems = GetTrainingMonitoringItems();
                        ViewBag.Description = GetTrainingItem();
                        ViewBag.TrainingPurposes = GetIntensityClassification();
                        ViewBag.TrainingTimes = GetTrainingTimes();
                        ViewBag.RPEScore = GetRPE();
                        ViewBag.GunItem = GetGunsItems();
                        ViewBag.DetectionSport = GetSpoetsItem();
                        ViewBag.Coaches = _db.Coaches.Where(c => c.IsActive).ToList();
                        ViewBag.SpecialTechnical = GetSpecialTechnical();
                        ViewBag.SpecialTechnicalAction = GetSpecialTechnicalAction();
                        ViewBag.MuscleStrength = GetMuscleStrength();
                        ViewBag.PhysicalFitness = GetPhysicalFitness();
                        ViewBag.Psychological = PsychologicalTraits();
                        ViewBag.PsychologicalFeelings = GetPsyFeelings();

                        var records = _db.SessionRPETrainingRecords.ToList();

                        var model = records.Select(r => new SessionRPETrainingRecordsModel
                        {
                            TrainingItem = r.TrainingItem,
                            RPEscore = r.RPEscore.GetValueOrDefault(),
                            TrainingLoad = r.TrainingLoad ?? 0,
                            DailyTrainingLoad = r.DailyTrainingLoad ?? 0,
                            WeeklyTrainingChange = r.WeeklyTrainingChange ?? 0,
                            TrainingHomogeneity = r.TrainingHomogeneity ?? 0,
                            TrainingTension = r.TrainingTension ?? 0,
                            ShortToLongTermTrainingLoadRatio = r.ShortToLongTermTrainingLoadRatio ?? 0,
                        }).ToList();

                        ViewBag.SessionTrainingRecords = model;

                        return View();
                    }
                    else
                    {
                        LogHelper.LogToDb("Dashboard", $"找不到使用者：{userName}", "WARN");

                        return RedirectToAction("Login", "Account");
                    }
                }
                else
                {
                    LogHelper.LogToDb("Dashboard", "使用者未通過身份驗證", "WARN");

                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogToDb("Dashboard", "主視圖載入發生例外", "ERROR", ex);

                throw ex;
            }
        }
        #endregion

        #region 一般監控訓練動態表單

        public ActionResult UpdateTableHeaders(string trainingItem)
        {
            try
            {
                ViewBag.MonitoringItems = GetTrainingMonitoringItems();
                ViewBag.Description = GetTrainingItem();
                ViewBag.TrainingPurposes = GetIntensityClassification();
                ViewBag.TrainingTimes = GetTrainingTimes();
                ViewBag.RPEScore = GetRPE();
                ViewBag.GunItem = GetGunsItems();
                ViewBag.DetectionSport = GetSpoetsItem();
                ViewBag.Coaches = _db.Coaches.Where(c => c.IsActive).ToList();
                ViewBag.SpecialTechnical = GetSpecialTechnical();
                ViewBag.SpecialTechnicalAction = GetSpecialTechnicalAction();
                ViewBag.MuscleStrength = GetMuscleStrength();
                ViewBag.PhysicalFitness = GetPhysicalFitness();

                LogHelper.LogToDb("UpdateTableHeaders", $"載入訓練項目表單成功：{trainingItem}");

                return PartialView("_SpecialTechnical", trainingItem);
            }
            catch (Exception ex)
            {
                LogHelper.LogToDb("UpdateTableHeaders", $"載入訓練項目表單失敗：{trainingItem}", "ERROR", ex);

                return new HttpStatusCodeResult(500, ex.Message);
            }
        }
        #endregion

        #region 日期
        public ActionResult GetDateData()
        {
            try
            {
                var today = DateTime.Today;
                var years = Enumerable.Range(today.Year - 100, 101).Reverse().ToList();
                var months = Enumerable.Range(1, 12).ToList();
                var days = Enumerable.Range(1, DateTime.DaysInMonth(today.Year, today.Month)).ToList();

                var dateData = new
                {
                    CurrentYear = today.Year,
                    CurrentMonth = today.Month,
                    CurrentDay = today.Day,
                    Years = years,
                    Months = months,
                    Days = days
                };

                return Json(dateData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return View("Error");
            }
        }
        #endregion

        #region 訓練監控項目選擇
        public List<string> GetTrainingMonitoringItems()
        {
            var dto = (from ti in _db.TrainingMonitoringItems
                       select ti.TrainingItem).ToList();

            var userRole = Session["UserRole"]?.ToString();

            if (userRole == "Athlete")
            {
                dto.Remove("檢測系統");
            }
            else if (userRole == "Coach") 
            {
                dto.Remove("心理特質與食慾圖量表");
            }

            return dto;
        }
        #endregion

        #region 訓練用途
        public List<string> GetTrainingPurposes()
        {
            var dto = (from tp in _db.TrainingPurpose
                       select tp.TrainingObject).ToList();
            return dto;
        }
        #endregion

        #region 能力測定
        public List<string> GetAbilityDetermination()
        {
            var dto = (from ad in _db.AbilityDetermination
                       select ad.DeterminationMethod).ToList();

            return dto;
        }
        #endregion

        #region 訓練衝量監控(session-RPE)
        public List<string> GetTrainingItem()
        {
            var dto = (from tp in _db.TrainingItems
                       select tp.TrainingName).ToList();

            return dto;
        }
        #endregion

        #region 難度分類
        public List<string> GetIntensityClassification()
        {
            var dto = (from tp in _db.IntensityClassification
                       select tp.Intensity).ToList();

            return dto;
        }
        #endregion

        #region 訓練時間
        public List<string> GetTrainingTimes()
        {
            var dto = (from tp in _db.TrainingTimes
                       select tp.TrainingTime.ToString()).ToList();
            return dto;
        }
        #endregion

        #region RPE
        public List<RPEModel> GetRPE()
        {
            var dto = (from tp in _db.RPE
                       select new RPEModel
                       {
                           Score = tp.Score,
                           Description = tp.Description,
                           Explanation = tp.Explanation,
                       }).ToList();
            return dto;
        }

        public ActionResult RPEsurvey()
        {
            var rpeList = GetRPE();
            ViewBag.RPEScores = rpeList;

            return PartialView("_RPESurvey");
        }
        #endregion

        #region 專項技術類-項目
        public List<string> GetSpecialTechnical()
        {
            var dto = (from st in _db.SpecialTechnical
                       select st.TechnicalItem).ToList();
            return dto;
        }
        #endregion

        #region 專項技術類-動作名稱
        public List<string> GetSpecialTechnicalAction()
        {
            var dto = (from sta in _db.SpecialTechnicalAction
                       select sta.TechnicalName).ToList();
            return dto;
        }

        public JsonResult GetSpecialTechnicalActions(string technicalItem)
        {
            var actions = _db.SpecialTechnicalAction
                            .Where(sta => sta.SpecialTechnical.TechnicalItem == technicalItem)
                            .Select(sta => sta.TechnicalName)
                            .ToList();

            return Json(actions, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 射擊用具項目
        public List<string> GetGunsItems()
        {
            var dto = (from gu in _db.ShottingItems
                       select gu.GunsItem).ToList();
            return dto;
        }
        #endregion

        #region 射箭訓練衝量
        public ActionResult arrowCaculate()
        {
            return View();
        }
        #endregion

        #region 射擊訓練衝量

        #endregion

        #region 檢測系統_運動項目
        public List<string> GetSpoetsItem()
        {
            var dto = (from Si in _db.DetectionTraining
                       where Si.ItemName != "跑步機" && Si.ItemName != "自由車"   // 暫不顯示跑步機和自由車
                       select Si.ItemName).Distinct().ToList();
            return dto;
        }
        #endregion

        #region 檢測系統_運動距離
        public ActionResult LoadDistanceDetails(string itemName)
        {
            try
            {
                var distances = (from Si in _db.DetectionTraining
                                 where Si.ItemName == itemName
                                 select Si.Distance).FirstOrDefault();

                if (!string.IsNullOrEmpty(distances))
                {
                    var distanceList = distances.Split('/').ToList();
                    if (itemName == "田徑場")
                    {
                        return PartialView("_Athletic", distanceList);
                    }
                    else if (itemName == "跑步機")
                    {
                        return PartialView("_Treadmill", distanceList);
                    }
                    else if (itemName == "游泳")
                    {
                        return PartialView("_Swimming", distanceList);
                    }
                    else if (itemName == "自由車")
                    {
                        return PartialView("_RoadBicycle", distanceList);
                    }
                    else if (itemName == "滑輪溜冰")
                    {
                        return PartialView("_RollerSkating", distanceList);
                    }
                }

                return PartialView("_Athletic", new List<string>());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 肌力類-訓練部位
        public List<string> GetMuscleStrength()
        {
            var dto = (from ms in _db.MuscleStrength
                       select ms.TrainingPart).ToList();
            return dto;
        }
        #endregion

        #region 體能類-訓練類型
        public List<string> GetPhysicalFitness()
        {
            var dto = (from pf in _db.PhysicalFitness
                       select pf.FitnessItem).ToList();
            return dto;
        }
        #endregion

        #region sessionRPE指標計算TL結果
        [HttpPost]
        public JsonResult CalculateRPE(double trainingTime, double rpeScore)
        {
            double calculationResult = trainingTime * rpeScore;
            double dailyResult = calculationResult;

            return Json(new { calculationResult = calculationResult, dailyResult = dailyResult });
        }
        #endregion

        #region sessionRPE指標計算每日訓練量結果
        public static class InMemoryData
        {
            public static List<TrainingData> TrainingDataList = new List<TrainingData>();
        }

        [HttpPost]
        public JsonResult CalculateDailyResult(double trainingTime, double rpeScore, DateTime date)
        {
            DateTime today = DateTime.Today;

            if (date.Date != today.Date)
            {
                return Json(new { dailyResult = "錯誤: 日期不符" });
            }

            var dailyTrainingData = GetDailyTrainingData(today);

            double totalDailyResult = 0;

            foreach (var item in dailyTrainingData)
            {
                double time = item.TrainingTime;
                double score = item.RPEScore;
                totalDailyResult += time * score;
            }

            double currentResult = trainingTime * rpeScore;

            totalDailyResult += currentResult;

            return Json(new { dailyResult = totalDailyResult });
        }

        private List<TrainingData> GetDailyTrainingData(DateTime date)
        {
            return InMemoryData.TrainingDataList
                      .Where(td => td.Date.Date == date.Date).ToList();
        }
        #endregion

        #region 檢查一般訓練紀錄是否有相同資料
        [HttpPost]
        public JsonResult CheckDuplicateGeneralTrainingRecord(int athleteId, DateTime trainingDate)
        {
            var duplicate = _db.GeneralTrainingRecord.FirstOrDefault(r =>
                r.AthleteID == athleteId &&
                r.TrainingDate.HasValue &&
                DbFunctions.TruncateTime(r.TrainingDate) == trainingDate.Date &&
                r.TrainingDate.Value.Hour == trainingDate.Hour &&
                r.TrainingDate.Value.Minute == trainingDate.Minute);

            return Json(new { exists = duplicate != null });
        }
        #endregion

        #region 檢查射箭紀錄是否有相同資料
        [HttpPost]
        public JsonResult CheckDuplicateCoachArcheryRecord(int athleteId, DateTime trainingDate)
        {
            var duplicate = _db.ArcheryRecord.FirstOrDefault(r =>
                r.AthleteID == athleteId &&
                r.TrainingDate.HasValue &&
                DbFunctions.TruncateTime(r.TrainingDate) == trainingDate.Date &&
                r.TrainingDate.Value.Hour == trainingDate.Hour &&
                r.TrainingDate.Value.Minute == trainingDate.Minute);

            return Json(new { exists = duplicate != null });
        }
        #endregion

        #region 檢查射擊紀錄是否有相同資料
        [HttpPost]
        public JsonResult CheckDuplicateCoachShootingRecord(int athleteId, DateTime trainingDate)
        {
            var duplicate = _db.ShootingRecord.FirstOrDefault(r =>
                r.AthleteID == athleteId &&
                r.TrainingDate.HasValue &&
                DbFunctions.TruncateTime(r.TrainingDate) == trainingDate.Date &&
                r.TrainingDate.Value.Hour == trainingDate.Hour &&
                r.TrainingDate.Value.Minute == trainingDate.Minute);

            return Json(new { exists = duplicate != null });
        }
        #endregion

        #region 儲存一般訓練紀錄-教練
        public ActionResult SaveGeneralTrainingRecord(List<GeneralTrainingRecord> records)
        {
            try
            {
                if (Session["UserRole"]?.ToString() != "Coach")
                {
                    return Json(new { success = false, message = "請確認是否為教練身份。" });
                }

                foreach (var record in records)
                {
                    if (string.IsNullOrEmpty(record.TrainingClassName))
                    {
                        return Json(new { success = false, message = "訓練項目資料不完整，無法儲存。" });
                    }

                    // 根據 AthleteID 與 TrainingDate 判斷是否有重複紀錄
                    var date = record.TrainingDate.Value;

                    var duplicate = _db.GeneralTrainingRecord.FirstOrDefault(r =>
                        r.AthleteID == record.AthleteID &&
                        r.TrainingDate.HasValue &&
                        DbFunctions.TruncateTime(r.TrainingDate) == DbFunctions.TruncateTime(date) &&
                        r.TrainingDate.Value.Hour == date.Hour &&
                        r.TrainingDate.Value.Minute == date.Minute);

                    if (duplicate != null)
                    {
                        _db.GeneralTrainingRecord.Remove(duplicate);
                    }

                    switch (record.TrainingClassName)
                    {
                        case "專項技術類":
                            record.TrainingParts = null;
                            record.TrainingType = null;
                            break;

                        case "混合肌力體能類":
                            record.TrainingItem = null;
                            record.ActionName = null;
                            record.TrainingParts = null;
                            record.TrainingType = null;
                            break;

                        case "肌力類":
                            record.TrainingItem = null;
                            record.ActionName = null;
                            record.TrainingType = null;
                            break;

                        case "體能類":
                            record.TrainingItem = null;
                            record.ActionName = null;
                            record.TrainingParts = null;
                            break;
                    }

                    if (string.IsNullOrEmpty(record.TrainingClassName))
                    {
                        return Json(new { success = false, message = "訓練項目資料不完整，無法儲存。" });
                    }

                    _db.GeneralTrainingRecord.Add(record);
                }

                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 儲存射箭訓練紀錄-教練
        public ActionResult SaveArcheryRecord(List<ArcheryRecord> records)
        {
            try
            {
                if (Session["UserRole"]?.ToString() != "Coach")
                {
                    return Json(new { success = false, message = "請確認是否為教練身份。" });
                }

                for (int i = 0; i < records.Count; i++)
                {
                    var sessionRecord = records[i];
                    var correspondingRecord = records[i];

                    if (correspondingRecord.AthleteID.HasValue && correspondingRecord.TrainingDate.HasValue)
                    {
                        var athleteID = correspondingRecord.AthleteID.Value;
                        var date = correspondingRecord.TrainingDate.Value;

                        // 刪除舊的 Training 記錄
                        var duplicateTraining = _db.ArcheryRecord.FirstOrDefault(r =>
                            r.AthleteID == athleteID &&
                            r.TrainingDate.HasValue &&
                            DbFunctions.TruncateTime(r.TrainingDate) == DbFunctions.TruncateTime(date) &&
                            r.TrainingDate.Value.Hour == date.Hour &&
                            r.TrainingDate.Value.Minute == date.Minute);

                        if (duplicateTraining != null)
                        {
                            _db.ArcheryRecord.Remove(duplicateTraining);
                            _db.SaveChanges();
                        }
                    }

                    // 儲存新的 Training 資料
                    _db.ArcheryRecord.Add(correspondingRecord);
                }

                _db.SaveChanges();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region 儲存射擊訓練紀錄-教練
        public ActionResult SaveShootingRecord(List<ShootingRecord> records)
        {
            try
            {
                if (Session["UserRole"]?.ToString() != "Coach")
                {
                    return Json(new { success = false, message = "請確認是否為教練身份。" });
                }

                for (int i = 0; i < records.Count; i++)
                {
                    var sessionRecord = records[i];
                    var correspondingRecord = records[i];

                    if (correspondingRecord.AthleteID.HasValue && correspondingRecord.TrainingDate.HasValue)
                    {
                        var athleteID = correspondingRecord.AthleteID.Value;
                        var date = correspondingRecord.TrainingDate.Value;

                        // 刪除舊的 Training 記錄
                        var duplicateTraining = _db.ShootingRecord.FirstOrDefault(r =>
                            r.AthleteID == athleteID &&
                            r.TrainingDate.HasValue &&
                            DbFunctions.TruncateTime(r.TrainingDate) == DbFunctions.TruncateTime(date) &&
                            r.TrainingDate.Value.Hour == date.Hour &&
                            r.TrainingDate.Value.Minute == date.Minute);

                        if (duplicateTraining != null)
                        {
                            _db.ShootingRecord.Remove(duplicateTraining);
                            _db.SaveChanges();
                        }
                    }

                    // 儲存新的 Training 資料
                    _db.ShootingRecord.Add(correspondingRecord);
                }

                _db.SaveChanges();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 運動員資訊
        public ActionResult athletes(int coachId)
        {
            var athletes = _db.Athletes
                              .Where(a => a.CoachID == coachId)
                              .Select(a => a.AthleteName).ToList();
            return PartialView("_Athletes", athletes);
        }
        #endregion

        #region 建立運動員資訊
        public ActionResult createAthletes()
        {
            return View();
        }

        [HttpPost]

        public ActionResult CreateAthletes(Athletes model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("CreateAthletes");
            }

            return View(model);
        }
        #endregion

        #region 訓練處方計算
        public ActionResult RedirectToPrescription()
        {
            return RedirectToAction("Prescription", "TrainingPrescription");
        }

        #endregion

        #region 測試讀取訓練資料
        public ActionResult LoadGeneralRecordPartial()
        {
            try
            {
                var records = _db.GeneralTrainingRecord
                 .Select(gr => new GeneralRecord
                 {
                     CoachID = gr.CoachID ?? 0,
                     AthleteID = gr.AthleteID ?? 0,
                     Coach = gr.Coach,
                     Athlete = gr.Athlete,
                     TariningClassName = gr.TrainingClassName,
                     TrainingDate = gr.TrainingDate ?? DateTime.MinValue,
                     TrainingItem = gr.TrainingItem,
                     ActionName = gr.ActionName,
                     TrainingParts = gr.TrainingParts,
                     TrainingType = gr.TrainingType,
                     TrainingOther = gr.TrainingOther,
                     TrainingTime = gr.TrainingTime,
                     RPEscore = gr.RPEscore ?? 0,
                     DailyTrainingLoad = gr.DailyTrainingLoad ?? 0
                 }).ToList();

                return PartialView("_GeneralRecord", records);
            }
            catch (Exception ex)
            {
                Console.WriteLine("加載紀錄失敗: " + ex.Message);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "無法加載紀錄");
            }
        }
        #endregion

        #region 儲存檢測系統訓練紀錄
        [HttpPost]
        public ActionResult SaveTrackFieldRecord(SaveTrackFieldRecordModel model)       
        {
            try
            {
                
                int? coachId = AuthHelper.GetCurrentCoachId();
                int? athleteId = AuthHelper.GetCurrentAthleteId();

                if (!coachId.HasValue || !athleteId.HasValue)
                {
                    return Json(new { success = false, message = "使用者未登入或ID錯誤" });
                }
                
                var coach = _db.Coaches.SingleOrDefault(c => c.ID == coachId.Value);
                var athlete = _db.Athletes.SingleOrDefault(a => a.ID == athleteId.Value);

                if (coach == null)
                {
                    return Json(new { success = false, message = "無效的教練ID" });
                }

                if (athlete == null)
                {
                    return Json(new { success = false, message = "無效的運動員ID" });
                }

                var detectionRecord = new DetectionTrainingRecord
                {
                    Coach = model.coach,
                    CoachID = coach.ID,
                    Athlete = model.athlete,
                    AthleteID = athlete.ID,
                    DetectionItem = "有/無氧代謝能力測定",
                    SportItem = model.SportItem,
                    TrainingDate = DateTime.Parse(model.DetectionDate),
                    CriticalSpeed = model.CriticalSpeed,
                    MaxAnaerobicWork = model.AnaerobicPower,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                };
                _db.DetectionTrainingRecord.Add(detectionRecord);
                _db.SaveChanges();

                int detectionRecordId = detectionRecord.ID;

                if (model.SportItem == "跑步機")
                {
                    for (int i = 0; i < model.IntenPercen.Count; i++)
                    {
                        var dtos = new TreadmillRecordDetails
                        {
                            DetectionTrainingRecordId = detectionRecordId,
                            IntenPercen = (model.IntenPercen[i]),
                            MaxRunningSpeed = model.MaxRunningSpeed,
                            ForceDuration = int.Parse(model.ForceDurations[i]),
                            Speed = float.Parse(model.Speeds[i]),
                            CreatedDate = DateTime.Now,
                            TrainingDateTime = DateTime.Parse(model.DetectionDate),
                        };
                        _db.TreadmillRecordDetails.Add(dtos);
                    }
                }
                else if (model.SportItem == "田徑場")
                {
                    for (int i = 0; i < model.Distances.Count; i++)
                    {
                        var dtos = new TrackFieldRecordDetails
                        {
                            DetectionTrainingRecordId = detectionRecordId,
                            Distance = (model.Distances[i]),
                            ForceDuration = int.Parse(model.ForceDurations[i]),
                            Speed = float.Parse(model.Speeds[i]),
                            CreatedDate = DateTime.Now,
                            TrainingDateTime = DateTime.Parse(model.DetectionDate),
                        };
                        _db.TrackFieldRecordDetails.Add(dtos);
                    }
                }
                else if (model.SportItem == "游泳")
                {
                    for (int i = 0; i < model.Distances.Count; i++)
                    {
                        var dtos = new SwimmingRecordDetails
                        {
                            DetectionTrainingRecordId = detectionRecordId,
                            Distance = (model.Distances[i]),
                            ForceDuration = int.Parse(model.ForceDurations[i]),
                            Speed = float.Parse(model.Speeds[i]),
                            CreatedDate = DateTime.Now,
                            TrainingDateTime = DateTime.Parse(model.DetectionDate),
                        };
                        _db.SwimmingRecordDetails.Add(dtos);
                    }
                }
                else if (model.SportItem == "自由車")
                {
                    for (int i = 0; i < model.IntenPercen.Count; i++)
                    {
                        var dtos = new BikeRecordDetails
                        {
                            DetectionTrainingRecordId = detectionRecordId,
                            IntenPercen = (model.IntenPercen[i]),
                            MaxPower = model.MaxPower,
                            ForceDuration = int.Parse(model.ForceDurations[i]),
                            Speed = float.Parse(model.Speeds[i]),
                            CreatedDate = DateTime.Now,
                            TrainingDateTime = DateTime.Parse(model.DetectionDate),
                        };
                        _db.BikeRecordDetails.Add(dtos);
                    }
                }
                else if (model.SportItem == "滑輪溜冰") 
                {
                    for (int i = 0; i < model.Distances.Count; i++)
                    {
                        var dtos = new RollerSkatingRecordDetails
                        {
                            DetectionTrainingRecordId = detectionRecordId,
                            Distance = (model.Distances[i]),
                            ForceDuration = int.Parse(model.ForceDurations[i]),
                            Speed = float.Parse(model.Speeds[i]),
                            CreatedDate = DateTime.Now,
                            TrainingDateTime = DateTime.Parse(model.DetectionDate),
                        };
                        _db.RollerSkatingRecordDetails.Add(dtos);
                    }
                }
                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 心理特質與食慾圖標題
        public List<string> PsychologicalTraits()
        {
            var dto = (from pt in _db.PsychologicalTraitsChart select pt.Item).ToList();
            return dto;
        }
        #endregion

        #region 睡眠品質
        public List<string> SleeP()
        {
            var dto = (from sl in _db.SleepQuality select sl.Quality).ToList();
            return dto;
        }
        #endregion

        #region 疲憊程度
        public List<string> FatiguE()
        {
            var dto = (from fa in _db.FatigueLevel select fa.Fatigue).ToList();
            return dto;
        }
        #endregion

        #region 訓練意願
        public List<string> TrainingMotivatioN()
        {
            var dto = (from tr in _db.TrainingMotivation select tr.TrainingWillingness).ToList();
            return dto;
        }
        #endregion

        #region 胃口
        public List<string> AppetitE()
        {
            var dto = (from ap in _db.Appetite select ap.AppetiteStatus).ToList();
            return dto;
        }
        #endregion

        #region 比賽意願
        public List<string> CompetitioN()
        {
            var dto = (from co in _db.CompetitionMotivation select co.CompetitionWillingness).ToList();
            return dto;
        }
        #endregion

        #region 心理特質感受
        public Dictionary<string, List<string>> GetPsyFeelings()
        {
            try
            {
                var psychologicalWithFeelings = new Dictionary<string, List<string>>();

                var sleepQuality = (from sl in _db.SleepQuality select sl.Quality).ToList();
                psychologicalWithFeelings.Add("睡眠品質", sleepQuality);

                var fatigueLevel = (from fa in _db.FatigueLevel select fa.Fatigue).ToList();
                psychologicalWithFeelings.Add("疲憊程度", fatigueLevel);

                var trainingMotivation = (from tr in _db.TrainingMotivation select tr.TrainingWillingness).ToList();
                psychologicalWithFeelings.Add("訓練意願", trainingMotivation);

                var appetite = (from ap in _db.Appetite select ap.AppetiteStatus).ToList();
                psychologicalWithFeelings.Add("胃口", appetite);

                var competitionMotivation = (from co in _db.CompetitionMotivation select co.CompetitionWillingness).ToList();
                psychologicalWithFeelings.Add("比賽意願", competitionMotivation);

                return psychologicalWithFeelings;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Dictionary<string, List<string>>();
            }
        }
        #endregion

        #region 心理特質感受-存檔
        [HttpPost]
        public JsonResult SubmitTraits(List<PsychologicalTraitViewModel> traits)
        {
            try
            {
                if (traits == null || !traits.Any())
                {
                    return Json(new { success = false, message = "沒有接收到資料" });
                }

                foreach (var trait in traits)
                {

                    var user = _db.Users.FirstOrDefault(u => u.UID == trait.UserID); 

                    if (user == null)
                    {
                        return Json(new { success = false, message = $"UserID {trait.UserID} 用戶不存在，請確認用戶資料。" });
                    }

                    var psychologicalResult = new PsychologicalTraitsResults
                    {
                        UserID = trait.UserID,
                        PsychologicalDate = trait.PsychologicalDate,
                        Trait = trait.Trait,
                        Feeling = trait.Feeling,
                        Score = trait.Score,
                        CreatedAt = DateTime.Now
                    };
                    _db.PsychologicalTraitsResults.Add(psychologicalResult);
                }

                _db.SaveChanges();

                return Json(new { success = true, message = "資料儲存成功"});
            }
             catch (Exception ex)
            {
                return Json(new { success = false, message = "儲存過程中發生錯誤", error = ex.Message });
            }
        }
        #endregion
    }
}