using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PhysicalFit.Controllers
{
    public class PhyFitController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        #region 註冊帳號
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(string UserName, string pwd, string Email)
        {
            try
            {
                if (pwd.Length < 6)
                {
                    ViewBag.ErrorMessage = "密碼長度至少要6位數";
                    return View();
                }

                if (_db.Users.Any(u => u.Name == UserName))
                {
                    ViewBag.ErrorMessage = "該帳號已存在";
                    return View();
                }

                var Pwd = Sha256Hash(pwd);
                var newUser = new Users
                {
                    Name = UserName,
                    Password = Pwd,
                    Email = Email,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                };

                _db.Users.Add(newUser);
                _db.SaveChanges();

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return View("Error");
            }
        }
        #endregion

        #region 密碼加密
        private static string Sha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        #endregion

        #region 登入
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string pwd)
        {
            // 將使用者輸入的密碼進行SHA256加密
            string hashedPwd = Sha256Hash(pwd);

            var dto = _db.Users.FirstOrDefault(u => u.Name == UserName && u.Password == hashedPwd);

            if (dto != null)
            {
                _db.SaveChanges();

                // 設定 Session 狀態為已登入
                Session["LoggedIn"] = true;
                Session["UserName"] = dto.Name;

                // 檢查是否有記錄的返回頁面
                string returnUrl = Session["ReturnUrl"] != null ? Session["ReturnUrl"].ToString() : Url.Action("Home", "PhyFit");

                // 清除返回頁面的 Session 記錄
                Session.Remove("ReturnUrl");

                // 重定向到記錄的返回頁面
                return Redirect(returnUrl);
            }
            else
            {
                // 驗證失敗
                ViewBag.ErrorMessage = "帳號或密碼錯誤";
                return View();
            }
        }
        #endregion

        #region 登出
        public ActionResult Logout()
        {
            // 清除所有的 Session 資訊
            Session.Clear();
            Session.Abandon();

            // 清除所有的 Forms 認證 Cookies
            FormsAuthentication.SignOut();

            // 取得登出前的頁面路徑，如果沒有則預設為首頁
            string returnUrl = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Home", "PhyFit");

            // 重定向到記錄的返回頁面
            return Redirect(returnUrl);
            // 重定向到 Home 頁面
        }
        #endregion

        #region 修改密碼

        #endregion

        #region 首頁-測試
        public ActionResult Home()
        {
            Session["ReturnUrl"] = Request.Url.ToString();

            ViewBag.TrainingPurposes = GetTrainingPurposes(); //訓練用途
            ViewBag.AbilityDetermination = GetAbilityDetermination(); //能力測定

            var dto = _db.RPE.Select(r => new RPEModel
            {
                Score = r.Score,
                Description = r.Description,
                Explanation = r.Explanation,

            }).ToList();

            return View(dto);
        }
        #endregion

        #region 訓練監控主視圖
        public ActionResult dashboard()
        {
            //Session["ReturnUrl"] = Request.Url.ToString();

            ViewBag.MonitoringItems = GetTrainingMonitoringItems(); //訓練監控項目選擇
            ViewBag.Description = GetTrainingItem(); //訓練衝量監控(session-RPE)
            ViewBag.TrainingPurposes = GetIntensityClassification(); //訓練強度
            ViewBag.TrainingTimes = GetTrainingTimes();//訓練時間
            ViewBag.RPEScore = GetRPE();//RPE量表
            ViewBag.GunItem = GetGunsItems(); //射擊用具項目
            ViewBag.DetectionSport = GetSpoetsItem(); //檢測系統_運動項目
            //ViewBag.SpoetsDistance = GetSpoetsDistance(); //檢測系統_距離

            var records = _db.SessionRPETrainingRecords.ToList();

            var model = records.Select(r => new SessionRPETrainingRecordsModel
            {
                TrainingItem = r.TrainingItem, //訓練名稱
                RPEscore = r.RPEscore, //RPE分數
                //TrainingTime = r.TrainingTime, //訓練時間
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
        #endregion

        #region 射擊用具項目
        public List<string> GetGunsItems()
        {
            var dto = (from gu in _db.ShottingItems
                       select gu.GunsItem).ToList();
            return dto;
        }
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
        //[HttpGet]
        //public JsonResult GetSpoetsDistance(string itemName)
        //{
        //    try
        //    {
        //        var distances = (from Si in _db.DetectionTraining
        //                         where Si.ItemName == itemName
        //                         select Si.Distance).FirstOrDefault();

        //        if (!string.IsNullOrEmpty(distances))
        //        {
        //            // 分割距離字串為列表
        //            var distanceList = distances.Split('/').ToList();
        //            return Json(distanceList, JsonRequestBehavior.AllowGet);
        //        }

        //        return Json(new List<string>(), JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
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

        #region 儲存sessionRPE訓練量結果
        [HttpPost]
        public JsonResult SaveTrainingRecord(SessionRPETrainingRecordsModel model)
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

        #region 讀取sessionRPE訓練量結果
        [HttpGet]
        public ActionResult LoadSessionRPETrainingRecords()
        {
            // 從數據庫中讀取最新的數據
            var records = _db.SessionRPETrainingRecords
                             .Select(record => new SessionRPETrainingRecordsModel
                             {
                                 TrainingDate = record.TrainingDate.HasValue ? record.TrainingDate.Value : DateTime.MinValue, //日期
                                 TrainingItem = record.TrainingItem, //訓練名稱
                                 DifficultyCategory = record.DifficultyCategory, //難度分類
                                 TrainingActionName = record.TrainingActionName, //動作名稱
                                 TrainingTime = record.TrainingTime, //訓練時間
                                 RPEscore = record.RPEscore, //RPE分數
                                 TrainingLoad = record.TrainingLoad ?? 0, //運動訓練量
                                 DailyTrainingLoad = record.DailyTrainingLoad ?? 0, //每日運動訓練量
                                 WeeklyTrainingChange = record.WeeklyTrainingChange ?? 0, //每週運動訓練量
                                 TrainingHomogeneity = record.TrainingHomogeneity ?? 0, //同質性
                                 TrainingTension = record.TrainingTension ?? 0, //張力值
                                 WeeklyTrainingLoad = record.WeeklyTrainingLoad ?? 0, //週間訓練變化
                                 ShortToLongTermTrainingLoadRatio = record.ShortToLongTermTrainingLoadRatio ?? 0, //短長期
                             }).ToList();
            // 返回部分視圖
            return PartialView("_WeeklyTrainingRecords", records);
        }
        #endregion

        #region 計算檢測結果
        [HttpGet]
        public JsonResult CalculateTrainingLoad(DateTime date)
        {
            try
            {
                // 從資料庫讀取指定日期的訓練記錄
                var records = _db.SessionRPETrainingRecords
                    .Where(record => record.TrainingDate == date)
                    .ToList();

                // 計算總訓練量
                var totalTrainingLoad = 0;
                var dailyTrainingLoadSum = 0;
                var weeklyTrainingLoadSum = 0;

                //計算運動訓練量
                foreach (var record in records)
                {
                    var trainingLoad = TrainingRecordHelper.CalculateTrainingLoad(record.TrainingTime, record.RPEscore);
                    totalTrainingLoad = trainingLoad;
                }

                //計算每日運動訓練量
                dailyTrainingLoadSum = TrainingRecordHelper.CalculateDailyTrainingLoadSum(_db, date);

                //計算每週運動訓練量
                weeklyTrainingLoadSum = TrainingRecordHelper.CalculateWeeklyTrainingLoadSum(_db, date);

                //計算訓練同質性(TM)
                double trainingMonotony = TrainingRecordHelper.CalculateTrainingMonotony(_db, date);

                //計算訓練張力值(TS)
                double trainingStrain = TrainingRecordHelper.CalculateTrainingStrain(_db, date);

                //計算週間訓練變化
                double weekToweekChange = TrainingRecordHelper.CalculateWeekToWeekChange(_db, date);

                //計算短長期訓練量比值
                double acwr = TrainingRecordHelper.CalculateACWR(_db, date);

                return Json(new
                {
                    TrainingLoad = totalTrainingLoad, //訓練量
                    DailyTrainingLoadSum = dailyTrainingLoadSum, //每日訓練量
                    WeeklyTrainingLoadSum = weeklyTrainingLoadSum, //週訓練量
                    TrainingMonotony = trainingMonotony, //TM同質性
                    TrainingStrain = trainingStrain, //TS張力值
                    WeekToWeekChange = weekToweekChange, //週間訓練變化
                    ACWR = acwr, //短長期訓練量比值
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}