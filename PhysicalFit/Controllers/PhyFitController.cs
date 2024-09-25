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

namespace PhysicalFit.Controllers
{
    public class PhyFitController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities(); //資料庫

        #region 首頁-測試
        //public ActionResult Home()
        //{
        //    Session["ReturnUrl"] = Request.Url.ToString();

        //    ViewBag.MonitoringItems = GetTrainingMonitoringItems(); //訓練監控項目選擇
        //    ViewBag.Description = GetTrainingItem(); //訓練衝量監控(session-RPE)
        //    ViewBag.TrainingPurposes = GetIntensityClassification(); //訓練強度
        //    ViewBag.TrainingTimes = GetTrainingTimes();//訓練時間
        //    ViewBag.RPEScore = GetRPE();//RPE量表
        //    ViewBag.GunItem = GetGunsItems(); //射擊用具項目
        //    ViewBag.DetectionSport = GetSpoetsItem(); //檢測系統_運動項目
        //    //ViewBag.SpoetsDistance = GetSpoetsDistance(); //檢測系統_距離
        //    ViewBag.Coaches = _db.Coaches.Where(c => c.IsActive).ToList(); //教練資訊

        //    var records = _db.SessionRPETrainingRecords.ToList();

        //    var model = records.Select(r => new SessionRPETrainingRecordsModel
        //    {
        //        TrainingItem = r.TrainingItem, //訓練名稱
        //        RPEscore = r.RPEscore.GetValueOrDefault(), //RPE分數
        //        //TrainingTime = r.TrainingTime, //訓練時間
        //        TrainingLoad = r.TrainingLoad ?? 0, //運動訓練量
        //        DailyTrainingLoad = r.DailyTrainingLoad ?? 0, //每日運動訓練量
        //        WeeklyTrainingChange = r.WeeklyTrainingChange ?? 0, //每週運動訓練量
        //        TrainingHomogeneity = r.TrainingHomogeneity ?? 0, //同質性
        //        TrainingTension = r.TrainingTension ?? 0, //張力值
        //        ShortToLongTermTrainingLoadRatio = r.ShortToLongTermTrainingLoadRatio ?? 0, //短長期
        //    }).ToList();

        //    ViewBag.SessionTrainingRecords = model;

        //    return View();
        //}
        #endregion

