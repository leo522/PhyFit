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
using OfficeOpenXml;
using System.IO;

namespace PhysicalFit.Controllers
{
    public class RecordController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        #region 查詢訓練紀錄
        public ActionResult SessionRecord(string item, int? AthleteID, DateTime? date, string data)
        {
            try
            {
                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(data));
                        var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(decodedData);

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

                ViewBag.SelectedTrainingItem = item;
                ViewBag.AthleteID = AthleteID;
                ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
                var userRole = Session["UserRole"]?.ToString();
                var loggedInAthleteID = AthleteID;

                if (userRole == "Athlete")
                {
                    AthleteID = loggedInAthleteID;
                }

                if (userRole == "Coach" && AthleteID == null)
                {
                    TempData["ErrorMessage"] = "請選擇運動員";
                    return RedirectToAction("dashboard", "PhyFit");
                }

                var combinedViewModel = new CombinedViewModel();
                combinedViewModel.TrainingRecord = new TrainingRecordViewModel
                {
                    TrainingItem = item,
                    GeneralTrainingRecord = new List<GeneralTrainingRecordViewModel>(),
                    ArcheryRecords = new List<ArcheryTrainingRecordViewModel>(),
                    ShootingRecords = new List<ShootingTrainingRecordViewModel>(),
                    DetectionRecords = new List<DetectionTrainingRecordViewModel>()
                };

                var athlete = _db.Athletes.FirstOrDefault(a => a.ID == AthleteID);
                var coachNames = _db.AthleteCoachRelations
                    .Where(r => r.AthleteID == athlete.ID)
                    .Select(r => r.Coaches.CoachName)
                    .ToList();

                combinedViewModel.TrainingRecord.CoachName = coachNames.Any()
                    ? string.Join("、", coachNames)
                    : "未設定";

                combinedViewModel.TrainingRecord.AthleteName = athlete?.AthleteName ?? "無資料";

                if (item != "檢測系統")
                {
                    // 一般訓練衝量監控
                    if (userRole == "Athlete")
                    {
                        var generalTrainingRecords = _db.AthleteGeneralTrainingRecord
                            .Where(x => x.AthleteID == AthleteID);

                        if (date.HasValue)
                        {
                            DateTime endDate = date.Value.Date;
                            DateTime startDate = endDate.AddDays(-6);

                            generalTrainingRecords = generalTrainingRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                        }

                        combinedViewModel.TrainingRecord.GeneralTrainingRecord = generalTrainingRecords
                            .OrderBy(x => x.TrainingDate)
                            .Select(x => new GeneralTrainingRecordViewModel
                            {
                                ID = x.ID,
                                TrainingName = x.TrainingClassName,
                                TrainingDate = x.TrainingDate ?? DateTime.Now,
                                Coach = x.Coach,
                                Athlete = x.Athlete,
                                TrainingItem = x.TrainingItem,
                                ActionName = x.ActionName,
                                TrainingOther = x.TrainingOther,
                                TrainingType = x.TrainingType,
                                TrainingParts = x.TrainingParts,
                                TrainingTime = x.TrainingTime,
                                RPEscore = x.RPEscore ?? 0,
                                EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                Source = "Athlete"
                            }).ToList();
                    }
                    else
                    {
                        var athleteRecords = _db.AthleteGeneralTrainingRecord
                            .Where(x => x.AthleteID == AthleteID);
                        var generalTrainingRecords = _db.GeneralTrainingRecord
                            .Where(x => x.AthleteID == AthleteID);

                        if (date.HasValue)
                        {
                            DateTime endDate = date.Value.Date;
                            DateTime startDate = endDate.AddDays(-6);

                            athleteRecords = athleteRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);

                            generalTrainingRecords = generalTrainingRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                        }

                        var combined = athleteRecords
                            .Select(x => new GeneralTrainingRecordViewModel
                            {
                                ID = x.ID,
                                TrainingName = x.TrainingClassName,
                                TrainingDate = x.TrainingDate ?? DateTime.Now,
                                Coach = x.Coach,
                                Athlete = x.Athlete,
                                TrainingItem = x.TrainingItem,
                                ActionName = x.ActionName,
                                TrainingOther = x.TrainingOther,
                                TrainingType = x.TrainingType,
                                TrainingParts = x.TrainingParts,
                                TrainingTime = x.TrainingTime,
                                RPEscore = x.RPEscore ?? 0,
                                EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                Source = "Athlete"
                            }).ToList();

                        combined.AddRange(
                            generalTrainingRecords
                                .OrderBy(x => x.TrainingDate)
                                .Select(x => new GeneralTrainingRecordViewModel
                                {
                                    ID = x.ID,
                                    TrainingName = x.TrainingClassName,
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    TrainingItem = x.TrainingItem,
                                    ActionName = x.ActionName,
                                    TrainingOther = x.TrainingOther,
                                    TrainingType = x.TrainingType,
                                    TrainingParts = x.TrainingParts,
                                    TrainingTime = x.TrainingTime,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                    Source = "Coach"
                                })
                        );

                        combinedViewModel.TrainingRecord.GeneralTrainingRecord = combined.OrderBy(x => x.TrainingDate).ToList();
                    }

                    // 射箭訓練衝量
                    if (userRole == "Athlete")
                    {
                        var archeryRecords = _db.AthleteArcheryTrainingRecord.Where(x => x.AthleteID == AthleteID);
                        if (date.HasValue)
                        {
                            DateTime endDate = date.Value.Date;
                            DateTime startDate = endDate.AddDays(-6);
                            archeryRecords = archeryRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                        }

                        combinedViewModel.TrainingRecord.ArcheryRecords = archeryRecords
                            .OrderBy(x => x.TrainingDate)
                            .Select(x => new ArcheryTrainingRecordViewModel
                            {
                                ID = x.ID,
                                TrainingDate = x.TrainingDate ?? DateTime.Now,
                                Coach = x.Coach,
                                Athlete = x.Athlete,
                                Poundage = x.Poundage ?? 0,
                                ArrowCount = x.ArrowCount ?? 0,
                                RPEscore = x.RPEscore ?? 0,
                                EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                Source = "Athlete"
                            }).ToList();
                    }
                    else
                    {
                        var athleteRecords = _db.AthleteArcheryTrainingRecord.Where(x => x.AthleteID == AthleteID);
                        var archeryRecords = _db.ArcheryRecord.Where(x => x.AthleteID == AthleteID);

                        if (date.HasValue)
                        {
                            DateTime endDate = date.Value.Date;
                            DateTime startDate = endDate.AddDays(-6);

                            athleteRecords = athleteRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);

                            archeryRecords = archeryRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                        }

                        var combined = athleteRecords
                            .Select(x => new ArcheryTrainingRecordViewModel
                            {
                                ID = x.ID,
                                TrainingDate = x.TrainingDate ?? DateTime.Now,
                                Coach = x.Coach,
                                Athlete = x.Athlete,
                                Poundage = x.Poundage ?? 0,
                                ArrowCount = x.ArrowCount ?? 0,
                                RPEscore = x.RPEscore ?? 0,
                                EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                Source = "Athlete"
                            }).ToList();

                        combined.AddRange(
                            archeryRecords.OrderBy(x => x.TrainingDate)
                                .Select(x => new ArcheryTrainingRecordViewModel
                                {
                                    ID = x.ID,
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    Poundage = x.Poundage ?? 0,
                                    ArrowCount = x.ArrowCount ?? 0,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                    Source = "Coach"
                                })
                        );

                        combinedViewModel.TrainingRecord.ArcheryRecords = combined.OrderBy(x => x.TrainingDate).ToList();
                    }

