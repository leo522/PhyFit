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
            ViewBag.MonitoringItems = GetTrainingMonitoringItems(); //訓練監控項目選擇
            ViewBag.Description = GetTrainingItem(); //訓練衝量監控(session-RPE)
            ViewBag.TrainingPurposes = GetIntensityClassification(); //訓練強度
            ViewBag.TrainingTimes = GetTrainingTimes();//訓練時間
            ViewBag.RPEScore = GetRPE();//RPE量表
            ViewBag.GunItem = GetGunsItems(); //射擊用具項目
            ViewBag.DetectionItem = GetDetectionItem(); //檢測系統_有無氧項目
            ViewBag.DetectionSport = GetSpoetsItem(); //檢測系統_運動項目
            ViewBag.SpoetsDistance = GetSpoetsDistance();
            return View();
        }
        #endregion

        #region 日期
        public ActionResult GetDateData()
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
        public List<string> GetRPE()
        {
            var dto = (from tp in _db.RPE
                       select tp.Score.ToString()).ToList();

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

        #region 檢測系統_有無氧代謝能力項目
        public List<string> GetDetectionItem()
        {
            var dto = (from di in _db.DetectionSys
                       select di.DetectionItem).ToList();
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
        public List<string> GetSpoetsDistance()
        {
            var dto = (from Si in _db.DetectionTraining
                       select Si.Distance).ToList();
            return dto;
        }
        #endregion
    }
}