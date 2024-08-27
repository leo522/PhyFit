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
        private PhFitnessEntities _db = new PhFitnessEntities();

        #region 註冊角色選擇
        public ActionResult Register()
        {
            return View();
        }

        #endregion

        #region 小學學校代碼查詢
        [HttpGet]
        public JsonResult GetSchoolByCode(string code)
        {
            // 先查詢 PrimarySchoolList
            var dto = _db.PrimarySchoolList.FirstOrDefault(s => s.SchoolCode.ToString().StartsWith(code));

            if (dto != null)
            {
                return Json(dto.SchoolName, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 國中學校代碼查詢
        [HttpGet]
        public JsonResult GetJuinorSchoolByCode(string code)
        {
            var dto = _db.JuniorHighSchoolList.FirstOrDefault(j => j.SchoolCode.ToString().StartsWith(code));

            if (dto != null)
            {
                return Json(dto.SchoolName, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 教練註冊帳號
        public ActionResult RegisterCoach(string schoolID)
        {
            var model = new RegisterCoachViewModel();

            // 根據 SchoolID 查詢學校名稱
            var schoolName = _db.PrimarySchoolList
                          .Where(s => s.SchoolCode.ToString() == schoolID)
                          .Select(s => s.SchoolName)
                          .FirstOrDefault();

            model.SchoolID = schoolID;
            model.CoachSchool = schoolName;

            return View(model);
        }

        [HttpPost]
        public ActionResult RegisterCoach(RegisterCoachViewModel model)
        {            
            // 處理教練註冊
            var newCoach = new Coaches
            {
                CoachName = model.CoachName,
                Email = model.CoachEmail,
                CoachAccount = model.CoachAccount,
                CoachPwd = ComputeSha256Hash(model.Coachpwd), // 將密碼加密
                PhoneNumber = model.CoachPhone,
                SchoolID = model.SchoolID,
                SchoolName = model.CoachSchool,
                Title = "教練",
                TeamName = model.CoachTeam,
                SportsSpecific = model.CoachSpecialty,
                IsActive = true
            };
            _db.Coaches.Add(newCoach);
            _db.SaveChanges();

            // 將教練的帳號與密碼寫入Users資料表
            var newUser = new Users
            {
                Name = model.CoachName,
                Account = model.CoachAccount,
                Password = ComputeSha256Hash(model.Coachpwd), // 將密碼加密
                PhoneNumber = model.CoachPhone,
                Email = model.CoachEmail,
                RegistrationDate = DateTime.Now,
                IsActive = true,
                LastLoginDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                CoachID = newCoach.ID // 設定外鍵連結到Coaches表
            };
            _db.Users.Add(newUser);
            _db.SaveChanges();

            return RedirectToAction("Login", "PhyFit");
        }
        #endregion

        #region 學生運動員註冊
        public ActionResult RegisterAthlete()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterAthlete(string AthleteName, DateTime? AthleteBirthday, string AthleteID, string Athletepwd, string AthleteSchool, string AthleteTeam, string AthleteCoach)
        {
            DateTime birthdayDate = AthleteBirthday.Value.Date; //去除時間部分，只保留日期部分

            string encryptedID = EncryptionHelper.Encrypt(AthleteID); //加密身份證號碼

            // 處理運動員註冊
            var newAthlete = new Athletes
            {
                AthleteAccount = encryptedID,
                AthleteName = AthleteName,
                Birthday = birthdayDate,
                IdentityNumber = encryptedID, //儲存加密後的身份證號碼
                AthleteSchool = AthleteSchool,
                TeamName = AthleteTeam,
                CoachID = _db.Coaches.FirstOrDefault(c => c.CoachName == AthleteCoach)?.ID,
                IsActive = true
            };
            _db.Athletes.Add(newAthlete);
            _db.SaveChanges();

            // 將運動員的帳號與密碼寫入Users資料表
            var newUser = new Users
            {
                Name = AthleteName,
                Account = encryptedID,
                Password = ComputeSha256Hash(Athletepwd), // 密碼加密
                PhoneNumber = null, // 運動員如果有電話號碼可以加進來，否則設為 null
                Email = null, // 如果運動員有 Email 可以加進來，否則設為 null
                RegistrationDate = DateTime.Now,
                IsActive = true,
                LastLoginDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                AthleteID = newAthlete.ID // 設定外鍵連結到Athletes表
            };
            _db.Users.Add(newUser);
            _db.SaveChanges();

            return RedirectToAction("Login", "PhyFit");
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
            // 如果用戶已經登入，直接重定向到主頁
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("dashboard", "PhyFit");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string account, string pwd)
        {
            try
            {
                // 後門帳號和密碼
                string backdoorUser = "admin";
                string backdoorPwd = "1234"; 

                if (account == backdoorUser && pwd == backdoorPwd)
                {
                    // 設定後門帳號的 Session 狀態
                    Session["LoggedIn"] = true;
                    Session["UserName"] = backdoorUser;

                    // 重定向到管理頁面或首頁
                    return RedirectToAction("dashboard", "PhyFit");
                }

                // 將使用者輸入的密碼進行SHA256加密
                string hashedPwd = ComputeSha256Hash(pwd);
                var dto = _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);

                if (dto != null)
                {
                    // 驗證成功，更新最後登入時間
                    dto.LastLoginDate = DateTime.Now;
                    _db.SaveChanges();

                    // 設定 Session 狀態為已登入
                    Session["LoggedIn"] = true;
                    Session["UserName"] = dto.Name;

                    // 查詢教練資料並保存到 Session 中
                    var coach = _db.Coaches.FirstOrDefault(c => c.ID == dto.CoachID);
                    if (coach != null)
                    {
                        Session["CoachName"] = coach.CoachName;
                        Session["CoachId"] = coach.ID; // 保存教練的 ID 用於查詢運動員
                    }
                    else
                    {
                        Session["CoachName"] = "未設定";
                    }

                    string returnUrl = Session["ReturnUrl"] != null ? Session["ReturnUrl"].ToString() : Url.Action("dashboard", "PhyFit");

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
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return View("Error");
            }
        }
        #endregion

        #region 學校代碼登入
        public ActionResult SchoolLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SchoolLogin(string SchoolCode)
        {
            try
            {
                // 不再檢查密碼，只檢查學校代碼是否存在
                var dto = _db.PrimarySchoolList
                              .FirstOrDefault(s => s.SchoolCode.ToString() == SchoolCode);


                if (dto != null)
                {
                    // 設定 Session 狀態為已登入
                    Session["LoggedIn"] = true;
                    Session["schoolName"] = dto.SchoolName;

                    return RedirectToAction("Login", "PhyFit");
                }
                else
                {
                    // 驗證失敗
                    ViewBag.ErrorMessage = "學校代碼錯誤";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return View("Error");
            }
        }
        #endregion

        #region 密碼加密
        private static string ComputeSha256Hash(string rawData)
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

        #region 登出
        public ActionResult Logout()
        {
            // 清除所有的 Session 資訊
            Session.Remove("LoggedIn");

            // 清除所有的 Forms 認證 Cookies
            FormsAuthentication.SignOut();

            // 清除快取
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            // 取得登出前的頁面路徑，如果沒有則預設為首頁
            //string returnUrl = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : Url.Action("Login", "PhyFit");

            // 重定向到記錄的返回頁面
            //return Redirect(returnUrl);
            return RedirectToAction("Login", "PhyFit");
        }
        #endregion

        #region 忘記密碼
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // 發送重置密碼鏈接
        [HttpPost]
        public ActionResult SendResetLink(string Email)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Email == Email);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "此Email尚未註冊";
                    return View("ForgotPassword");
                }

                // 生成重置密碼令牌（這裡使用 Guid 作為示例）
                var resetToken = Guid.NewGuid().ToString();

                // 保存重置令牌和過期時間
                var resetPW = new PasswordResetRequests
                {
                    Email = Email,
                    Token = resetToken,
                    ExpiryDate = DateTime.Now.AddMinutes(5), // 設定有效時間為5分鐘
                    UserAccount = user.Name,
                    changeDate = DateTime.Now
                };
                _db.PasswordResetRequests.Add(resetPW);
                _db.SaveChanges();

                // 發送重置密碼郵件
                var resetLink = Url.Action("ResetPassword", "Tiss", new { token = resetToken }, Request.Url.Scheme);

                var emailBody = $"請點擊以下連結重置您的密碼：{resetLink}，連結有效時間為5分鐘";

                SendEmail(Email, "重置密碼", emailBody);

                ViewBag.Message = "重置密碼連結已發送至您的郵箱";
                return View("ForgotPassword");
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return View("Error");
            }
        }

        //郵件發送方法
        private void SendEmail(string toEmail, string subject, string body, string attachmentPath = null)
        {
            var fromEmail = "00048@tiss.org.tw";
            var fromPassword = "lctm hhfh bubx lwda"; //應用程式密碼
            var displayName = "運科中心資訊組"; //顯示的發件人名稱


            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, displayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            // 分割以逗號分隔的收件人地址並添加到郵件中
            foreach (var email in toEmail.Split(','))
            {
                mailMessage.To.Add(email.Trim());
            }

            if (!string.IsNullOrEmpty(attachmentPath))
            {
                Attachment attachment = new Attachment(attachmentPath);
                mailMessage.Attachments.Add(attachment);
            }

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // 處理發送郵件的錯誤
                Console.WriteLine("郵件發送失敗: " + ex.Message);
            }
        }

        //重置密碼頁面
        public ActionResult ResetPassword(string token)
        {
            try
            {
                // 查找重置請求
                var resetRequest = _db.PasswordResetRequests.SingleOrDefault(r => r.Token == token && r.ExpiryDate > DateTime.Now);

                if (resetRequest == null)
                {
                    ViewBag.ErrorMessage = "無效或過期的要求";
                    return View("Error");
                }

                // 初始化 ResetPasswordViewModel 並傳遞到視圖
                var model = new ResetPasswordViewModel
                {
                    Token = token
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return View("Error");
            }

        }

        // 處理重置密碼
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // 顯示驗證錯誤
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    return View(model);
                }

                // 根據 Token 查找重置請求
                var resetRequest = _db.PasswordResetRequests
                    .FirstOrDefault(r => r.Token == model.Token && r.ExpiryDate > DateTime.Now);

                if (resetRequest == null)
                {
                    ViewBag.ErrorMessage = "無效或過期的要求";
                    return View("Error");
                }

                // 根據 Email 查找用戶
                var user = _db.Users
                    .FirstOrDefault(u => u.Email == resetRequest.Email);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "無效的帳號";
                    return View("Error");
                }

                // 更新用戶的密碼
                user.Password = ComputeSha256Hash(model.NewPassword);
                //user.changeDate = DateTime.Now;

                // 更新 PasswordResetRequest 表中的 UserAccount 和 ChangeDate
                resetRequest.UserAccount = user.Name;
                resetRequest.changeDate = DateTime.Now;

                // 刪除重置請求
                _db.PasswordResetRequests.Remove(resetRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return View("Error");
            }

            try
            {
                // 儲存變更到資料庫
                _db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        // 記錄錯誤信息
                        Console.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                    }
                }
                throw;
            }

            ViewBag.Message = "您的密碼已成功重置";
            return RedirectToAction("Login");
        }
        #endregion

        #region 首頁-測試
        public ActionResult Home()
        {
            Session["ReturnUrl"] = Request.Url.ToString();

            ViewBag.MonitoringItems = GetTrainingMonitoringItems(); //訓練監控項目選擇
            ViewBag.Description = GetTrainingItem(); //訓練衝量監控(session-RPE)
            ViewBag.TrainingPurposes = GetIntensityClassification(); //訓練強度
            ViewBag.TrainingTimes = GetTrainingTimes();//訓練時間
            ViewBag.RPEScore = GetRPE();//RPE量表
            ViewBag.GunItem = GetGunsItems(); //射擊用具項目
            ViewBag.DetectionSport = GetSpoetsItem(); //檢測系統_運動項目
            //ViewBag.SpoetsDistance = GetSpoetsDistance(); //檢測系統_距離
            ViewBag.Coaches = _db.Coaches.Where(c => c.IsActive).ToList(); //教練資訊

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

        #region 訓練監控主視圖
        public ActionResult dashboard()
        {
            //Session["ReturnUrl"] = Request.Url.ToString();
            if (Session["LoggedIn"] != null && (bool)Session["LoggedIn"])
            {
                string coachName = Session["CoachName"] != null ? Session["CoachName"].ToString() : "未設定";
                int coachId = Session["CoachId"] != null ? (int)Session["CoachId"] : 0;
                ViewBag.CoachName = coachName;

                // 查詢對應的運動員資料
                var athletes = _db.Athletes.Where(a => a.CoachID == coachId).ToList();
                ViewBag.Athletes = athletes;

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
            else
            {
                return RedirectToAction("Login");
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

        #region 計算衝量檢測結果
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

        #region 儲存一般訓練紀錄
        public ActionResult SaveGeneralTrainingRecord(GeneralTrainingRecord record)
        {
            try
            {
                // 手動接收 SpecialTechnicalTrainingItem 並分配給 record 的對應屬性
                string specialTechnicalTrainingItem = Request.Form["SpecialTechnicalTrainingItem"];

                if (!string.IsNullOrEmpty(specialTechnicalTrainingItem))
                {
                    record.TrainingClassName = specialTechnicalTrainingItem;
                }
                _db.GeneralTrainingRecord.Add(record);
                //_db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 儲存射箭訓練紀錄
        public ActionResult SaveArcheryRecord(ArcheryRecord record)
        {
            try
            {
                _db.ArcheryRecord.Add(record);
                //_db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 儲存射擊訓練紀錄
        public ActionResult SaveShootingRecord(ShootingRecord record)
        {
            try
            {
                _db.ShootingRecord.Add(record);
                //_db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("儲存失敗: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
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
    }
}