                    // 射擊訓練衝量
                    if (userRole == "Athlete")
                    {
                        var shootingRecords = _db.AthleteShootingRecord.Where(x => x.AthleteID == AthleteID);
                        if (date.HasValue)
                        {
                            DateTime endDate = date.Value.Date;
                            DateTime startDate = endDate.AddDays(-6);
                            shootingRecords = shootingRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                        }

                        combinedViewModel.TrainingRecord.ShootingRecords = shootingRecords
                            .OrderBy(x => x.TrainingDate)
                            .Select(x => new ShootingTrainingRecordViewModel
                            {
                                ID = x.ID,
                                TrainingDate = x.TrainingDate ?? DateTime.Now,
                                Coach = x.Coach,
                                Athlete = x.Athlete,
                                ShootingTool = x.ShootingTool,
                                BulletCount = x.BulletCount ?? 0,
                                RPEscore = x.RPEscore ?? 0,
                                EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                Source = "Athlete"
                            }).ToList();
                    }
                    else
                    {
                        var athleteRecords = _db.AthleteShootingRecord.Where(x => x.AthleteID == AthleteID);
                        var shootingRecords = _db.ShootingRecord.Where(x => x.AthleteID == AthleteID);

                        if (date.HasValue)
                        {
                            DateTime endDate = date.Value.Date;
                            DateTime startDate = endDate.AddDays(-6);
                            athleteRecords = athleteRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                            shootingRecords = shootingRecords
                                .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate &&
                                            DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                        }

                        var combined = athleteRecords
                            .Select(x => new ShootingTrainingRecordViewModel
                            {
                                ID = x.ID,
                                TrainingDate = x.TrainingDate ?? DateTime.Now,
                                Coach = x.Coach,
                                Athlete = x.Athlete,
                                ShootingTool = x.ShootingTool,
                                BulletCount = x.BulletCount ?? 0,
                                RPEscore = x.RPEscore ?? 0,
                                EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                Source = "Athlete"
                            }).ToList();

                        combined.AddRange(
                            shootingRecords.OrderBy(x => x.TrainingDate)
                                .Select(x => new ShootingTrainingRecordViewModel
                                {
                                    ID = x.ID,
                                    TrainingDate = x.TrainingDate ?? DateTime.Now,
                                    Coach = x.Coach,
                                    Athlete = x.Athlete,
                                    ShootingTool = x.ShootingTool,
                                    BulletCount = x.BulletCount ?? 0,
                                    RPEscore = x.RPEscore ?? 0,
                                    EachTrainingLoad = x.EachTrainingLoad ?? 0,
                                    Source = "Coach"
                                })
                        );

                        combinedViewModel.TrainingRecord.ShootingRecords = combined.OrderBy(x => x.TrainingDate).ToList();
                    }
                }
                else
                {
                    var sportItems = _db.DetectionTraining
                            .Where(r => r.ItemName != "跑步機" && r.ItemName != "自由車")  // 暫不顯示跑步機和自由車
                            .Select(r => r.ItemName)
                            .ToList();

                    ViewBag.SportItems = sportItems;

                    var detectionRecords = _db.DetectionTrainingRecord
                        .Where(x => x.AthleteID == AthleteID);

                    if (date.HasValue)
                    {
                        DateTime endDate = date.Value.Date;
                        DateTime startDate = endDate.AddDays(-6);

                        detectionRecords = detectionRecords
                            .Where(x => DbFunctions.TruncateTime(x.TrainingDate) >= startDate && DbFunctions.TruncateTime(x.TrainingDate) <= endDate);
                    }

                    detectionRecords = detectionRecords
                        .GroupBy(x => x.SportItem)
                        .SelectMany(g => g
                            .OrderByDescending(x => x.ID)
                            .Take(12))
                        .OrderByDescending(x => x.TrainingDate);

                    combinedViewModel.TrainingRecord.DetectionRecords = detectionRecords
                        .Select(x => new DetectionTrainingRecordViewModel
                        {
                            ID = x.ID,
                            TrainingDate = x.TrainingDate ?? DateTime.Now,
                            Coach = x.Coach,
                            Athlete = x.Athlete,
                            DetectionItem = x.DetectionItem,
                            SportItem = x.SportItem,
                            CriticalSpeed = x.CriticalSpeed ?? 0,
                            MaxAnaerobicWork = x.MaxAnaerobicWork ?? 0,
                            RollerSkill = x.RollerSkill ?? 0,
                            TrainingVolume = x.TrainingVolume ?? 0,
                            TrainingPrescription = x.TrainingPrescription ?? 0,
                            CoefficientOfDetermination = x.CoefficientOfDetermination ?? 0,
                        }).ToList();
                }

                return View(combinedViewModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 匯出28日紀錄
        public JsonResult CheckExportData(DateTime selectedDate, int athleteID, string role)
        {
            var start = selectedDate.AddDays(-27);
            bool hasData = false;

            if (role == "Athlete")
            {
                hasData = _db.AthleteGeneralTrainingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.AthleteArcheryTrainingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.AthleteShootingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.PsychologicalTraitsResults.Any(r => r.UserID == athleteID && r.PsychologicalDate >= start && r.PsychologicalDate <= selectedDate);
            }
            else if (role == "Coach")
            {
                hasData = _db.AthleteGeneralTrainingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.AthleteArcheryTrainingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.AthleteShootingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.GeneralTrainingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.ArcheryRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.ShootingRecord.Any(r => r.AthleteID == athleteID && r.TrainingDate >= start && r.TrainingDate <= selectedDate) ||
                          _db.PsychologicalTraitsResults.Any(r => r.UserID == athleteID && r.PsychologicalDate >= start && r.PsychologicalDate <= selectedDate);
            }

            if (hasData)
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { success = false, message = "無符合條件的資料，請確認日期與選手是否正確。" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportRawData(DateTime selectedDate, int athleteID, string athleteName, string role)
        {
            var startDate = selectedDate.AddDays(-27);
            var fileName = $"{startDate:yyyy-MM-dd} ~ {selectedDate:yyyy-MM-dd} - {athleteName}.xlsx";
            ExcelPackage.License.SetNonCommercialOrganization("My Noncommercial organization");
            using (var package = new ExcelPackage())
            {
                if (role == "Athlete")
                {
                    AddWorksheet(package, "一般訓練衝量", _db.AthleteGeneralTrainingRecord, athleteID, startDate, selectedDate);
                    AddWorksheet(package, "射箭訓練衝量", _db.AthleteArcheryTrainingRecord, athleteID, startDate, selectedDate);
                    AddWorksheet(package, "射擊訓練衝量", _db.AthleteShootingRecord, athleteID, startDate, selectedDate);
                    var traitGroups = _db.PsychologicalTraitsResults
                        .Where(r => r.UserID == athleteID && r.PsychologicalDate >= startDate && r.PsychologicalDate <= selectedDate)
                        .GroupBy(r => r.Trait)
                        .ToList();

                    foreach (var group in traitGroups)
                    {
                        var sheetName = group.Key;
                        AddCustomWorksheet(package, sheetName, group.ToList());
                    }
                }
                else if (role == "Coach")
                {
                    AddWorksheet(package, "一般訓練衝量 (教練)", _db.GeneralTrainingRecord, athleteID, startDate, selectedDate);
                    AddWorksheet(package, "一般訓練衝量 (選手)", _db.AthleteGeneralTrainingRecord, athleteID, startDate, selectedDate);
                    
                    AddWorksheet(package, "射箭訓練衝量 (教練)", _db.ArcheryRecord, athleteID, startDate, selectedDate);
                    AddWorksheet(package, "射箭訓練衝量 (選手)", _db.AthleteArcheryTrainingRecord, athleteID, startDate, selectedDate);
                    
                    AddWorksheet(package, "射擊訓練衝量 (教練)", _db.ShootingRecord, athleteID, startDate, selectedDate);
                    AddWorksheet(package, "射擊訓練衝量 (選手)", _db.AthleteShootingRecord, athleteID, startDate, selectedDate);

                    var traitGroups = _db.PsychologicalTraitsResults
                        .Where(r => r.UserID == athleteID && r.PsychologicalDate >= startDate && r.PsychologicalDate <= selectedDate)
                        .GroupBy(r => r.Trait)
                        .ToList();

                    foreach (var group in traitGroups)
                    {
                        var sheetName = group.Key;
                        AddCustomWorksheet(package, sheetName, group.ToList());
                    }
                }

                if (package.Workbook.Worksheets.Count == 0)
                {
                    return Content("無符合條件的資料，因此未產生任何工作表。");
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        private void AddWorksheet<T>(ExcelPackage package, string sheetName, DbSet<T> table, int athleteID, DateTime start, DateTime end) where T : class
        {
            if (!SheetFieldMappings.TryGetValue(sheetName, out var fieldMap) || fieldMap.Count == 0)
                return;

            var allData = table.ToList();

            // 過濾 AthleteID 與 Date 範圍
            var filteredData = allData.Where(entry =>
            {
                var athleteIdProp = entry.GetType().GetProperty("AthleteID");
                var dateProp = entry.GetType().GetProperty("TrainingDate");

                // 若找不到必要欄位就排除
                if (athleteIdProp == null || dateProp == null)
                    return false;

                var entryAthleteId = athleteIdProp.GetValue(entry) as int?;
                var entryDate = dateProp.GetValue(entry) as DateTime?;

                return entryAthleteId == athleteID && entryDate >= start && entryDate <= end;
            }).ToList();

            filteredData = filteredData.OrderBy(entry =>
            {
                var dateProp = entry.GetType().GetProperty("TrainingDate");
                return dateProp?.GetValue(entry) as DateTime? ?? DateTime.MinValue;
            }).ToList();

            // 若無資料則不產生工作表
            if (!filteredData.Any())
                return;

            // 建立工作表
            var ws = package.Workbook.Worksheets.Add(sheetName);

            // 欄位標題（僅使用 fieldMap 設定的欄位）
            int colIndex = 1;
            foreach (var pair in fieldMap)
            {
                ws.Cells[1, colIndex++].Value = pair.Value;
            }

            // 寫資料列
            for (int row = 0; row < filteredData.Count; row++)
            {
                int col = 1;
                foreach (var key in fieldMap.Keys)
                {
                    var prop = typeof(T).GetProperty(key);
                    var cell = ws.Cells[row + 2, col++];

                    // 處理「一般訓練衝量」的 ActionName 欄位
                    if ((sheetName.Contains("一般訓練衝量")) && key == "ActionName")
                    {
                        var trainingOtherProp = typeof(T).GetProperty("TrainingOther");
                        var trainingOtherValue = trainingOtherProp?.GetValue(filteredData[row])?.ToString();

                        if (!string.IsNullOrWhiteSpace(trainingOtherValue))
                        {
                            cell.Value = trainingOtherValue;
                            continue;
                        }
                    }

                    if (prop != null)
                    {
                        var value = prop.GetValue(filteredData[row]);

                        if (value is DateTime dt)
                        {
                            cell.Style.Numberformat.Format = "yyyy-mm-dd HH:mm";
                            cell.Value = dt;
                        }
                        else
                        {
                            cell.Value = value;
                        }
                    }
                }
            }

            ws.Cells.AutoFitColumns();
        }

        private Dictionary<string, Dictionary<string, string>> SheetFieldMappings = new Dictionary<string, Dictionary<string, string>>()
        {
            { "一般訓練衝量", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "TrainingClassName", "課程名稱" },
                { "TrainingItem", "運動種類" },
                { "ActionName", "訓練動作" },
                { "TrainingParts", "訓練部位" },
                { "TrainingType", "訓練類型" },
                { "TrainingTime", "訓練時間" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷量(TL)" }
            }},
            { "射箭訓練衝量", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "Poundage", "磅數" },
                { "ArrowCount", "箭數" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷(TL)" }
            }},
            { "射擊訓練衝量", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "ShootingTool", "射擊工具" },
                { "BulletCount", "子彈數" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷量(TL)" }
            }},
            { "一般訓練衝量 (教練)", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "TrainingClassName", "課程名稱" },
                { "TrainingItem", "運動種類" },
                { "ActionName", "訓練動作" },
                { "TrainingParts", "訓練部位" },
                { "TrainingType", "訓練類型" },
                { "TrainingTime", "訓練時間" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷量(TL)" }
            }},
            { "射箭訓練衝量 (教練)", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "Poundage", "磅數" },
                { "ArrowCount", "箭數" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷(TL)" }
            }},
            { "射擊訓練衝量 (教練)", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "ShootingTool", "射擊工具" },
                { "BulletCount", "子彈數" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷量(TL)" }
            }},
            { "一般訓練衝量 (選手)", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "TrainingClassName", "課程名稱" },
                { "TrainingItem", "運動種類" },
                { "ActionName", "訓練動作" },
                { "TrainingParts", "訓練部位" },
                { "TrainingType", "訓練類型" },
                { "TrainingTime", "訓練時間" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷量(TL)" }
            }},
            { "射箭訓練衝量 (選手)", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "Poundage", "磅數" },
                { "ArrowCount", "箭數" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷(TL)" }
            }},
            { "射擊訓練衝量 (選手)", new Dictionary<string, string> {
                { "TrainingDate", "訓練日期" },
                { "ShootingTool", "射擊工具" },
                { "BulletCount", "子彈數" },
                { "RPEscore", "自覺費力程度" },
                { "EachTrainingLoad", "單次運動負荷量(TL)" }
            }}
        };

        private void AddCustomWorksheet<T>(ExcelPackage package, string sheetName, List<T> data) where T : class
        {
            if (data == null || !data.Any()) return;

            data = data.OrderBy(d =>
            {
                var prop = typeof(T).GetProperty("PsychologicalDate");
                return prop?.GetValue(d) as DateTime? ?? DateTime.MinValue;
            }).ToList();

            var ws = package.Workbook.Worksheets.Add(sheetName);
            // 欄位名單（只顯示這些欄位）
            var allowedFields = new List<string> { "PsychologicalDate", "Feeling", "Score" };

            // 對應中文欄名
            var fieldDisplayMap = new Dictionary<string, string>
            {
                { "PsychologicalDate", "日期" },
                { "Feeling", "感受" },
                { "Score", "分數" }
            };

            // 取得欄位對應的 PropertyInfo
            var propertyMap = typeof(T).GetProperties()
                .Where(p => allowedFields.Contains(p.Name))
                .ToDictionary(p => p.Name, p => p);

            // 標題列
            int col = 1;
            foreach (var fieldName in allowedFields)
            {
                var displayName = fieldDisplayMap.ContainsKey(fieldName) ? fieldDisplayMap[fieldName] : fieldName;
                ws.Cells[1, col++].Value = displayName;
            }

            // 資料列
            for (int row = 0; row < data.Count; row++)
            {
                col = 1;
                foreach (var fieldName in allowedFields)
                {
                    if (propertyMap.TryGetValue(fieldName, out var prop))
                    {
                        var value = prop.GetValue(data[row]);
                        var cell = ws.Cells[row + 2, col++];

                        if (value is DateTime dt)
                        {
                            cell.Style.Numberformat.Format = "yyyy-mm-dd";
                            cell.Value = dt;
                        }
                        else
                        {
                            cell.Value = value;
                        }
                    }
                }
            }

            ws.Cells.AutoFitColumns();
        }
        #endregion

        #region 心理特質折線圖
        [HttpGet]
        public JsonResult GetPsychologicalData(int AthleteID, string startDate, string endDate)
        {
            try
            {
                DateTime start = DateTime.Parse(startDate);
                DateTime end = DateTime.Parse(endDate);

                if (end < start)
                {
                    return Json(new { error = "結束日期不能早於起始日期" }, JsonRequestBehavior.AllowGet);
                }

                var records = _db.PsychologicalTraitsResults
                    .Where(r => r.UserID == AthleteID &&
                                DbFunctions.TruncateTime(r.PsychologicalDate) >= start &&
                                DbFunctions.TruncateTime(r.PsychologicalDate) <= end)
                    .OrderBy(r => r.PsychologicalDate)
                    .ToList();

                var dates = records.Select(r => r.PsychologicalDate.ToString("yyyy-MM-dd")).Distinct().ToList();
                var sleepQualityScores = new List<int>();
                var fatigueScores = new List<int>();
                var trainingWillingnessScores = new List<int>();
                var appetiteScores = new List<int>();
                var competitionWillingnessScores = new List<int>();

                foreach (var record in records)
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
                    dates,
                    sleepQualityScores,
                    fatigueScores,
                    trainingWillingnessScores,
                    appetiteScores,
                    competitionWillingnessScores
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 每日TL長條圖（每日 TM / TS）
        [HttpGet]
        public ActionResult GetDailyTrainingLoadData(int? AthleteID, string startDate, string endDate, bool isAthlete = true)
        {
            if (!AthleteID.HasValue || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return Json(new { error = "資料缺漏" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                DateTime start = DateTime.Parse(startDate);
                DateTime end = DateTime.Parse(endDate);

                if (end < start)
                {
                    return Json(new { error = "結束日期不能早於起始日期" }, JsonRequestBehavior.AllowGet);
                }

                var items = new[] { "一般訓練衝量監控 (session-RPE)", "射箭訓練衝量", "射擊訓練衝量" };
                var dates = new List<string>();
                var allSeries = new Dictionary<string, List<double>>();

                // 初始化各訓練項目欄位
                foreach (var item in items)
                {
                    allSeries[item] = new List<double>();
                }

                // 新增每日總 TL 陣列（合併用）
                var totalDailyLoads = new List<double>();

                // 每日加總訓練量
                for (var day = start; day <= end; day = day.AddDays(1))
                {
                    string dateStr = day.ToString("yyyy-MM-dd");
                    dates.Add(dateStr);

                    double totalLoad = 0;

                    foreach (var item in items)
                    {
                        var dailyLoad = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, dateStr, item, isAthlete, AthleteID);
                        allSeries[item].Add(dailyLoad);
                        totalLoad += dailyLoad;
                    }

                    totalDailyLoads.Add(totalLoad);
                }

                // 每日 TM / TS：向前推一週範圍
                var tmList = new List<double>();
                var tsList = new List<double>();

                for (int i = 0; i < dates.Count; i++)
                {
                    var endDateForDay = start.AddDays(i + 1);
                    var startDateForDay = endDateForDay.AddDays(-7);

                    var tm = TrainingRecordHelper.CalculateTrainingMonotonyForAllTypes(_db, startDateForDay, endDateForDay, isAthlete, AthleteID);
                    var ts = TrainingRecordHelper.CalculateTrainingStrainForAllTypes(_db, startDateForDay, endDateForDay, isAthlete, AthleteID);

                    tmList.Add(tm);
                    tsList.Add(ts);
                }

                allSeries["訓練同質性 (TM)"] = tmList;
                allSeries["訓練張力值 (TS)"] = tsList;

                return Json(new { dates, series = allSeries }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 每週TL長條圖
        [HttpGet]
        public ActionResult GetWeeklyTrainingLoadData(int? AthleteID, string startDate, string endDate, bool isAthlete = true)
        {
            if (!AthleteID.HasValue || string.IsNullOrEmpty(startDate) || string.IsNullOrEmpty(endDate))
            {
                return Json(new { error = "資料缺漏" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                DateTime start = DateTime.Parse(startDate).Date;
                DateTime end = DateTime.Parse(endDate).Date;

                if (end < start)
                {
                    return Json(new { error = "結束日期不能早於起始日期" }, JsonRequestBehavior.AllowGet);
                }

                var items = new[] { "一般訓練衝量監控 (session-RPE)", "射箭訓練衝量", "射擊訓練衝量" };
                var allSeries = new Dictionary<string, List<double>>();
                var weekLabels = new List<string>();

                // 初始化 series
                foreach (var item in items)
                    allSeries[item] = new List<double>();

                // 以 endDate 為當週最後一天，往前推每週資料
                for (DateTime weekEnd = end; weekEnd >= start; weekEnd = weekEnd.AddDays(-7))
                {
                    DateTime weekStart = weekEnd.AddDays(-6);
                    if (weekStart < start) weekStart = start;

                    string label = $"{weekStart:yyyy-MM-dd} ~ {weekEnd:yyyy-MM-dd}";
                    weekLabels.Add(label);

                    foreach (var item in items)
                    {
                        double weeklySum = 0;
                        for (var d = weekStart; d <= weekEnd; d = d.AddDays(1))
                        {
                            double load = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, d.ToString("yyyy-MM-dd"), item, isAthlete, AthleteID);
                            weeklySum += load;
                        }
                        allSeries[item].Add(weeklySum);
                    }
                }

                // 時間反轉排序
                weekLabels.Reverse();
                foreach (var key in items)
                    allSeries[key].Reverse();

                return Json(new { dates = weekLabels, series = allSeries }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 檢測系統線性圖
        [HttpGet]
        public JsonResult GetDetectionRegressionData(int athleteId, string sportItem)
        {
            var detectionRecords = _db.DetectionTrainingRecord
                .Where(x => x.AthleteID == athleteId && x.SportItem == sportItem)
                .OrderByDescending(x => x.TrainingDate)
                .Take(12)
                .ToList();

            var regressionData = new List<object>();

            foreach (var record in detectionRecords)
            {
                List<(double x, double y)> dataPoints = new List<(double, double)>();

                switch (sportItem)
                {
                    case "跑步機":
                        dataPoints = _db.TreadmillRecordDetails
                            .Where(d => d.DetectionTrainingRecordId == record.ID)
                            .ToList()
                            .Select(d => {
                                double intensity = 0;
                                if (d.IntenPercen != null && d.IntenPercen.EndsWith("%"))
                                    double.TryParse(d.IntenPercen.Replace("%", ""), out intensity);
                                return ((double)d.ForceDuration, intensity);
                            })
                            .ToList();
                        break;

                    case "田徑場":
                        dataPoints = _db.TrackFieldRecordDetails
                            .Where(d => d.DetectionTrainingRecordId == record.ID)
                            .ToList()
                            .Select(d => {
                                double distance = 0;
                                if (d.Distance != null && d.Distance.EndsWith("m"))
                                    double.TryParse(d.Distance.Replace("m", ""), out distance);
                                return ((double)d.ForceDuration, distance);
                            })
                            .ToList();
                        break;

                    case "游泳":
                        dataPoints = _db.SwimmingRecordDetails
                            .Where(d => d.DetectionTrainingRecordId == record.ID)
                            .ToList()
                            .Select(d => {
                                double distance = 0;
                                if (d.Distance != null && d.Distance.EndsWith("m"))
                                    double.TryParse(d.Distance.Replace("m", ""), out distance);
                                return ((double)d.ForceDuration, distance);
                            })
                            .ToList();
                        break;

                    case "自由車":
                        dataPoints = _db.BikeRecordDetails
                            .Where(d => d.DetectionTrainingRecordId == record.ID)
                            .ToList()
                            .Select(d => {
                                double intensity = 0;
                                if (d.IntenPercen != null && d.IntenPercen.EndsWith("%"))
                                    double.TryParse(d.IntenPercen.Replace("%", ""), out intensity);
                                return ((double)d.ForceDuration, intensity);
                            })
                            .ToList();
                        break;

                    case "滑輪溜冰":
                        var rawDetails = _db.RollerSkatingRecordDetails
                            .Where(d => d.DetectionTrainingRecordId == record.ID)
                            .ToList();

                        double baselineTime = 0;

                        // 先找到 100m(穿溜冰鞋) 的時間作為 baseline
                        foreach (var d in rawDetails)
                        {
                            if (!string.IsNullOrEmpty(d.Distance) && d.Distance.Contains("100m(穿溜冰鞋)"))
                            {
                                baselineTime = (double)d.ForceDuration;
                                break;
                            }
                        }

                        dataPoints = rawDetails
                            .Where(d => {
                                var dist = d.Distance;
                                if (string.IsNullOrEmpty(dist)) return false;
                                // 排除 100m
                                if (dist.Contains("100m")) return false;

                                return true;
                            })
                            .Select(d => {
                                string raw = d.Distance;
                                double distance = 0;

                                // 擷取開頭數字距離
                                var match = System.Text.RegularExpressions.Regex.Match(raw, @"^(\d+)");
                                if (match.Success)
                                    double.TryParse(match.Groups[1].Value, out distance);

                                double time = (double)d.ForceDuration;

                                // 如果距離大於 100，則做 baseline 校正
                                if (distance > 100)
                                {
                                    distance -= 100;
                                    time -= baselineTime;
                                }

                                return (x: time, y: distance);
                            })
                            .Where(p => p.y > 0 && p.x > 0)
                            .ToList();
                        break;

                    default:
                        continue;
                }

                regressionData.Add(new
                {
                    id = record.ID,
                    date = record.TrainingDate?.ToString("yyyy-MM-dd"),
                    dataPoints = dataPoints.Select(p => new { x = p.x, y = p.y }).ToList()
                });
            }

            return Json(regressionData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 查詢檢測系統訓練紀錄詳情
        public ActionResult DetailsByTrainingRecord(int id, string sportItem)
        {
            object detailData = null;

            switch (sportItem)
            {
                case "跑步機":
                    detailData = _db.TreadmillRecordDetails
                                    .Where(d => d.DetectionTrainingRecordId == id)
                                    .ToList();
                    break;

                case "田徑場":
                    detailData = _db.TrackFieldRecordDetails
                                    .Where(d => d.DetectionTrainingRecordId == id)
                                    .ToList();
                    break;

                case "游泳":
                    detailData = _db.SwimmingRecordDetails
                                    .Where(d => d.DetectionTrainingRecordId == id)
                                    .ToList();
                    break;

                case "自由車":
                    detailData = _db.BikeRecordDetails
                                    .Where(d => d.DetectionTrainingRecordId == id)
                                    .ToList();
                    break;

                case "滑輪溜冰":
                    detailData = _db.RollerSkatingRecordDetails
                                    .Where(d => d.DetectionTrainingRecordId == id)
                                    .ToList();
                    break;

                default:
                    return Content("尚未支援此運動項目的詳細資料查詢");
            }

            if (detailData == null || !(detailData as IEnumerable<object>).Any())
            {
                return Content("查無對應資料");
            }

            ViewBag.SportItem = sportItem;
            ViewBag.TrainingRecord = _db.DetectionTrainingRecord.FirstOrDefault(r => r.ID == id);

            return View("DetailsDynamic", detailData);
        }
        #endregion

        #region 修改訓練紀錄RPE呈現
        public PartialViewResult LoadRPESurvey()
        {
            var rpeList = _db.RPE.Select(r => new RPEModel
            {
                Score = r.Score,
                Description = r.Description,
                Explanation = r.Explanation
            }).ToList();

            ViewBag.RPEScore = rpeList;
            return PartialView("_RPESurvey");
        }
        #endregion

        #region 修改訓練紀錄
        [HttpGet]
        public ActionResult Edit(int id, string item)
        {
            var userRole = Session["UserRole"]?.ToString();
            if (string.IsNullOrEmpty(userRole))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "未登入");

            if (item == "一般訓練衝量監控 (session-RPE)")
            {
                GeneralTrainingRecordViewModel viewModel;

                if (userRole == "Athlete")
                {
                    var record = _db.AthleteGeneralTrainingRecord.FirstOrDefault(r => r.ID == id);
                    if (record == null)
                        return HttpNotFound("找不到運動員訓練紀錄");

                    viewModel = new GeneralTrainingRecordViewModel
                    {
                        ID = record.ID,
                        TrainingName = record.TrainingClassName,
                        TrainingDate = record.TrainingDate ?? DateTime.Now,
                        Coach = record.Coach,
                        Athlete = record.Athlete,
                        AthleteID = record.AthleteID,
                        TrainingItem = record.TrainingItem,
                        ActionName = record.ActionName,
                        TrainingOther = record.TrainingOther,
                        TrainingTime = record.TrainingTime,
                        RPEscore = record.RPEscore ?? 0,
                        EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        Source = "Athlete"
                    };
                }
                else if (userRole == "Coach")
                {
                    var record = _db.GeneralTrainingRecord.FirstOrDefault(r => r.ID == id);
                    if (record == null)
                        return HttpNotFound("找不到教練訓練紀錄");

                    viewModel = new GeneralTrainingRecordViewModel
                    {
                        ID = record.ID,
                        TrainingName = record.TrainingClassName,
                        TrainingDate = record.TrainingDate ?? DateTime.Now,
                        Coach = record.Coach,
                        Athlete = record.Athlete,
                        AthleteID = record.AthleteID,
                        TrainingItem = record.TrainingItem,
                        ActionName = record.ActionName,
                        TrainingOther = record.TrainingOther,
                        TrainingTime = record.TrainingTime,
                        RPEscore = record.RPEscore ?? 0,
                        EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        Source = "Coach"
                    };
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "無權限");
                }

                ViewBag.Description = _db.TrainingItems.Select(t => t.TrainingName).ToList();
                ViewBag.SpecialTechnical = _db.SpecialTechnical.Select(s => s.TechnicalItem).ToList();
                ViewBag.ActionNames = _db.SpecialTechnicalAction.Select(a => a.TechnicalName).ToList();
                ViewBag.MuscleStrength = _db.MuscleStrength.Select(m => m.TrainingPart).ToList();
                ViewBag.PhysicalFitness = _db.PhysicalFitness.Select(p => p.FitnessItem).ToList();
                ViewBag.TrainingTimes = _db.TrainingTimes.Select(t => t.TrainingTime.ToString()).ToList();

                return PartialView("Edit", viewModel);
            }
            if (item == "射箭訓練衝量")
            {
                ArcheryTrainingRecordViewModel viewModel;

                if (userRole == "Athlete")
                {
                    var record = _db.AthleteArcheryTrainingRecord.FirstOrDefault(r => r.ID == id);
                    if (record == null)
                        return HttpNotFound("找不到運動員射箭訓練紀錄");

                    viewModel = new ArcheryTrainingRecordViewModel
                    {
                        ID = record.ID,
                        TrainingDate = record.TrainingDate ?? DateTime.Now,
                        Coach = record.Coach,
                        Athlete = record.Athlete,
                        AthleteID = record.AthleteID,
                        Poundage = record.Poundage ?? 0,
                        ArrowCount = record.ArrowCount ?? 0,
                        RPEscore = record.RPEscore ?? 0,
                        EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        Source = "Athlete"
                    };
                }
                else if (userRole == "Coach")
                {
                    var record = _db.ArcheryRecord.FirstOrDefault(r => r.ID == id);
                    if (record == null)
                        return HttpNotFound("找不到教練射箭訓練紀錄");

                    viewModel = new ArcheryTrainingRecordViewModel
                    {
                        ID = record.ID,
                        TrainingDate = record.TrainingDate ?? DateTime.Now,
                        Coach = record.Coach,
                        Athlete = record.Athlete,
                        AthleteID = record.AthleteID,
                        Poundage = record.Poundage ?? 0,
                        ArrowCount = record.ArrowCount ?? 0,
                        RPEscore = record.RPEscore ?? 0,
                        EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        Source = "Coach"
                    };
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "無權限");
                }

                return PartialView("Edit_Archery", viewModel);
            }
            if (item == "射擊訓練衝量")
            {
                ShootingTrainingRecordViewModel viewModel;

                if (userRole == "Athlete")
                {
                    var record = _db.AthleteShootingRecord.FirstOrDefault(r => r.ID == id);
                    if (record == null)
                        return HttpNotFound("找不到運動員射擊訓練紀錄");

                    viewModel = new ShootingTrainingRecordViewModel
                    {
                        ID = record.ID,
                        TrainingDate = record.TrainingDate ?? DateTime.Now,
                        Coach = record.Coach,
                        Athlete = record.Athlete,
                        AthleteID = record.AthleteID,
                        ShootingTool = record.ShootingTool,
                        BulletCount = record.BulletCount ?? 0,
                        RPEscore = record.RPEscore ?? 0,
                        EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        Source = "Athlete"
                    };
                }
                else if (userRole == "Coach")
                {
                    var record = _db.ShootingRecord.FirstOrDefault(r => r.ID == id);
                    if (record == null)
                        return HttpNotFound("找不到教練射擊訓練紀錄");

                    viewModel = new ShootingTrainingRecordViewModel
                    {
                        ID = record.ID,
                        TrainingDate = record.TrainingDate ?? DateTime.Now,
                        Coach = record.Coach,
                        Athlete = record.Athlete,
                        AthleteID = record.AthleteID,
                        ShootingTool = record.ShootingTool,
                        BulletCount = record.BulletCount ?? 0,
                        RPEscore = record.RPEscore ?? 0,
                        EachTrainingLoad = record.EachTrainingLoad ?? 0,
                        Source = "Coach"
                    };
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "無權限");
                }

                ViewBag.ShootingTools = _db.ShottingItems.Select(t => t.GunsItem).ToList();
                return PartialView("Edit_Shooting", viewModel);
            }
            return Content("不支援的訓練項目");
        }

        [HttpGet]
        public JsonResult GetActionNamesByItem(int itemIndex)
        {
            var actions = _db.SpecialTechnicalAction
                .Where(a => a.SpecialTechnicalID == itemIndex)
                .Select(a => a.TechnicalName)
                .Distinct()
                .ToList();

            return Json(actions, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(GeneralTrainingRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "資料驗證失敗" });
            }

            try
            {
                DateTime targetTime = model.TrainingDate;

                bool isDuplicate = model.Source == "Athlete"
                    ? _db.AthleteGeneralTrainingRecord.Any(r =>
                        r.AthleteID == model.AthleteID &&
                        r.ID != model.ID &&
                        DbFunctions.DiffMinutes(r.TrainingDate, targetTime) == 0)
                    : _db.GeneralTrainingRecord.Any(r =>
                        r.AthleteID == model.AthleteID &&
                        r.ID != model.ID &&
                        DbFunctions.DiffMinutes(r.TrainingDate, targetTime) == 0);

                if (isDuplicate)
                {
                    return Json(new { success = false, message = "與其他紀錄時間重複，請更改時間後再儲存。" });
                }

                if (model.Source == "Athlete")
                {
                    var record = _db.AthleteGeneralTrainingRecord.FirstOrDefault(r => r.ID == model.ID);
                    if (record == null)
                        return Json(new { success = false, message = "找不到該筆資料" });

                    // 更新欄位
                    record.TrainingClassName = model.TrainingName;
                    record.TrainingDate = model.TrainingDate;
                    record.TrainingItem = model.TrainingItem;
                    record.ActionName = model.ActionName;
                    record.TrainingOther = model.TrainingOther;
                    record.TrainingParts = model.TrainingParts;
                    record.TrainingType = model.TrainingType;
                    record.TrainingTime = model.TrainingTime;
                    record.RPEscore = model.RPEscore;
                    record.EachTrainingLoad = model.EachTrainingLoad;
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
                }
                else if (model.Source == "Coach")
                {
                    var record = _db.GeneralTrainingRecord.FirstOrDefault(r => r.ID == model.ID);
                    if (record == null)
                        return Json(new { success = false, message = "找不到該筆資料" });

                    // 更新欄位
                    record.TrainingClassName = model.TrainingName;
                    record.TrainingDate = model.TrainingDate;
                    record.TrainingItem = model.TrainingItem;
                    record.ActionName = model.ActionName;
                    record.TrainingOther = model.TrainingOther;
                    record.TrainingParts = model.TrainingParts;
                    record.TrainingType = model.TrainingType;
                    record.TrainingTime = model.TrainingTime;
                    record.RPEscore = model.RPEscore;
                    record.EachTrainingLoad = model.EachTrainingLoad;
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
                }

                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditArchery(ArcheryTrainingRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "資料驗證失敗" });
            }

            try
            {
                DateTime targetTime = model.TrainingDate;

                bool isDuplicate = model.Source == "Athlete"
                    ? _db.AthleteArcheryTrainingRecord.Any(r =>
                        r.AthleteID == model.AthleteID &&
                        r.ID != model.ID &&
                        DbFunctions.DiffMinutes(r.TrainingDate, targetTime) == 0)
                    : _db.ArcheryRecord.Any(r =>
                        r.AthleteID == model.AthleteID &&
                        r.ID != model.ID &&
                        DbFunctions.DiffMinutes(r.TrainingDate, targetTime) == 0);

                if (isDuplicate)
                {
                    return Json(new { success = false, message = "與其他紀錄時間重複，請更改時間後再儲存。" });
                }

                if (model.Source == "Athlete")
                {
                    var record = _db.AthleteArcheryTrainingRecord.FirstOrDefault(r => r.ID == model.ID);
                    if (record == null)
                        return Json(new { success = false, message = "找不到該筆運動員資料" });

                    record.TrainingDate = model.TrainingDate;
                    record.Poundage = model.Poundage;
                    record.ArrowCount = model.ArrowCount;
                    record.RPEscore = model.RPEscore;
                    record.EachTrainingLoad = model.EachTrainingLoad;
                }
                else if (model.Source == "Coach")
                {
                    var record = _db.ArcheryRecord.FirstOrDefault(r => r.ID == model.ID);
                    if (record == null)
                        return Json(new { success = false, message = "找不到該筆教練資料" });

                    record.TrainingDate = model.TrainingDate;
                    record.Poundage = model.Poundage;
                    record.ArrowCount = model.ArrowCount;
                    record.RPEscore = model.RPEscore;
                    record.EachTrainingLoad = model.EachTrainingLoad;
                }
                else
                {
                    return Json(new { success = false, message = "來源不明，無法儲存" });
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EditShooting(ShootingTrainingRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "資料驗證失敗" });
            }

            try
            {
                DateTime targetTime = model.TrainingDate;

                bool isDuplicate = model.Source == "Athlete"
                    ? _db.AthleteShootingRecord.Any(r =>
                        r.AthleteID == model.AthleteID &&
                        r.ID != model.ID &&
                        DbFunctions.DiffMinutes(r.TrainingDate, targetTime) == 0)
                    : _db.ShootingRecord.Any(r =>
                        r.AthleteID == model.AthleteID &&
                        r.ID != model.ID &&
                        DbFunctions.DiffMinutes(r.TrainingDate, targetTime) == 0);

                if (isDuplicate)
                {
                    return Json(new { success = false, message = "與其他紀錄時間重複，請更改時間後再儲存。" });
                }

                if (model.Source == "Athlete")
                {
                    var record = _db.AthleteShootingRecord.FirstOrDefault(r => r.ID == model.ID);
                    if (record == null)
                        return Json(new { success = false, message = "找不到該筆運動員資料" });

                    record.TrainingDate = model.TrainingDate;
                    record.ShootingTool = model.ShootingTool;
                    record.BulletCount = model.BulletCount;
                    record.RPEscore = model.RPEscore;
                    record.EachTrainingLoad = model.EachTrainingLoad;
                }
                else if (model.Source == "Coach")
                {
                    var record = _db.ShootingRecord.FirstOrDefault(r => r.ID == model.ID);
                    if (record == null)
                        return Json(new { success = false, message = "找不到該筆教練資料" });

                    record.TrainingDate = model.TrainingDate;
                    record.ShootingTool = model.ShootingTool;
                    record.BulletCount = model.BulletCount;
                    record.RPEscore = model.RPEscore;
                    record.EachTrainingLoad = model.EachTrainingLoad;
                }
                else
                {
                    return Json(new { success = false, message = "來源不明，無法儲存" });
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

        #region 刪除紀錄
        [HttpPost]
        public JsonResult Delete(int id, string source, string item)
        {
            try
            {
                if (item == "一般訓練衝量監控 (session-RPE)")
                {
                    if (source == "Athlete")
                    {
                        var record = _db.AthleteGeneralTrainingRecord.FirstOrDefault(r => r.ID == id);
                        if (record != null)
                        {
                            _db.AthleteGeneralTrainingRecord.Remove(record);
                        }
                    }
                    else if (source == "Coach")
                    {
                        var record = _db.GeneralTrainingRecord.FirstOrDefault(r => r.ID == id);
                        if (record != null)
                        {
                            _db.GeneralTrainingRecord.Remove(record);
                        }
                    }
                }
                else if (item == "射箭訓練衝量")
                {
                    if (source == "Athlete")
                    {
                        var record = _db.AthleteArcheryTrainingRecord.FirstOrDefault(r => r.ID == id);
                        if (record != null)
                        {
                            _db.AthleteArcheryTrainingRecord.Remove(record);
                        }
                    }
                    else if (source == "Coach")
                    {
                        var record = _db.ArcheryRecord.FirstOrDefault(r => r.ID == id);
                        if (record != null)
                        {
                            _db.ArcheryRecord.Remove(record);
                        }
                    }
                }
                else if (item == "射擊訓練衝量")
                {
                    if (source == "Athlete")
                    {
                        var record = _db.AthleteShootingRecord.FirstOrDefault(r => r.ID == id);
                        if (record != null)
                        {
                            _db.AthleteShootingRecord.Remove(record);
                        }
                    }
                    else if (source == "Coach")
                    {
                        var record = _db.ShootingRecord.FirstOrDefault(r => r.ID == id);
                        if (record != null)
                        {
                            _db.ShootingRecord.Remove(record);
                        }
                    }
                }
                else if (item == "檢測系統")
                {
                    var record = _db.DetectionTrainingRecord.FirstOrDefault(r => r.ID == id);
                    if (record != null)
                    {
                        var sportItem = record.SportItem;

                        if (sportItem == "跑步機")
                        {
                            var detail = _db.TreadmillRecordDetails.Where(d => d.DetectionTrainingRecordId == id).ToList();
                            _db.TreadmillRecordDetails.RemoveRange(detail);
                        }
                        else if (sportItem == "田徑場")
                        {
                            var detail = _db.TrackFieldRecordDetails.Where(d => d.DetectionTrainingRecordId == id).ToList();
                            _db.TrackFieldRecordDetails.RemoveRange(detail);
                        }
                        else if (sportItem == "游泳")
                        {
                            var detail = _db.SwimmingRecordDetails.Where(d => d.DetectionTrainingRecordId == id).ToList();
                            _db.SwimmingRecordDetails.RemoveRange(detail);
                        }
                        else if (sportItem == "自由車")
                        {
                            var detail = _db.BikeRecordDetails.Where(d => d.DetectionTrainingRecordId == id).ToList();
                            _db.BikeRecordDetails.RemoveRange(detail);
                        }
                        else if (sportItem == "滑輪溜冰")
                        {
                            var detail = _db.RollerSkatingRecordDetails.Where(d => d.DetectionTrainingRecordId == id).ToList();
                            _db.RollerSkatingRecordDetails.RemoveRange(detail);
                        }

                        _db.DetectionTrainingRecord.Remove(record);
                    }
                }
                else
                {
                    return Json(new { success = false, message = "無效的來源" });
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

        #region 讀取session訓練量結果
        public ActionResult LoadSessionRPETrainingRecords(string item, bool isAthlete, int? AthleteID)
        {
            try
            {
                var model = new TrainingRecordViewModel { TrainingItem = item };

                if (isAthlete)
                {
                    model.GeneralTrainingRecord = _db.AthleteGeneralTrainingRecord
                        .Where(record => record.AthleteID == AthleteID)
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
                        .Where(record => record.AthleteID == AthleteID)
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
                        .Where(record => record.AthleteID == AthleteID)
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
                    model.GeneralTrainingRecord = _db.GeneralTrainingRecord.Where(record => record.AthleteID == AthleteID)
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

                    model.ArcheryRecords = _db.ArcheryRecord.Where(record => record.AthleteID == AthleteID)
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

                    model.ShootingRecords = _db.ShootingRecord.Where(record => record.AthleteID == AthleteID)
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
                Console.WriteLine($"發生錯誤: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region 計算session RPE指標結果
        [HttpGet]
        public JsonResult CalculateTrainingLoad(string date, string trainingType, int? AthleteID, bool isAthlete)
        {
            try
            {
                if (!DateTime.TryParse(date, out DateTime selectedDate))  //驗證傳入的日期格式
                {
                    return Json(new { error = "無效的日期格式" }, JsonRequestBehavior.AllowGet);
                }

                // 計算日期範圍
                DateTime endOfWeek = selectedDate.Date;
                DateTime startOfWeek = endOfWeek.AddDays(-6);

                double totalTrainingLoad = 0;
                double dailyTrainingLoadSum = 0;
                double weeklyTrainingLoadSum = 0;
                double trainingMonotony = 0;
                double trainingStrain = 0;
                double weekToWeekChange = 0;
                double acwr = 0;

                // 查詢每日訓練數據
                var dailyRecords = isAthlete
                    ? _db.AthleteGeneralTrainingRecord
                        .Where(record => record.AthleteID == AthleteID &&
                                         DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                        .Select(record => record.EachTrainingLoad)
                        .ToList()
                    : _db.GeneralTrainingRecord
                        .Where(record => record.AthleteID == AthleteID &&
                                         DbFunctions.TruncateTime(record.TrainingDate) == selectedDate)
                        .Select(record => record.EachTrainingLoad)
                        .ToList();

                // 計算每日總訓練量
                //totalTrainingLoad = dailyRecords.Sum();

                // 調用 Helper 方法計算數據
                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSumForAllTypes(_db, date, isAthlete, AthleteID);
                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSumForAllTypes(_db, startOfWeek, endOfWeek, isAthlete, AthleteID);
                trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotonyForAllTypes(_db, startOfWeek, endOfWeek, isAthlete, AthleteID);
                trainingStrain = TrainingRecordHelper.CalculateTrainingStrainForAllTypes(_db, startOfWeek, endOfWeek, isAthlete, AthleteID);

                // 計算前一週的範圍
                DateTime startOfPreviousWeek = startOfWeek.AddDays(-7);
                DateTime endOfPreviousWeek = startOfWeek.AddDays(-1);

                weekToWeekChange = TrainingRecordHelper.CalculateWeekToWeekChangeForAllTypes(
                    _db, startOfWeek, endOfWeek, startOfPreviousWeek, endOfPreviousWeek, isAthlete, AthleteID);

                // 計算過往四週的ACWR
                acwr = TrainingRecordHelper.CalculateACWRForAllTypes(_db, selectedDate, isAthlete, AthleteID);

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

        #region 教練儲存sessionRPE訓練量結果
        [HttpPost]
        public JsonResult SaveTrainingRecord(SessionRPETrainingRecordsModel model, int shootingRecordId)
        {
            try
            {
                int trainingTimeInMinutes = ConvertTrainingTimeToMinutes(model.TrainingTime);

                int dailyTrainingLoad = trainingTimeInMinutes * model.RPEscore;

                var newRecord = new SessionRPETrainingRecords
                {
                    TrainingDate = model.TrainingDate,
                    TrainingItem = model.TrainingItem,
                    DifficultyCategory = model.DifficultyCategory,
                    TrainingActionName = model.TrainingActionName,
                    TrainingTime = trainingTimeInMinutes.ToString(),
                    RPEscore = model.RPEscore,
                    TrainingLoad = model.TrainingLoad,
                    DailyTrainingLoad = dailyTrainingLoad,
                    WeeklyTrainingLoad = model.WeeklyTrainingLoad,
                    TrainingHomogeneity = model.TrainingHomogeneity,
                    TrainingTension = model.TrainingTension,
                    WeeklyTrainingChange = model.WeeklyTrainingChange,
                    ShortToLongTermTrainingLoadRatio = model.ShortToLongTermTrainingLoadRatio,
                    CreatedDate = DateTime.Now
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
        private int ConvertTrainingTimeToMinutes(string trainingTime)
        {
            if (string.IsNullOrEmpty(trainingTime))
                return 0;

            if (trainingTime.EndsWith("小時"))
            {
                if (int.TryParse(trainingTime.Replace("小時", "").Trim(), out int hours))
                {
                    return hours * 60;
                }
            }

            if (trainingTime.EndsWith("分鐘"))
            {
                if (int.TryParse(trainingTime.Replace("分鐘", "").Trim(), out int minutes))
                {
                    return minutes;
                }
            }

            return 0;
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

        #region 檢查一般訓練紀錄是否有相同資料
        [HttpPost]
        public JsonResult CheckDuplicateTrainingRecord(int athleteId, DateTime trainingDate)
        {
            var duplicate = _db.AthleteGeneralTrainingRecord.FirstOrDefault(r =>
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
        public JsonResult CheckDuplicateArcheryRecord(int athleteId, DateTime trainingDate)
        {
            var duplicate = _db.AthleteArcheryTrainingRecord.FirstOrDefault(r =>
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
        public JsonResult CheckDuplicateShootingRecord(int athleteId, DateTime trainingDate)
        {
            var duplicate = _db.AthleteShootingRecord.FirstOrDefault(r =>
                r.AthleteID == athleteId &&
                r.TrainingDate.HasValue &&
                DbFunctions.TruncateTime(r.TrainingDate) == trainingDate.Date &&
                r.TrainingDate.Value.Hour == trainingDate.Hour &&
                r.TrainingDate.Value.Minute == trainingDate.Minute);

            return Json(new { exists = duplicate != null });
        }
        #endregion

        #region 儲存運動員一般訓練紀錄
        public ActionResult SaveAthleteTrainingRecord(List<AthleteGeneralTrainingRecord> records)
        {
            try
            {
                if (Session["UserRole"]?.ToString() != "Athlete")
                {
                    return Json(new { success = false, message = "請確認是否為運動員身份。" });
                }

                foreach (var record in records)
                {
                    if (string.IsNullOrEmpty(record.TrainingClassName))
                    {
                        return Json(new { success = false, message = "訓練項目資料不完整，無法儲存。" });
                    }

                    // 根據 AthleteID 與 TrainingDate 判斷是否有重複紀錄
                    var date = record.TrainingDate.Value;

                    var duplicate = _db.AthleteGeneralTrainingRecord.FirstOrDefault(r =>
                        r.AthleteID == record.AthleteID &&
                        r.TrainingDate.HasValue &&
                        DbFunctions.TruncateTime(r.TrainingDate) == DbFunctions.TruncateTime(date) &&
                        r.TrainingDate.Value.Hour == date.Hour &&
                        r.TrainingDate.Value.Minute == date.Minute);

                    if (duplicate != null)
                    {
                        _db.AthleteGeneralTrainingRecord.Remove(duplicate);
                    }

                    // 依據訓練類別清空不適用欄位
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
        public ActionResult SaveAthleteArcheryRecord(List<AthleteArcheryTrainingRecord> records)
        {
            try
            {
                if (Session["UserRole"]?.ToString() != "Athlete")
                {
                    return Json(new { success = false, message = "請確認是否為運動員身份。" });
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
                        var duplicateTraining = _db.AthleteArcheryTrainingRecord.FirstOrDefault(r =>
                            r.AthleteID == athleteID &&
                            r.TrainingDate.HasValue &&
                            DbFunctions.TruncateTime(r.TrainingDate) == DbFunctions.TruncateTime(date) &&
                            r.TrainingDate.Value.Hour == date.Hour &&
                            r.TrainingDate.Value.Minute == date.Minute);

                        if (duplicateTraining != null)
                        {
                            _db.AthleteArcheryTrainingRecord.Remove(duplicateTraining);
                            _db.SaveChanges();
                        }
                    }

                    // 儲存新的 Training 資料
                    _db.AthleteArcheryTrainingRecord.Add(correspondingRecord);
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

        #region 儲存運動員射擊訓練紀錄
        public ActionResult SaveAthleteShootingRecord(List<AthleteShootingRecord> records)
        {
            try
            {
                if (Session["UserRole"]?.ToString() != "Athlete")
                {
                    return Json(new { success = false, message = "請確認是否為運動員身份。" });
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
                        var duplicateTraining = _db.AthleteShootingRecord.FirstOrDefault(r =>
                            r.AthleteID == athleteID &&
                            r.TrainingDate.HasValue &&
                            DbFunctions.TruncateTime(r.TrainingDate) == DbFunctions.TruncateTime(date) &&
                            r.TrainingDate.Value.Hour == date.Hour &&
                            r.TrainingDate.Value.Minute == date.Minute);

                        if (duplicateTraining != null)
                        {
                            _db.AthleteShootingRecord.Remove(duplicateTraining);
                            _db.SaveChanges();
                        }
                    }

                    // 儲存新的 Training 資料
                    _db.AthleteShootingRecord.Add(correspondingRecord);
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

        #region 儲存檢測系統訓練紀錄
        [HttpPost]
        public ActionResult SaveTrackFieldRecord(SaveTrackFieldRecordModel model)
        {
            try
            {
                int? coachId = AuthHelper.GetCurrentCoachId();
                int? athleteId = model.athleteID;

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
                    CriticalSpeed = Math.Round(model.CriticalSpeed, 1),
                    MaxAnaerobicWork = Math.Round(model.AnaerobicPower, 2),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    RollerSkill = model.RollerSkill,
                };

                if (model.SportItem == "滑輪溜冰")
                {
                    detectionRecord.RollerSkill = Math.Round(model.RollerSkill, 2);
                }

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
                            ForceDuration = Math.Round(float.Parse(model.ForceDurations[i]), 3),
                            Speed = Math.Round(float.Parse(model.Speeds[i]), 1),
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
                            ForceDuration = Math.Round(float.Parse(model.ForceDurations[i]), 3),
                            Speed = Math.Round(float.Parse(model.Speeds[i]), 1),
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
                            ForceDuration = Math.Round(float.Parse(model.ForceDurations[i]), 3),
                            Speed = Math.Round(float.Parse(model.Speeds[i]),1),
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
                            ForceDuration = Math.Round(float.Parse(model.ForceDurations[i]), 3),
                            Speed = Math.Round(float.Parse(model.Speeds[i]), 1),
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
                            ForceDuration = Math.Round(float.Parse(model.ForceDurations[i]),3),
                            Speed = Math.Round(float.Parse(model.Speeds[i]),1),
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

        #region 處理月份篩選
        public ActionResult SessionRecordByMonth(int? AthleteID, int year, int month)
        {
            try
            {
                var dtos = _db.PsychologicalTraitsResults.Where(dt => dt.UserID == AthleteID && dt.PsychologicalDate.Year == year && dt.PsychologicalDate.Month == month).OrderBy(dt => dt.PsychologicalDate).ToList();

                var dates = dtos.Select(dt => dt.PsychologicalDate.ToString("yyyy-MM-dd")).Distinct().ToList();
                var sleepQualityScores = new List<int>();
                var fatigueScores = new List<int>();
                var trainingWillingnessScores = new List<int>();
                var appetiteScores = new List<int>();
                var competitionWillingnessScores = new List<int>();

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