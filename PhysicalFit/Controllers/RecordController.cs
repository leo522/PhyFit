using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace PhysicalFit.Controllers
{
    public class RecordController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities(); //資料庫

        #region 查詢訓練紀錄

        public ActionResult SessionRecord(string item, int? AthleteID)
        {
            if (AthleteID == null)
            {
                TempData["ErrorMessage"] = "請選擇運動員";
                return RedirectToAction("dashboard", "PhyFit");
            }

            var viewModel = new TrainingRecordViewModel { TrainingItem = item };

            switch (item)
            {
                case "RPE訓練紀錄":
                    viewModel.RPERecords = _db.SessionTrainingRecords
                                              .OrderBy(x => x.TrainingDate)
                                              .Select(x => new RPETrainingRecordViewModel
                                              {
                                                  TrainingDate = x.TrainingDate,
                                                  //AthleteName = x.UserAccount ?? 0,
                                                  //RPELevel = x.TrainingItem
                                              })
                                              .ToList();
                    break;

                case "專項訓練-射箭訓練衝量":
                    viewModel.ArcheryRecords = _db.ArcheryRecord
                                                  .Where(x => x.AthleteID == AthleteID) // 只查詢選中的運動員
                                                  .OrderBy(x => x.TrainingDate)
                                                  .Select(x => new ArcheryTrainingRecordViewModel
                                                  {
                                                      TrainingDate = x.TrainingDate ?? DateTime.Now,
                                                      Coach = x.Coach,
                                                      Athlete = x.Athlete,
                                                      Poundage = x.Poundage ?? 0,
                                                      ArrowCount = x.ArrowCount ?? 0,
                                                      RPEscore = x.RPEscore ?? 0,
                                                      EachTrainingLoad = x.EachTrainingLoad ?? 0
                                                  })
                                                  .ToList();
                    break;

                case "專項訓練-射擊訓練衝量":
                    viewModel.ShootingRecords = _db.ShootingRecord
                                                   .Where(x => x.AthleteID == AthleteID) // 只查詢選中的運動員
                                                   .OrderBy(x => x.TrainingDate)
                                                   .Select(x => new ShootingTrainingRecordViewModel
                                                   {
                                                       TrainingDate = x.TrainingDate ?? DateTime.Now,
                                                       Coach = x.Coach,
                                                       Athlete = x.Athlete,
                                                       ShootingTool = x.ShootingTool,
                                                       BulletCount = x.BulletCount ?? 0,
                                                       RPEscore = x.RPEscore ?? 0,
                                                       EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                                   })
                                                   .ToList();
                    break;

                default:
                    TempData["ErrorMessage"] = "無效的訓練項目";
                    return RedirectToAction("dashboard", "PhyFit");
            }

            return View(viewModel);
        }

        #endregion

        #region 讀取session訓練量結果

        public ActionResult LoadSessionRPETrainingRecords(string item)
        {
            // 建立 TrainingRecordViewModel 並傳入訓練項目名稱
            var model = new TrainingRecordViewModel { TrainingItem = item };
            // 從三個表中讀取資料
            var sessionRPERecords = _db.SessionRPETrainingRecords.ToList();
            var archeryRecords = _db.ArcheryRecord.ToList();
            var shootingRecords = _db.ShootingRecord.ToList();

            // 整合所有的訓練數據，並存入viewModel
            model.RPERecords = sessionRPERecords
                .Select(record => new RPETrainingRecordViewModel
                {
                    TrainingDate = record.TrainingDate ?? DateTime.Now,
                    RPELevel = record.RPEscore ?? 0,
                    // 可以繼續添加其他字段
                })
                .ToList();

            model.ArcheryRecords = archeryRecords
                .Select(record => new ArcheryTrainingRecordViewModel
                {
                    TrainingDate = record.TrainingDate ?? DateTime.Now,
                    Coach = record.Coach,
                    Athlete = record.Athlete,
                    Poundage = record.Poundage ?? 0,
                    ArrowCount = record.ArrowCount ?? 0,
                    RPEscore = record.RPEscore ?? 0,
                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
                })
                .ToList();

            model.ShootingRecords = shootingRecords
                .Select(record => new ShootingTrainingRecordViewModel
                {
                    TrainingDate = record.TrainingDate ?? DateTime.Now,
                    Coach = record.Coach,
                    Athlete = record.Athlete,
                    ShootingTool = record.ShootingTool,
                    BulletCount = record.BulletCount ?? 0,
                    RPEscore = record.RPEscore ?? 0,
                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
                })
                .ToList();

            // 返回主視圖 (SessionRecord.cshtml)
            return View("SessionRecord", model);
        }

        #endregion

        #region 計算session RPE指標結果

        [HttpGet]
        public JsonResult CalculateTrainingLoad(DateTime date, string trainingType)
        {
            try
            {
                // 移除時間部分，只保留年月日
                DateTime selectedDate = date.Date;

                int totalTrainingLoad = 0;
                int dailyTrainingLoadSum = 0;
                int weeklyTrainingLoadSum = 0;
                double trainingMonotony = 0;
                double trainingStrain = 0;
                double weekToWeekChange = 0;
                double acwr = 0;

                // 根據不同的訓練類型進行查詢和計算
                if (trainingType == "RPE訓練紀錄")
                {
                    var sessionRPERecords = _db.SessionRPETrainingRecords
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                        .ToList();

                    totalTrainingLoad = sessionRPERecords.Sum(r => r.TrainingLoad ?? 0);
                    dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType);
                    weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType);
                    trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType);
                    trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType);
                    weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType);
                    acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType);
                }
                else if (trainingType == "專項訓練-射箭訓練衝量")
                {
                    var archeryRecords = _db.ArcheryRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                        .ToList();

                    totalTrainingLoad = archeryRecords.Sum(r => r.EachTrainingLoad ?? 0);
                    dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType);
                    weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType);
                    trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType);
                    trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType);
                    weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType);
                    acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType);
                }
                else if (trainingType == "專項訓練-射擊訓練衝量")
                {
                    var shootingRecords = _db.ShootingRecord
                        .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                        .ToList();

                    totalTrainingLoad = shootingRecords.Sum(r => r.EachTrainingLoad ?? 0);
                    dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType);
                    weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType);
                    trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType);
                    trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType);
                    weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType);
                    acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType);
                }
                else
                {
                    throw new ArgumentException("未知的訓練類型");
                }

                // 返回計算結果
                return Json(new
                {
                    TrainingLoad = totalTrainingLoad,
                    DailyTrainingLoadSum = dailyTrainingLoadSum,
                    WeeklyTrainingLoadSum = weeklyTrainingLoadSum,
                    TrainingMonotony = trainingMonotony,
                    TrainingStrain = trainingStrain,
                    WeekToWeekChange = weekToWeekChange,
                    ACWR = acwr
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        #endregion

        #region 儲存sessionRPE訓練量結果

        [HttpPost]
        public JsonResult SaveTrainingRecord(SessionRPETrainingRecordsModel model, int shootingRecordId)
        {
            try
            {
                // 將 TrainingTime 轉換為分鐘
                int trainingTimeInMinutes = ConvertTrainingTimeToMinutes(model.TrainingTime);

                //計算每日運動訓練量
                int dailyTrainingLoad = trainingTimeInMinutes * model.RPEscore;

                var newRecord = new SessionRPETrainingRecords
                {
                    TrainingDate = model.TrainingDate, //日期
                    TrainingItem = model.TrainingItem, //訓練衝量監控項目
                    DifficultyCategory = model.DifficultyCategory, //難度分類
                    TrainingActionName = model.TrainingActionName, //動作名稱
                    TrainingTime = trainingTimeInMinutes.ToString(), // 訓練時間轉換為分鐘
                    RPEscore = model.RPEscore, //RPE分數
                    TrainingLoad = model.TrainingLoad, //運動訓練量
                    DailyTrainingLoad = dailyTrainingLoad, //每日運動訓練量(計算後)
                    WeeklyTrainingLoad = model.WeeklyTrainingLoad, //每週運動訓練量
                    TrainingHomogeneity = model.TrainingHomogeneity, //訓練同質性
                    TrainingTension = model.TrainingTension, //訓練張力值
                    WeeklyTrainingChange = model.WeeklyTrainingChange, //週間訓練變化
                    ShortToLongTermTrainingLoadRatio = model.ShortToLongTermTrainingLoadRatio, //短期：長期訓練量比值
                    CreatedDate = DateTime.Now //建立時間
                };

                _db.SessionRPETrainingRecords.Add(newRecord);
                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 小時轉換為分鐘
        // 將訓練時間轉換為分鐘的函式
        private int ConvertTrainingTimeToMinutes(string trainingTime)
        {
            if (string.IsNullOrEmpty(trainingTime))
                return 0;

            // 解析小時數
            if (trainingTime.EndsWith("小時"))
            {
                if (int.TryParse(trainingTime.Replace("小時", "").Trim(), out int hours))
                {
                    return hours * 60; //轉換為分鐘
                }
            }

            // 解析分鐘數
            if (trainingTime.EndsWith("分鐘"))
            {
                if (int.TryParse(trainingTime.Replace("分鐘", "").Trim(), out int minutes))
                {
                    return minutes; //直接返回分鐘
                }
            }

            return 0; //如果無法解析，返回 0
        }

        #endregion

        public ActionResult Indicator(int coachId)
        {
            var athletes = _db.Athletes
                              .Where(a => a.CoachID == coachId)
                              .Select(a => a.AthleteName)
                              .ToList();
            return PartialView("_Indicator");

        }
    }
}