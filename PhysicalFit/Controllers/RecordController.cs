using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Microsoft.Ajax.Utilities;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace PhysicalFit.Controllers
{
    public class RecordController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities(); //資料庫

        #region 查詢訓練紀錄
        public ActionResult SessionRecord(string item, int? AthleteID, DateTime? date, string data)
        {
            try
            {
                // 如果傳遞的是加密的 data，進行解碼並解析
                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(data));
                        var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedData);

                        // 使用解碼的值覆蓋現有參數
                        item = parameters.ContainsKey("item") ? parameters["item"] : item;
                        AthleteID = parameters.ContainsKey("AthleteID") ? int.Parse(parameters["AthleteID"]) : AthleteID;
                        date = parameters.ContainsKey("date") ? DateTime.Parse(parameters["date"]) : date;
                    }
                    catch (Exception)
                    {
                        TempData["ErrorMessage"] = "請確認資料格式正確";
                        return RedirectToAction("dashboard", "PhyFit");
                    }
                }

                ViewBag.SelectedTrainingItem = item; //把item傳遞到ViewBag
                ViewBag.AthleteID = AthleteID;
                var userRole = Session["UserRole"]?.ToString();
                var loggedInAthleteID = AthleteID;

                // 如果是運動員，強制使用自己的 AthleteID
                if (userRole == "Athlete")
                {
                    AthleteID = loggedInAthleteID;
                }

                // 如果是教練，但沒有選擇運動員，顯示錯誤信息
                if (userRole == "Coach" && AthleteID == null)
                {
                    TempData["ErrorMessage"] = "請選擇運動員";
                    return RedirectToAction("dashboard", "PhyFit");
                }

                // 初始化視圖模型
                var combinedViewModel = new CombinedViewModel();
                combinedViewModel.TrainingRecord = new TrainingRecordViewModel { TrainingItem = item };

                // 查詢心理特質與食慾的數據
                var psychologicalData = _db.PsychologicalTraitsResults
                    .Where(x => x.UserID == AthleteID)
                    .OrderBy(x => x.PsychologicalDate)
                    .ToList();

                if (psychologicalData == null || !psychologicalData.Any())
                {
                    ViewBag.NoDataMessage = "無相關資料";
                }
                else
                {
                    ViewBag.NoDataMessage = null; // 如果有資料，清空提示
                }

                // 初始化ViewModel中的各個List
                var dates = new List<string>();
                var sleepQualityScores = new List<int>();
                var fatigueScores = new List<int>();
                var trainingWillingnessScores = new List<int>();
                var appetiteScores = new List<int>();
                var competitionWillingnessScores = new List<int>();

                //迭代資料並根據 Trait 將不同的數據分到對應的 List 中
                foreach (var record in psychologicalData)
                {
                    var dateString = record.PsychologicalDate.ToString("yyyy-MM-dd");

                    // 如果當前日期還沒有被添加，先添加日期
                    if (!dates.Contains(dateString))
                    {
                        dates.Add(dateString);
                    }

                    // 根據不同的心理特質與食慾圖量表來填充對應的分數 List
                    switch (record.Trait)
                    {
                        case "睡眠品質":
                            sleepQualityScores.Add(record.Score);
                            break;
                        case "疲憊程度":
                            fatigueScores.Add(record.Score);
                            break;
                        case "訓練意願":
                            trainingWillingnessScores.Add(record.Score);
                            break;
                        case "胃口":
                            appetiteScores.Add(record.Score);
                            break;
                        case "比賽意願":
                            competitionWillingnessScores.Add(record.Score);
                            break;
                    }
                }

                // 填充心理特質與食慾圖量表 ViewModel
                combinedViewModel.PsychologicalRecord = new PsychologicalViewModel
                {
                    Dates = dates,
                    SleepQualityScores = sleepQualityScores,
                    FatigueScores = fatigueScores,
                    TrainingWillingnessScores = trainingWillingnessScores,
                    AppetiteScores = appetiteScores,
                    CompetitionWillingnessScores = competitionWillingnessScores
                };


                // 根據選擇的訓練項目查詢對應的數據
                switch (item)
                {
                    case "一般訓練衝量監控 (session-RPE)":
                        if (userRole == "Athlete")
                        {
                            var generalTrainingRecords = _db.AthleteGeneralTrainingRecord
                            .Where(x => x.AthleteID == AthleteID);

                            if (date.HasValue)
                            {
                                generalTrainingRecords = generalTrainingRecords
                                    .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
                            }

                            generalTrainingRecords = generalTrainingRecords.OrderBy(x => x.TrainingDate);

                            combinedViewModel.TrainingRecord.GeneralTrainingRecord = generalTrainingRecords
                                .Select(x => new GeneralTrainingRecordViewModel
                                {
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    TrainingItem = x.TrainingItem,
                                    ActionName = x.ActionName,
                                    TrainingTime = x.TrainingTime,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                }).ToList();
                        }
                        else
                        {
                            var generalTrainingRecords = _db.GeneralTrainingRecord
                                .Where(x => x.AthleteID == AthleteID);

                            if (date.HasValue)
                            {
                                generalTrainingRecords = generalTrainingRecords
                                    .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
                            }

                            generalTrainingRecords = generalTrainingRecords.OrderBy(x => x.TrainingDate);

                            combinedViewModel.TrainingRecord.GeneralTrainingRecord = generalTrainingRecords
                                .Select(x => new GeneralTrainingRecordViewModel
                                {
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    TrainingItem = x.TrainingItem,
                                    ActionName = x.ActionName,
                                    TrainingTime = x.TrainingTime,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                }).ToList();
                        }
                        break;

                    case "射箭訓練衝量":
                        if (userRole == "Athlete")
                        {
                            var archeryRecords = _db.AthleteArcheryTrainingRecord.Where(x => x.AthleteID == AthleteID);

                            if (date.HasValue)
                            {
                                archeryRecords = archeryRecords
                                    .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
                            }

                            archeryRecords = archeryRecords.OrderBy(x => x.TrainingDate);

                            combinedViewModel.TrainingRecord.ArcheryRecords = archeryRecords
                                .Select(x => new ArcheryTrainingRecordViewModel
                                {
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    Poundage = x.Poundage ?? 0,
                                    ArrowCount = x.ArrowCount ?? 0,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                }).ToList();
                        }
                        else
                        {
                            var archeryRecords = _db.ArcheryRecord.Where(x => x.AthleteID == AthleteID);

                            if (date.HasValue)
                            {
                                archeryRecords = archeryRecords
                                    .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
                            }

                            archeryRecords = archeryRecords.OrderBy(x => x.TrainingDate);

                            combinedViewModel.TrainingRecord.ArcheryRecords = archeryRecords
                                .Select(x => new ArcheryTrainingRecordViewModel
                                {
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    Poundage = x.Poundage ?? 0,
                                    ArrowCount = x.ArrowCount ?? 0,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                }).ToList();
                        }
                        break;

                    case "射擊訓練衝量":
                        if (userRole == "Athlete")
                        {
                            var shootingRecords = _db.AthleteShootingRecord.Where(x => x.AthleteID == AthleteID);

                            if (date.HasValue)
                            {
                                shootingRecords = shootingRecords
                                    .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
                            }

                            shootingRecords = shootingRecords.OrderBy(x => x.TrainingDate);

                            combinedViewModel.TrainingRecord.ShootingRecords = shootingRecords
                                .Select(x => new ShootingTrainingRecordViewModel
                                {
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    ShootingTool = x.ShootingTool,
                                    BulletCount = x.BulletCount ?? 0,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                }).ToList();
                        }
                        else
                        {
                            var shootingRecords = _db.ShootingRecord.Where(x => x.AthleteID == AthleteID);

                            if (date.HasValue)
                            {
                                shootingRecords = shootingRecords
                                    .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
                            }

                            shootingRecords = shootingRecords.OrderBy(x => x.TrainingDate);

                            combinedViewModel.TrainingRecord.ShootingRecords = shootingRecords
                                .Select(x => new ShootingTrainingRecordViewModel
                                {
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    ShootingTool = x.ShootingTool,
                                    BulletCount = x.BulletCount ?? 0,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                }).ToList();
                        }
                        break;

                    case "檢測系統":
                        // 加入檢測系統的處理邏輯
                        var detectionRecords = _db.DetectionTrainingRecord
                            .Where(x => x.AthleteID == AthleteID);

                        if (date.HasValue)
                        {
                            detectionRecords = detectionRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
                        }

                        detectionRecords = detectionRecords.OrderBy(x => x.TrainingDate);

                        combinedViewModel.TrainingRecord.DetectionRecords = detectionRecords
                            .Select(x => new DetectionTrainingRecordViewModel
                            {
                                TrainingDate = x.TrainingDate ?? DateTime.Now,
                                Coach = x.Coach,
                                Athlete = x.Athlete,
                                DetectionItem = x.DetectionItem,
                                SportItem = x.SportItem,
                                CriticalSpeed = x.CriticalSpeed ?? 0,
                                MaxAnaerobicWork = x.MaxAnaerobicWork ?? 0,
                                TrainingVolume = x.TrainingVolume ?? 0,
                                TrainingPrescription = x.TrainingPrescription ?? 0,
                                CoefficientOfDetermination = x.CoefficientOfDetermination ?? 0
                            }).ToList();
                        break;

                    case "心理特質和食慾圖量表":

                        // 迭代資料並根據 Trait 將不同的數據分到對應的 List 中
                        foreach (var record in psychologicalData)
                        {
                            var dateString = record.PsychologicalDate.ToString("yyyy-MM-dd");

                            if (!dates.Contains(dateString))
                            {
                                dates.Add(dateString);
                            }

                            switch (record.Trait)
                            {
                                case "睡眠品質":
                                    sleepQualityScores.Add(record.Score);
                                    break;
                                case "疲憊程度":
                                    fatigueScores.Add(record.Score);
                                    break;
                                case "訓練意願":
                                    trainingWillingnessScores.Add(record.Score);
                                    break;
                                case "胃口":
                                    appetiteScores.Add(record.Score);
                                    break;
                                case "比賽意願":
                                    competitionWillingnessScores.Add(record.Score);
                                    break;
                            }
                        }

                        // 將心理特質與食慾數據添加到 ViewModel
                        combinedViewModel.TrainingRecord.Psychological = new List<PsychologicalViewModel>
                {
                    new PsychologicalViewModel
                    {
                        Dates = dates,
                        SleepQualityScores = sleepQualityScores,
                        FatigueScores = fatigueScores,
                        TrainingWillingnessScores = trainingWillingnessScores,
                        AppetiteScores = appetiteScores,
                        CompetitionWillingnessScores = competitionWillingnessScores
                    }
                };
                        break;

                    default:
                        TempData["ErrorMessage"] = "無效的訓練項目";
                        return RedirectToAction("dashboard", "PhyFit");
                }

                return View(combinedViewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public ActionResult SessionRecord(string item, int? AthleteID, DateTime? date, string data)
        //{
        //    try
        //    {
        //        // 如果傳遞的是加密的 data，進行解碼並解析
        //        if (!string.IsNullOrEmpty(data))
        //        {
        //            try
        //            {
        //                string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(data));
        //                var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedData);

        //                // 使用解碼的值覆蓋現有參數
        //                item = parameters.ContainsKey("item") ? parameters["item"] : item;
        //                AthleteID = parameters.ContainsKey("AthleteID") ? int.Parse(parameters["AthleteID"]) : AthleteID;
        //                date = parameters.ContainsKey("date") ? DateTime.Parse(parameters["date"]) : date;
        //            }
        //            catch (Exception)
        //            {
        //                TempData["ErrorMessage"] = "請確認資料格式正確";
        //                return RedirectToAction("dashboard", "PhyFit");
        //            }
        //        }

        //        ViewBag.SelectedTrainingItem = item; //把item傳遞到ViewBag
        //        ViewBag.AthleteID = AthleteID;
        //        var userRole = Session["UserRole"]?.ToString();
        //        var loggedInAthleteID = AthleteID;

        //        // 如果是運動員，強制使用自己的 AthleteID
        //        if (userRole == "Athlete")
        //        {
        //            AthleteID = loggedInAthleteID;
        //        }

        //        // 如果是教練，但沒有選擇運動員，顯示錯誤信息
        //        if (userRole == "Coach" && AthleteID == null)
        //        {
        //            TempData["ErrorMessage"] = "請選擇運動員";
        //            return RedirectToAction("dashboard", "PhyFit");
        //        }

        //        // 初始化視圖模型
        //        var combinedViewModel = new CombinedViewModel();
        //        combinedViewModel.TrainingRecord = new TrainingRecordViewModel { TrainingItem = item };

        //        // 查詢心理特質與食慾的數據
        //        var psychologicalData = _db.PsychologicalTraitsResults
        //            .Where(x => x.UserID == AthleteID)
        //            .OrderBy(x => x.PsychologicalDate)
        //            .ToList();

        //        // 初始化ViewModel中的各個List
        //        var dates = new List<string>();
        //        var sleepQualityScores = new List<int>();
        //        var fatigueScores = new List<int>();
        //        var trainingWillingnessScores = new List<int>();
        //        var appetiteScores = new List<int>();
        //        var competitionWillingnessScores = new List<int>();

        //        //迭代資料並根據 Trait 將不同的數據分到對應的 List 中
        //        foreach (var record in psychologicalData)
        //        {
        //            var dateString = record.PsychologicalDate.ToString("yyyy-MM-dd");

        //            // 如果當前日期還沒有被添加，先添加日期
        //            if (!dates.Contains(dateString))
        //            {
        //                dates.Add(dateString);
        //            }

        //            // 根據不同的心理特質與食慾圖量表來填充對應的分數 List
        //            switch (record.Trait)
        //            {
        //                case "睡眠品質":
        //                    sleepQualityScores.Add(record.Score);
        //                    break;
        //                case "疲憊程度":
        //                    fatigueScores.Add(record.Score);
        //                    break;
        //                case "訓練意願":
        //                    trainingWillingnessScores.Add(record.Score);
        //                    break;
        //                case "胃口":
        //                    appetiteScores.Add(record.Score);
        //                    break;
        //                case "比賽意願":
        //                    competitionWillingnessScores.Add(record.Score);
        //                    break;
        //            }
        //        }

        //        // 填充心理特質與食慾圖量表 ViewModel
        //        combinedViewModel.PsychologicalRecord = new PsychologicalViewModel
        //        {
        //            Dates = dates,
        //            SleepQualityScores = sleepQualityScores,
        //            FatigueScores = fatigueScores,
        //            TrainingWillingnessScores = trainingWillingnessScores,
        //            AppetiteScores = appetiteScores,
        //            CompetitionWillingnessScores = competitionWillingnessScores
        //        };


        //        // 根據選擇的訓練項目查詢對應的數據
        //        switch (item)
        //        {
        //            case "一般訓練衝量監控 (session-RPE)":
        //                if (userRole == "Athlete")
        //                {
        //                    var generalTrainingRecords = _db.AthleteGeneralTrainingRecord
        //                    .Where(x => x.AthleteID == AthleteID);

        //                    if (date.HasValue)
        //                    {
        //                        generalTrainingRecords = generalTrainingRecords
        //                            .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
        //                    }

        //                    generalTrainingRecords = generalTrainingRecords.OrderBy(x => x.TrainingDate);

        //                    combinedViewModel.TrainingRecord.GeneralTrainingRecord = generalTrainingRecords
        //                        .Select(x => new GeneralTrainingRecordViewModel
        //                        {
        //                            TrainingDate = x.TrainingDate ?? DateTime.Now,
        //                            Coach = x.Coach,
        //                            Athlete = x.Athlete,
        //                            TrainingItem = x.TrainingItem,
        //                            ActionName = x.ActionName,
        //                            TrainingTime = x.TrainingTime,
        //                            RPEscore = x.RPEscore ?? 0,
        //                            EachTrainingLoad = x.EachTrainingLoad ?? 0,
        //                        }).ToList();
        //                }
        //                else
        //                {
        //                    var generalTrainingRecords = _db.GeneralTrainingRecord
        //                        .Where(x => x.AthleteID == AthleteID);

        //                    if (date.HasValue)
        //                    {
        //                        generalTrainingRecords = generalTrainingRecords
        //                            .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
        //                    }

        //                    generalTrainingRecords = generalTrainingRecords.OrderBy(x => x.TrainingDate);

        //                    combinedViewModel.TrainingRecord.GeneralTrainingRecord = generalTrainingRecords
        //                        .Select(x => new GeneralTrainingRecordViewModel
        //                        {
        //                            TrainingDate = x.TrainingDate ?? DateTime.Now,
        //                            Coach = x.Coach,
        //                            Athlete = x.Athlete,
        //                            TrainingItem = x.TrainingItem,
        //                            ActionName = x.ActionName,
        //                            TrainingTime = x.TrainingTime,
        //                            RPEscore = x.RPEscore ?? 0,
        //                            EachTrainingLoad = x.EachTrainingLoad ?? 0,
        //                        }).ToList();
        //                }
        //                break;

        //            case "射箭訓練衝量":
        //                if (userRole == "Athlete")
        //                {
        //                    var archeryRecords = _db.AthleteArcheryTrainingRecord.Where(x => x.AthleteID == AthleteID);

        //                    if (date.HasValue)
        //                    {
        //                        archeryRecords = archeryRecords
        //                            .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
        //                    }

        //                    archeryRecords = archeryRecords.OrderBy(x => x.TrainingDate);

        //                    combinedViewModel.TrainingRecord.ArcheryRecords = archeryRecords
        //                        .Select(x => new ArcheryTrainingRecordViewModel
        //                        {
        //                            TrainingDate = x.TrainingDate ?? DateTime.Now,
        //                            Coach = x.Coach,
        //                            Athlete = x.Athlete,
        //                            Poundage = x.Poundage ?? 0,
        //                            ArrowCount = x.ArrowCount ?? 0,
        //                            RPEscore = x.RPEscore ?? 0,
        //                            EachTrainingLoad = x.EachTrainingLoad ?? 0,
        //                        }).ToList();
        //                }
        //                else
        //                {
        //                    var archeryRecords = _db.ArcheryRecord.Where(x => x.AthleteID == AthleteID);

        //                    if (date.HasValue)
        //                    {
        //                        archeryRecords = archeryRecords
        //                            .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
        //                    }

        //                    archeryRecords = archeryRecords.OrderBy(x => x.TrainingDate);

        //                    combinedViewModel.TrainingRecord.ArcheryRecords = archeryRecords
        //                        .Select(x => new ArcheryTrainingRecordViewModel
        //                        {
        //                            TrainingDate = x.TrainingDate ?? DateTime.Now,
        //                            Coach = x.Coach,
        //                            Athlete = x.Athlete,
        //                            Poundage = x.Poundage ?? 0,
        //                            ArrowCount = x.ArrowCount ?? 0,
        //                            RPEscore = x.RPEscore ?? 0,
        //                            EachTrainingLoad = x.EachTrainingLoad ?? 0,
        //                        }).ToList();
        //                }
        //                break;

        //            case "射擊訓練衝量":
        //                if (userRole == "Athlete")
        //                {
        //                    var shootingRecords = _db.AthleteShootingRecord.Where(x => x.AthleteID == AthleteID);

        //                    if (date.HasValue)
        //                    {
        //                        shootingRecords = shootingRecords
        //                            .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
        //                    }

        //                    shootingRecords = shootingRecords.OrderBy(x => x.TrainingDate);

        //                    combinedViewModel.TrainingRecord.ShootingRecords = shootingRecords
        //                        .Select(x => new ShootingTrainingRecordViewModel
        //                        {
        //                            TrainingDate = x.TrainingDate ?? DateTime.Now,
        //                            Coach = x.Coach,
        //                            Athlete = x.Athlete,
        //                            ShootingTool = x.ShootingTool,
        //                            BulletCount = x.BulletCount ?? 0,
        //                            RPEscore = x.RPEscore ?? 0,
        //                            EachTrainingLoad = x.EachTrainingLoad ?? 0,
        //                        }).ToList();
        //                }
        //                else
        //                {
        //                    var shootingRecords = _db.ShootingRecord.Where(x => x.AthleteID == AthleteID);

        //                    if (date.HasValue)
        //                    {
        //                        shootingRecords = shootingRecords
        //                            .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
        //                    }

        //                    shootingRecords = shootingRecords.OrderBy(x => x.TrainingDate);

        //                    combinedViewModel.TrainingRecord.ShootingRecords = shootingRecords
        //                        .Select(x => new ShootingTrainingRecordViewModel
        //                        {
        //                            TrainingDate = x.TrainingDate ?? DateTime.Now,
        //                            Coach = x.Coach,
        //                            Athlete = x.Athlete,
        //                            ShootingTool = x.ShootingTool,
        //                            BulletCount = x.BulletCount ?? 0,
        //                            RPEscore = x.RPEscore ?? 0,
        //                            EachTrainingLoad = x.EachTrainingLoad ?? 0,
        //                        }).ToList();
        //                }
        //                break;

        //            case "檢測系統":
        //                // 加入檢測系統的處理邏輯
        //                var detectionRecords = _db.DetectionTrainingRecord
        //                    .Where(x => x.AthleteID == AthleteID);

        //                if (date.HasValue)
        //                {
        //                    detectionRecords = detectionRecords
        //                        .Where(x => DbFunctions.TruncateTime(x.TrainingDate) == DbFunctions.TruncateTime(date.Value));
        //                }

        //                detectionRecords = detectionRecords.OrderBy(x => x.TrainingDate);

        //                combinedViewModel.TrainingRecord.DetectionRecords = detectionRecords
        //                    .Select(x => new DetectionTrainingRecordViewModel
        //                    {
        //                        TrainingDate = x.TrainingDate ?? DateTime.Now,
        //                        Coach = x.Coach,
        //                        Athlete = x.Athlete,
        //                        DetectionItem = x.DetectionItem,
        //                        SportItem = x.SportItem,
        //                        CriticalSpeed = x.CriticalSpeed ?? 0,
        //                        MaxAnaerobicWork = x.MaxAnaerobicWork ?? 0,
        //                        TrainingVolume = x.TrainingVolume ?? 0,
        //                        TrainingPrescription = x.TrainingPrescription ?? 0,
        //                        CoefficientOfDetermination = x.CoefficientOfDetermination ?? 0
        //                    }).ToList();
        //                break;

        //            case "心理特質和食慾圖量表":

        //                // 迭代資料並根據 Trait 將不同的數據分到對應的 List 中
        //                foreach (var record in psychologicalData)
        //                {
        //                    var dateString = record.PsychologicalDate.ToString("yyyy-MM-dd");

        //                    if (!dates.Contains(dateString))
        //                    {
        //                        dates.Add(dateString);
        //                    }

        //                    switch (record.Trait)
        //                    {
        //                        case "睡眠品質":
        //                            sleepQualityScores.Add(record.Score);
        //                            break;
        //                        case "疲憊程度":
        //                            fatigueScores.Add(record.Score);
        //                            break;
        //                        case "訓練意願":
        //                            trainingWillingnessScores.Add(record.Score);
        //                            break;
        //                        case "胃口":
        //                            appetiteScores.Add(record.Score);
        //                            break;
        //                        case "比賽意願":
        //                            competitionWillingnessScores.Add(record.Score);
        //                            break;
        //                    }
        //                }

        //                // 將心理特質與食慾數據添加到 ViewModel
        //                combinedViewModel.TrainingRecord.Psychological = new List<PsychologicalViewModel>
        //                {
        //                    new PsychologicalViewModel
        //                    {
        //                        Dates = dates,
        //                        SleepQualityScores = sleepQualityScores,
        //                        FatigueScores = fatigueScores,
        //                        TrainingWillingnessScores = trainingWillingnessScores,
        //                        AppetiteScores = appetiteScores,
        //                        CompetitionWillingnessScores = competitionWillingnessScores
        //                    }
        //                };
        //                break;

        //            default:
        //                TempData["ErrorMessage"] = "無效的訓練項目";
        //                return RedirectToAction("dashboard", "PhyFit");
        //        }

        //        return View(combinedViewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #endregion

        #region 讀取session訓練量結果
        public ActionResult LoadSessionRPETrainingRecords(string item, bool isAthlete)
        {
            try
            {
                var model = new TrainingRecordViewModel { TrainingItem = item };

                if (isAthlete)
                {
                    model.GeneralTrainingRecord = _db.AthleteGeneralTrainingRecord
                        .Select(record => new GeneralTrainingRecordViewModel
                        {
                            TrainingDate = record.TrainingDate ?? DateTime.Now,
                            AthleteName = record.Athlete,
                            TrainingClassName = record.TrainingClassName,
                            TrainingItem = record.TrainingItem,
                            ActionName = record.ActionName,
                            TrainingTime = record.TrainingTime,
                            RPEscore = record.RPEscore ?? 0,
                            EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        })
                        .ToList();

                    model.ArcheryRecords = _db.AthleteArcheryTrainingRecord
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

                    model.ShootingRecords = _db.AthleteShootingRecord
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
                }
                else
                {
                    model.GeneralTrainingRecord = _db.GeneralTrainingRecord
                        .Select(record => new GeneralTrainingRecordViewModel
                        {
                            TrainingDate = record.TrainingDate ?? DateTime.Now,
                            AthleteName = record.Athlete,
                            TrainingClassName = record.TrainingClassName,
                            TrainingItem = record.TrainingItem,
                            ActionName = record.ActionName,
                            TrainingTime = record.TrainingTime,
                            RPEscore = record.RPEscore ?? 0,
                            EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        })
                        .ToList();

                    model.ArcheryRecords = _db.ArcheryRecord
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

                    model.ShootingRecords = _db.ShootingRecord
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
                }

                return View("SessionRecord", model);
            }
            catch (Exception ex)
            {
                // 記錄例外
                Console.WriteLine($"發生錯誤: {ex.Message}");
                throw;
            }
        }
        //public ActionResult LoadSessionRPETrainingRecords(string item, bool isAthlete)
        //{
        //    try
        //    {
        //        // 建立 TrainingRecordViewModel 並傳入訓練項目名稱
        //        var model = new TrainingRecordViewModel { TrainingItem = item };

        //        // 根據用戶角色選擇正確的表來讀取數據
        //        if (isAthlete)
        //        {
        //            var sessionRPERecords = _db.AthleteGeneralTrainingRecord.ToList();
        //            var archeryRecords = _db.AthleteArcheryTrainingRecord.ToList();
        //            var shootingRecords = _db.AthleteShootingRecord.ToList();

        //            // 整合運動員的訓練數據，並存入viewModel
        //            model.GeneralTrainingRecord = sessionRPERecords
        //                .Select(record => new GeneralTrainingRecordViewModel
        //                {
        //                    TrainingDate = record.TrainingDate ?? DateTime.Now,
        //                    AthleteName = record.Athlete,
        //                    TrainingClassName = record.TrainingClassName,
        //                    TrainingItem = record.TrainingItem,
        //                    ActionName = record.ActionName,
        //                    TrainingTime = record.TrainingTime,
        //                    RPEscore = record.RPEscore ?? 0,
        //                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
        //                    // 可以繼續添加其他字段
        //                })
        //                .ToList();

        //            model.ArcheryRecords = archeryRecords
        //                .Select(record => new ArcheryTrainingRecordViewModel
        //                {
        //                    TrainingDate = record.TrainingDate ?? DateTime.Now,
        //                    Coach = record.Coach,
        //                    Athlete = record.Athlete,
        //                    Poundage = record.Poundage ?? 0,
        //                    ArrowCount = record.ArrowCount ?? 0,
        //                    RPEscore = record.RPEscore ?? 0,
        //                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
        //                })
        //                .ToList();

        //            model.ShootingRecords = shootingRecords
        //                .Select(record => new ShootingTrainingRecordViewModel
        //                {
        //                    TrainingDate = record.TrainingDate ?? DateTime.Now,
        //                    Coach = record.Coach,
        //                    Athlete = record.Athlete,
        //                    ShootingTool = record.ShootingTool,
        //                    BulletCount = record.BulletCount ?? 0,
        //                    RPEscore = record.RPEscore ?? 0,
        //                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
        //                })
        //                .ToList();
        //        }
        //        else
        //        {
        //            var sessionRPERecords = _db.GeneralTrainingRecord.ToList();
        //            var archeryRecords = _db.ArcheryRecord.ToList();
        //            var shootingRecords = _db.ShootingRecord.ToList();

        //            // 整合教練的訓練數據，並存入viewModel
        //            model.GeneralTrainingRecord = sessionRPERecords
        //                .Select(record => new GeneralTrainingRecordViewModel
        //                {
        //                    TrainingDate = record.TrainingDate ?? DateTime.Now,
        //                    AthleteName = record.Athlete,
        //                    TrainingClassName = record.TrainingClassName,
        //                    TrainingItem = record.TrainingItem,
        //                    ActionName = record.ActionName,
        //                    TrainingTime = record.TrainingTime,
        //                    RPEscore = record.RPEscore ?? 0,
        //                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
        //                })
        //                .ToList();

        //            model.ArcheryRecords = archeryRecords
        //                .Select(record => new ArcheryTrainingRecordViewModel
        //                {
        //                    TrainingDate = record.TrainingDate ?? DateTime.Now,
        //                    Coach = record.Coach,
        //                    Athlete = record.Athlete,
        //                    Poundage = record.Poundage ?? 0,
        //                    ArrowCount = record.ArrowCount ?? 0,
        //                    RPEscore = record.RPEscore ?? 0,
        //                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
        //                })
        //                .ToList();

        //            model.ShootingRecords = shootingRecords
        //                .Select(record => new ShootingTrainingRecordViewModel
        //                {
        //                    TrainingDate = record.TrainingDate ?? DateTime.Now,
        //                    Coach = record.Coach,
        //                    Athlete = record.Athlete,
        //                    ShootingTool = record.ShootingTool,
        //                    BulletCount = record.BulletCount ?? 0,
        //                    RPEscore = record.RPEscore ?? 0,
        //                    EachTrainingLoad = record.EachTrainingLoad ?? 0,
        //                })
        //                .ToList();
        //        }

        //        // 返回主視圖 (SessionRecord.cshtml)
        //        return View("SessionRecord", model);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        #endregion

        #region 計算session RPE指標結果
        [HttpGet]
        public JsonResult CalculateTrainingLoad(DateTime date, string trainingType, bool isAthlete)
        {
            try
            {
                DateTime selectedDate = date.Date;

                int totalTrainingLoad = 0;
                int dailyTrainingLoadSum = 0;
                int weeklyTrainingLoadSum = 0;
                double trainingMonotony = 0;
                double trainingStrain = 0;
                double weekToWeekChange = 0;
                double acwr = 0;

                if (isAthlete)
                {
                    if (trainingType == "一般訓練衝量監控 (session-RPE)")
                    {
                        var sessionRPERecords = _db.AthleteGeneralTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                            .Select(record => new
                            {
                                record.EachTrainingLoad
                            })
                            .ToList();

                        totalTrainingLoad = sessionRPERecords.Sum(r => r.EachTrainingLoad ?? 0);
                    }
                    else if (trainingType == "射箭訓練衝量")
                    {
                        var archeryRecords = _db.AthleteArcheryTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                            .Select(record => new
                            {
                                record.EachTrainingLoad
                            })
                            .ToList();

                        totalTrainingLoad = archeryRecords.Sum(r => r.EachTrainingLoad ?? 0);
                    }
                    else if (trainingType == "射擊訓練衝量")
                    {
                        var shootingRecords = _db.AthleteShootingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                            .Select(record => new
                            {
                                record.EachTrainingLoad
                            })
                            .ToList();

                        totalTrainingLoad = shootingRecords.Sum(r => r.EachTrainingLoad ?? 0);
                    }
                    else
                    {
                        throw new ArgumentException("未知的訓練類型");
                    }
                }
                else
                {
                    if (trainingType == "一般訓練衝量監控 (session-RPE)")
                    {
                        var sessionRPERecords = _db.GeneralTrainingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                            .Select(record => new
                            {
                                record.EachTrainingLoad
                            })
                            .ToList();

                        totalTrainingLoad = sessionRPERecords.Sum(r => r.EachTrainingLoad ?? 0);
                    }
                    else if (trainingType == "射箭訓練衝量")
                    {
                        var archeryRecords = _db.ArcheryRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                            .Select(record => new
                            {
                                record.EachTrainingLoad
                            })
                            .ToList();

                        totalTrainingLoad = archeryRecords.Sum(r => r.EachTrainingLoad ?? 0);
                    }
                    else if (trainingType == "射擊訓練衝量")
                    {
                        var shootingRecords = _db.ShootingRecord
                            .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                            .Select(record => new
                            {
                                record.EachTrainingLoad
                            })
                            .ToList();

                        totalTrainingLoad = shootingRecords.Sum(r => r.EachTrainingLoad ?? 0);
                    }
                    else
                    {
                        throw new ArgumentException("未知的訓練類型");
                    }
                }

                // 計算其他數據
                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType, isAthlete);
                trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType, isAthlete);
                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType, isAthlete);
                acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType, isAthlete);

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
        //[HttpGet]
        //public JsonResult CalculateTrainingLoad(DateTime date, string trainingType, bool isAthlete)
        //{
        //    try
        //    {
        //        // 移除時間部分，只保留年月日
        //        DateTime selectedDate = date.Date;

        //        int totalTrainingLoad = 0;
        //        int dailyTrainingLoadSum = 0;
        //        int weeklyTrainingLoadSum = 0;
        //        double trainingMonotony = 0;
        //        double trainingStrain = 0;
        //        double weekToWeekChange = 0;
        //        double acwr = 0;

        //        // 根據不同的訓練類型和用戶角色進行查詢和計算
        //        if (isAthlete)
        //        {
        //            if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //            {
        //                var sessionRPERecords = _db.AthleteGeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
        //                    .ToList();

        //                totalTrainingLoad = sessionRPERecords.Sum(r => r.EachTrainingLoad ?? 0);
        //                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType, isAthlete);
        //                trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType, isAthlete);
        //                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType, isAthlete);
        //                acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType, isAthlete);
        //            }
        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                var archeryRecords = _db.AthleteArcheryTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
        //                    .ToList();

        //                totalTrainingLoad = archeryRecords.Sum(r => r.EachTrainingLoad ?? 0);
        //                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType, isAthlete);
        //                trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType, isAthlete);
        //                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType, isAthlete);
        //                acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType, isAthlete);
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                var shootingRecords = _db.AthleteShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
        //                    .ToList();

        //                totalTrainingLoad = shootingRecords.Sum(r => r.EachTrainingLoad ?? 0);
        //                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType, isAthlete);
        //                trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType, isAthlete);
        //                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType, isAthlete);
        //                acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType, isAthlete);
        //            }
        //            else
        //            {
        //                throw new ArgumentException("未知的訓練類型");
        //            }
        //        }
        //        else
        //        {
        //            if (trainingType == "一般訓練衝量監控 (session-RPE)")
        //            {
        //                var sessionRPERecords = _db.GeneralTrainingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
        //                    .ToList();

        //                totalTrainingLoad = sessionRPERecords.Sum(r => r.EachTrainingLoad ?? 0);
        //                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType, isAthlete);
        //                trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType, isAthlete);
        //                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType, isAthlete);
        //                acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType, isAthlete);
        //            }
        //            else if (trainingType == "射箭訓練衝量")
        //            {
        //                var archeryRecords = _db.ArcheryRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
        //                    .ToList();

        //                totalTrainingLoad = archeryRecords.Sum(r => r.EachTrainingLoad ?? 0);
        //                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType, isAthlete);
        //                trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType, isAthlete);
        //                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType, isAthlete);
        //                acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType, isAthlete);
        //            }
        //            else if (trainingType == "射擊訓練衝量")
        //            {
        //                var shootingRecords = _db.ShootingRecord
        //                    .Where(record => DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
        //                    .ToList();

        //                totalTrainingLoad = shootingRecords.Sum(r => r.EachTrainingLoad ?? 0);
        //                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, selectedDate, trainingType, isAthlete);
        //                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, selectedDate, trainingType, isAthlete);
        //                trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, selectedDate, trainingType, isAthlete);
        //                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, selectedDate, trainingType, isAthlete);
        //                acwr = TrainingRecordHelper.CalculateACWR(_db, selectedDate, trainingType, isAthlete);
        //            }
        //            else
        //            {
        //                throw new ArgumentException("未知的訓練類型");
        //            }
        //        }

        //        // 返回計算結果
        //        return Json(new
        //        {
        //            TrainingLoad = totalTrainingLoad,
        //            DailyTrainingLoadSum = dailyTrainingLoadSum,
        //            WeeklyTrainingLoadSum = weeklyTrainingLoadSum,
        //            TrainingMonotony = trainingMonotony,
        //            TrainingStrain = trainingStrain,
        //            WeekToWeekChange = weekToWeekChange,
        //            ACWR = acwr
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        #endregion

        #region 教練儲存sessionRPE訓練量結果

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

        #region 儲存運動員一般訓練紀錄
        public ActionResult SaveAthleteTrainingRecord(List<AthleteGeneralTrainingRecord> records)
        {
            try
            {
                // 確認用戶是運動員
                if (Session["UserRole"]?.ToString() != "Athlete")
                {
                    return Json(new { success = false, message = "請確認是否為運動員身份。" });
                }

                foreach (var record in records)
                {
                    // 根據 TrainingClassName 設置相應的屬性
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

                    _db.AthleteGeneralTrainingRecord.Add(record);
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

        #region 儲存運動員射箭訓練紀錄

        public ActionResult SaveAthleteArcheryRecord(List<AthleteArcheryTrainingRecord> records, List<AthleteArcheryRPERecord> sessionRecords)
        {
            try
            {
                // 確認用戶是運動員
                if (Session["UserRole"]?.ToString() != "Athlete")
                {
                    return Json(new { success = false, message = "請確認是否為運動員身份。" });
                }

                // 確保 records 和 sessionRecords 數量對應，避免多次儲存
                if (records.Count != sessionRecords.Count)
                {
                    return Json(new { success = false, message = "記錄數量不匹配，請檢查輸入資料。" });
                }

                // 將 sessionRecords 和 records 逐一對應
                for (int i = 0; i < sessionRecords.Count; i++)
                {
                    var sessionRecord = sessionRecords[i];
                    sessionRecord.CreatedDate = DateTime.Now; // 設定 CreatedDate

                    _db.AthleteArcheryRPERecord.Add(sessionRecord);
                    _db.SaveChanges(); // 儲存當前的 RPE 記錄

                    // 將對應的訓練記錄與當前的 sessionRecord 綁定
                    var correspondingRecord = records[i];
                    correspondingRecord.SessionRPEAthleteRecordID = sessionRecord.ID;
                    _db.AthleteArcheryTrainingRecord.Add(correspondingRecord);
                }

                _db.SaveChanges(); //最後一次性儲存所有訓練記錄

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region 儲存運動員射擊訓練紀錄
        public ActionResult SaveAthleteShootingRecord(List<AthleteShootingRecord> records, List<AthleteShootingSessionRPERecord> sessionRecords)
        {
            try
            {
                // 確認用戶是運動員
                if (Session["UserRole"]?.ToString() != "Athlete")
                {
                    return Json(new { success = false, message = "請確認是否為運動員身份。" });
                }

                // 檢查 sessionRecords 和 records 數量是否一致
                if (records.Count != sessionRecords.Count)
                {
                    return Json(new { success = false, message = "記錄數量不匹配，請檢查輸入資料。" });
                }

                // 逐一儲存每一筆 RPE 記錄及其對應的訓練記錄
                for (int i = 0; i < sessionRecords.Count; i++)
                {
                    var sessionRecord = sessionRecords[i];
                    sessionRecord.CreatedDate = DateTime.Now; // 設定 CreatedDate

                    _db.AthleteShootingSessionRPERecord.Add(sessionRecord);
                    _db.SaveChanges(); // 儲存 RPE 記錄

                    // 將當前 sessionRecord 的 ID 設定到對應的 ShootingRecord
                    var correspondingRecord = records[i];
                    correspondingRecord.SessionRPEShottingRecordID = sessionRecord.ID;

                    _db.AthleteShootingRecord.Add(correspondingRecord);
                }

                _db.SaveChanges(); //儲存所有 Shooting 訓練記錄

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
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
                    //TrainingVolume = model.TrainingVolume,
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
                            //ForceDuration = float.Parse(model.ForceDurations[i]), //力竭時間
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

        #region 處理月份篩選
        public ActionResult SessionRecordByMonth(int? AthleteID, int year, int month)
        {
            try
            {
                //篩選符合年份和月份的心理特質與食慾數據
                var dtos = _db.PsychologicalTraitsResults.Where(dt => dt.UserID == AthleteID && dt.PsychologicalDate.Year == year && dt.PsychologicalDate.Month == month).OrderBy(dt => dt.PsychologicalDate).ToList();

                // 初始化數據集合
                var dates = dtos.Select(dt => dt.PsychologicalDate.ToString("yyyy-MM-dd")).Distinct().ToList();
                var sleepQualityScores = new List<int>();
                var fatigueScores = new List<int>();
                var trainingWillingnessScores = new List<int>();
                var appetiteScores = new List<int>();
                var competitionWillingnessScores = new List<int>();

                // 根據 Trait 將數據分配到對應的 List
                foreach (var record in dtos)
                {
                    switch (record.Trait)
                    {
                        case "睡眠品質":
                            sleepQualityScores.Add(record.Score);
                            break;
                        case "疲憊程度":
                            fatigueScores.Add(record.Score);
                            break;
                        case "訓練意願":
                            trainingWillingnessScores.Add(record.Score);
                            break;
                        case "胃口":
                            appetiteScores.Add(record.Score);
                            break;
                        case "比賽意願":
                            competitionWillingnessScores.Add(record.Score);
                            break;
                    }
                }

                return Json(new
                {
                    dates = dates,
                    sleepQualityScores = sleepQualityScores,
                    fatigueScores = fatigueScores,
                    trainingWillingnessScores = trainingWillingnessScores,
                    appetiteScores = appetiteScores,
                    competitionWillingnessScores = competitionWillingnessScores
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        #endregion
    }
}