        #region 訓練監控主視圖
        public ActionResult dashboard()
        {
            if (User.Identity.IsAuthenticated)
            {
                // 檢查用戶角色
                var userRole = Session["UserRole"]?.ToString();
                ViewBag.UserRole = userRole; // 儲存用戶角色以便在視圖中使用

                string userName = User.Identity.Name; // 獲取當前登入用戶的名稱或其他唯一識別信息
                var user = _db.Users.FirstOrDefault(u => u.Name == userName);

                if (user != null)
                {
                    Athletes athlete = null;
                    List<Athletes> athletesUnderCoach = null; // 這裡我們先定義一個運動員列表

                    //判斷用戶是否為運動員
                    if (user.AthleteID.HasValue)
                    {
                        int athleteId = user.AthleteID.Value;
                        athlete = _db.Athletes.FirstOrDefault(a => a.ID == athleteId);

                        if (athlete != null)
                        {
                            ViewBag.AthleteName = athlete.AthleteName;  // 設置運動員名字
                            ViewBag.AthleteID = athlete.ID;             // 設置運動員ID

                            if (athlete.CoachID.HasValue)
                            {
                                // 查詢教練資料
                                var coach = _db.Coaches.FirstOrDefault(c => c.ID == athlete.CoachID.Value);
                                ViewBag.CoachName = coach?.CoachName ?? "未設定教練";
                                ViewBag.CoachID = coach?.ID; // 設置CoachID
                            }
                        }
                        else
                        {
                            // 當運動員資料不存在時，設置預設值
                            ViewBag.AthleteName = "無運動員資料";
                            ViewBag.AthleteID = 0;
                        }
                    }
                    else if (user.CoachID.HasValue) // 如果用戶是教練
                {
                    int coachId = user.CoachID.Value;
                    var coach = _db.Coaches.FirstOrDefault(c => c.ID == coachId);
                    ViewBag.CoachName = coach?.CoachName ?? "未設定教練";
                    ViewBag.CoachID = coach?.ID; // 設置CoachID

                    // 查詢與該教練相關的運動員
                    athletesUnderCoach = _db.Athletes.Where(a => a.CoachID == coachId).ToList();
                    
                    if (athletesUnderCoach.Any())
                    {
                        athlete = athletesUnderCoach.First();
                        ViewBag.AthleteName = athlete.AthleteName;  
                        ViewBag.AthleteID = athlete.ID;  
                    }
                    else
                    {
                        ViewBag.AthleteName = "無運動員資料"; 
                        ViewBag.AthleteID = 0; 
                    }
                }

                // 將運動員列表儲存到 ViewBag 中
                ViewBag.Athletes = athletesUnderCoach ?? new List<Athletes>(); // 避免空值
                    // 判斷用戶是否為教練
                    //if (user.CoachID.HasValue)
                    //{
                    //    int coachId = user.CoachID.Value;
                    //    var coach = _db.Coaches.FirstOrDefault(c => c.ID == coachId);
                    //    ViewBag.CoachName = coach?.CoachName ?? "未設定教練";
                    //    ViewBag.CoachID = coach?.ID; // 確保CoachID也設置

                    //    // 查詢與該教練相關的運動員
                    //    ViewBag.Athletes = _db.Athletes.Where(a => a.CoachID == coachId).ToList();
                    //}

                    ViewBag.MonitoringItems = GetTrainingMonitoringItems(); //訓練監控項目選擇
                    ViewBag.Description = GetTrainingItem(); //訓練衝量監控
                    ViewBag.TrainingPurposes = GetIntensityClassification(); //訓練強度
                    ViewBag.TrainingTimes = GetTrainingTimes(); //訓練時間
                    ViewBag.RPEScore = GetRPE(); //RPE量表
                    ViewBag.GunItem = GetGunsItems(); //射擊用具項目
                    ViewBag.DetectionSport = GetSpoetsItem(); //檢測系統_運動項目
                    ViewBag.Coaches = _db.Coaches.Where(c => c.IsActive).ToList(); //教練資訊
                    ViewBag.SpecialTechnical = GetSpecialTechnical(); //專項技術類-項目
                    ViewBag.SpecialTechnicalAction = GetSpecialTechnicalAction(); //專項技術類-動作
                    ViewBag.MuscleStrength = GetMuscleStrength(); //肌力訓練部位
                    ViewBag.PhysicalFitness = GetPhysicalFitness(); //體能類訓練類型

                    var records = _db.SessionRPETrainingRecords.ToList();

                    var model = records.Select(r => new SessionRPETrainingRecordsModel
                    {
                        TrainingItem = r.TrainingItem, //訓練名稱
                        RPEscore = r.RPEscore.GetValueOrDefault(), //RPE分數
                        TrainingLoad = r.TrainingLoad ?? 0, //運動訓練量
                        DailyTrainingLoad = r.DailyTrainingLoad ?? 0, //每日運動訓練量
                        WeeklyTrainingChange = r.WeeklyTrainingChange ?? 0, //每週運動訓練量
                        TrainingHomogeneity = r.TrainingHomogeneity ?? 0, //同質性
                        TrainingTension = r.TrainingTension ?? 0, //張力值
                        ShortToLongTermTrainingLoadRatio = r.ShortToLongTermTrainingLoadRatio ?? 0, //短長期
                    }).ToList();

                    ViewBag.SessionTrainingRecords = model;

                    return View();
                }
                else
                {
                    return RedirectToAction("Login","Account");
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }
        #endregion

        #region 一般監控訓練動態表單

        public ActionResult UpdateTableHeaders(string trainingItem)
        {
            try
            {
                ViewBag.MonitoringItems = GetTrainingMonitoringItems(); //訓練監控項目選擇
                ViewBag.Description = GetTrainingItem(); //訓練衝量監控(session-RPE)
                ViewBag.TrainingPurposes = GetIntensityClassification(); //訓練強度
                ViewBag.TrainingTimes = GetTrainingTimes();//訓練時間
                ViewBag.RPEScore = GetRPE();//RPE量表
                ViewBag.GunItem = GetGunsItems(); //射擊用具項目
                ViewBag.DetectionSport = GetSpoetsItem(); //檢測系統_運動項目
                                                          //ViewBag.SpoetsDistance = GetSpoetsDistance(); //檢測系統_距離
                ViewBag.Coaches = _db.Coaches.Where(c => c.IsActive).ToList(); //教練資訊
                ViewBag.SpecialTechnical = GetSpecialTechnical(); //專項技術類-項目
                ViewBag.SpecialTechnicalAction = GetSpecialTechnicalAction(); //專項技術類-動作
                ViewBag.MuscleStrength = GetMuscleStrength(); //肌力訓練部位
                ViewBag.PhysicalFitness = GetPhysicalFitness(); //體能類訓練類型

                return PartialView("_SpecialTechnical", trainingItem);

            }
            catch (Exception ex)
            {
                // 記錄錯誤信息或將錯誤返回到前端
                // Logger.Log(ex);
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
            // 讀取 TrainingPurpose 資料
            var dto = (from ti in _db.TrainingMonitoringItems
                       select ti.TrainingItem).ToList();
            return dto;
        }

        #endregion

        #region 訓練用途
        public List<string> GetTrainingPurposes()
        {
            // 讀取 TrainingPurpose 資料
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
            var rpeList = GetRPE(); // 調用 GetRPE 方法來獲取 RPE 資料
            ViewBag.RPEScores = rpeList; // 將資料傳遞到 ViewBag

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
                       select Si.ItemName).ToList();
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
            double dailyResult = calculationResult; // 根據需要添加其他邏輯計算每日運動量

            return Json(new { calculationResult = calculationResult, dailyResult = dailyResult });
        }
        #endregion

        #region sessionRPE指標計算每日訓練量結果
        public static class InMemoryData
        {
            // 存儲運動數據的靜態列表
            public static List<TrainingData> TrainingDataList = new List<TrainingData>();
        }

        [HttpPost]
        public JsonResult CalculateDailyResult(double trainingTime, double rpeScore, DateTime date)
        {
            DateTime today = DateTime.Today;

            // 確保接收的日期是當天日期
            if (date.Date != today.Date)
            {
                return Json(new { dailyResult = "錯誤: 日期不符" });
            }

            // 獲取當天所有的單次運動時間和RPE分數
            var dailyTrainingData = GetDailyTrainingData(today);

            double totalDailyResult = 0;

            // 計算當天的總運動量
            foreach (var item in dailyTrainingData)
            {
                double time = item.TrainingTime;
                double score = item.RPEScore;
                totalDailyResult += time * score;
            }

            // 計算當前的單次運動時間與RPE分數
            double currentResult = trainingTime * rpeScore;

            // 更新每日運動訓練量
            totalDailyResult += currentResult;

            return Json(new { dailyResult = totalDailyResult });
        }

        private List<TrainingData> GetDailyTrainingData(DateTime date)
        {
            // 根據日期過濾內存中的數據
            return InMemoryData.TrainingDataList
                      .Where(td => td.Date.Date == date.Date)
                      .ToList();
        }
        #endregion

        #region 儲存一般訓練紀錄-教練
        public ActionResult SaveGeneralTrainingRecord(GeneralTrainingRecord record)
        {
            try
            {
                // 確認用戶是教練
                if (Session["UserRole"]?.ToString() != "Coach")
                {
                    return Json(new { success = false, message = "請確認是否為教練身份。" });
                }

                string specialTechnicalTrainingItem = Request.Form["SpecialTechnicalTrainingItem"];

                if (!string.IsNullOrEmpty(specialTechnicalTrainingItem))
                {
                    record.TrainingClassName = specialTechnicalTrainingItem;
                }
                _db.GeneralTrainingRecord.Add(record);
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
        public ActionResult SaveArcheryRecord(ArcheryRecord record, ArcherySessionRPERecord sessionRecord)
        {
            try
            {
                // 確認用戶是教練
                if (Session["UserRole"]?.ToString() != "Coach")
                {
                    return Json(new { success = false, message = "請確認是否為教練身份。" });
                }

                //1.儲存ShottingSessionRPERecord
                sessionRecord.CreatedDate = DateTime.Now; // 設定CreatedDate

                _db.ArcherySessionRPERecord.Add(sessionRecord);
                _db.SaveChanges();

                //2.使用儲存後的ID更新ShootingRecord
                record.SessionRPEArcheryRecordID = sessionRecord.ID;

                //3.更新 record 的 CoachID 和 AthleteID
                record.CoachID = record.CoachID; // 教練ID
                record.AthleteID = record.AthleteID; // 運動員ID

                //4.儲存ShootingRecord
                _db.ArcheryRecord.Add(record);
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
        public ActionResult SaveShootingRecord(ShootingRecord record, ShottingSessionRPERecord sessionRecord)
        {
            try
            {
                // 確認用戶是教練
                if (Session["UserRole"]?.ToString() != "Coach")
                {
                    return Json(new { success = false, message = "請確認是否為教練身份。" });
                }


                // 1. 儲存ShottingSessionRPERecord
                sessionRecord.CreatedDate = DateTime.Now; //設定CreatedDate

                _db.ShottingSessionRPERecord.Add(sessionRecord);
                _db.SaveChanges();

                // 2. 使用儲存後的ID更新ShootingRecord
                record.SessionRPEShottingRecordID = sessionRecord.ID;

                //3.更新 record 的 CoachID 和 AthleteID
                record.CoachID = record.CoachID; // 教練ID
                record.AthleteID = record.AthleteID; // 運動員ID

                //4. 儲存ShootingRecord
                _db.ShootingRecord.Add(record);
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
                              .Select(a => a.AthleteName)
                              .ToList();
            return PartialView("_Athletes", athletes);
        }

        #endregion

        #region 建立運動員資訊
        //[Authorize(Roles = "Coach")] // 確保用戶是教練角色
        public ActionResult createAthletes()
        {
            return View();
        }

        [HttpPost]
        //[Authorize(Roles = "Coach")] // 確保用戶是教練角色
        public ActionResult CreateAthletes(Athletes model)
        {
            if (ModelState.IsValid)
            {
                // 將運動員資訊存儲到資料庫
                // 例如：
                // db.Athletes.Add(model);
                // db.SaveChanges();

                return RedirectToAction("CreateAthletes"); // 重定向到運動員列表頁面
            }

            return View(model);
        }
        #endregion

        #region 訓練處方計算
        public ActionResult RedirectToPrescription()
        {
            // 重定向到 TrainingPrescriptionController 的 Prescription 方法
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

                return PartialView("_GeneralRecord", records); // 返回部分視圖
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
                
                int? coachId = AuthHelper.GetCurrentCoachId(); //獲取當前登入的教練ID
                int? athleteId = AuthHelper.GetCurrentAthleteId(); //獲取當前登入的運動員ID

                if (!coachId.HasValue || !athleteId.HasValue)
                {
                    return Json(new { success = false, message = "使用者未登入或ID錯誤" });
                }
                
                var coach = _db.Coaches.SingleOrDefault(c => c.ID == coachId.Value); //查詢教練資料
                var athlete = _db.Athletes.SingleOrDefault(a => a.ID == athleteId.Value); //查詢運動員資料

                if (coach == null)
                {
                    return Json(new { success = false, message = "無效的教練ID" });
                }

                if (athlete == null)
                {
                    return Json(new { success = false, message = "無效的運動員ID" });
                }

                // 儲存各項檢測系統訓練紀錄
                var detectionRecord = new DetectionTrainingRecord
                {
                    Coach = model.coach, //教練名字
                    CoachID = coach.ID, //教練ID
                    Athlete = model.athlete, //運動員
                    AthleteID = athlete.ID, //運動員ID
                    DetectionItem = "有/無氧代謝能力測定",
                    SportItem = model.SportItem, //運動項目
                    TrainingDate = DateTime.Parse(model.DetectionDate), //訓練日期
                    CriticalSpeed = model.CriticalSpeed, //臨界速度
                    MaxAnaerobicWork = model.AnaerobicPower, //最大無氧做功
                    CreatedDate = DateTime.Now, //建立時間
                    ModifiedDate = DateTime.Now, //修改時間
                };
                _db.DetectionTrainingRecord.Add(detectionRecord);
                _db.SaveChanges();

                int detectionRecordId = detectionRecord.ID;

                // 儲存田徑場檢測詳細記錄
                if (model.SportItem == "跑步機")
                {
                    for (int i = 0; i < model.IntenPercen.Count; i++)
                    {
                        var dtos = new TreadmillRecordDetails
                        {
                            DetectionTrainingRecordId = detectionRecordId, //主資料表ID
                            IntenPercen = (model.IntenPercen[i]), //強度百分比 
                            MaxRunningSpeed = model.MaxRunningSpeed, //最大跑速
                            ForceDuration = int.Parse(model.ForceDurations[i]), //力竭時間
                            Speed = float.Parse(model.Speeds[i]), //速度
                            CreatedDate = DateTime.Now, //建立時間
                            TrainingDateTime = DateTime.Parse(model.DetectionDate), //訓練日期
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
                            DetectionTrainingRecordId = detectionRecordId, //主資料表ID
                            Distance = (model.Distances[i]), //訓練距離
                            ForceDuration = int.Parse(model.ForceDurations[i]), //力竭時間
                            Speed = float.Parse(model.Speeds[i]), //速度
                            CreatedDate = DateTime.Now, //建立時間
                            TrainingDateTime = DateTime.Parse(model.DetectionDate), //訓練日期
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
                            DetectionTrainingRecordId = detectionRecordId, //主資料表ID
                            Distance = (model.Distances[i]), //訓練距離
                            ForceDuration = int.Parse(model.ForceDurations[i]), //力竭時間
                            Speed = float.Parse(model.Speeds[i]), //速度
                            CreatedDate = DateTime.Now, //建立時間
                            TrainingDateTime = DateTime.Parse(model.DetectionDate), //訓練日期
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
                            DetectionTrainingRecordId = detectionRecordId, //主資料表ID
                            IntenPercen = (model.IntenPercen[i]), //強度百分比 
                            MaxPower = model.MaxPower, //最大功率
                            ForceDuration = int.Parse(model.ForceDurations[i]), //力竭時間
                            Speed = float.Parse(model.Speeds[i]), //速度
                            CreatedDate = DateTime.Now, //建立時間
                            TrainingDateTime = DateTime.Parse(model.DetectionDate), //訓練日期
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
                            DetectionTrainingRecordId = detectionRecordId, //主資料表ID
                            Distance = (model.Distances[i]), //訓練距離
                            ForceDuration = int.Parse(model.ForceDurations[i]), //力竭時間
                            Speed = float.Parse(model.Speeds[i]), //速度
                            CreatedDate = DateTime.Now, //建立時間
                            TrainingDateTime = DateTime.Parse(model.DetectionDate), //訓練日期
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
    }
}