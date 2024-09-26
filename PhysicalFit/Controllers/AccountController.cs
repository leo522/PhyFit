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
using System.Text.RegularExpressions;
using System.Data.Entity;

namespace PhysicalFit.Controllers
{
    public class AccountController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities(); //資料庫

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
                return Json(new { SchoolName = dto.SchoolName, CityName = dto.CityName }, JsonRequestBehavior.AllowGet);
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
                return Json(new { SchoolName = dto.SchoolName, CityName = dto.CityName }, JsonRequestBehavior.AllowGet);
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 模糊查詢學校名稱
        [HttpGet]
        public JsonResult GetSchoolByName(string name)
        {
            // 查詢小學
            var primarySchoolResults = _db.PrimarySchoolList
                     .Where(s => s.SchoolName.Contains(name))
                     .Select(s => new { s.SchoolCode, s.SchoolName, s.CityName })
                     .ToList();

            // 查詢國中
            var juniorHighSchoolResults = _db.JuniorHighSchoolList
                                             .Where(j => j.SchoolName.Contains(name))
                                             .Select(j => new { j.SchoolCode, j.SchoolName, j.CityName})
                                             .ToList();

            // 合併查詢結果
            var results = primarySchoolResults.Concat(juniorHighSchoolResults).ToList();

            if (results.Any())
            {
                return Json(results, JsonRequestBehavior.AllowGet);
            }

            return Json(new List<object>(), JsonRequestBehavior.AllowGet);
        }
        #endregion
        
        #region 教練註冊帳號
        public ActionResult RegisterCoach(string schoolID)
        {
            var model = new RegisterCoachViewModel();

            model.SchoolID = schoolID;
            // 根據 SchoolID 查詢學校名稱
            var schoolName = _db.PrimarySchoolList
                          .Where(s => s.SchoolCode.ToString() == schoolID)
                          .Select(s => s.SchoolName)
                          .FirstOrDefault();

           
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
                RegistrationDate = DateTime.Now, // 設定註冊時間
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

            return RedirectToAction("Login", "Account");
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

            //string encryptedID = EncryptionHelper.Encrypt(AthleteID); //AES對稱加密身份證號碼

            // 處理運動員註冊
            var newAthlete = new Athletes
            {
                AthleteAccount = AthleteID,
                AthletePWD = ComputeSha256Hash(Athletepwd),
                AthleteName = AthleteName,
                Birthday = birthdayDate,
                IdentityNumber = AthleteID.ToUpper(),
                AthleteSchool = AthleteSchool,
                TeamName = AthleteTeam,
                CoachID = _db.Coaches.FirstOrDefault(c => c.CoachName == AthleteCoach)?.ID,
                RegistrationDate = DateTime.Now, // 設定註冊時間
                IsActive = true
            };
            _db.Athletes.Add(newAthlete);
            _db.SaveChanges();

            // 將運動員的帳號與密碼寫入Users資料表
            var newUser = new Users
            {
                Name = AthleteName,
                Account = AthleteID,
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

            return RedirectToAction("Login", "Account");
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
                string hashedPwd = ComputeSha256Hash(pwd);
                Users user = null;

                // 驗證身份並查詢用戶
                user = _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);

                if (user != null)
                {
                    user.LastLoginDate = DateTime.Now; //更新用戶的最後登入時間
                    _db.SaveChanges();

                    //設定 Session，根據 CoachID 判斷用戶角色
                    Session["UserRole"] = user.CoachID.HasValue ? "Coach" : "Athlete";

                    // 設定 FormsAuthentication Ticket
                    var authTicket = new FormsAuthenticationTicket(
                        1,
                        user.Name,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30),
                        false,
                        user.CoachID.HasValue ? user.CoachID.Value.ToString() : user.AthleteID.ToString(),
                        FormsAuthentication.FormsCookiePath);

                    //加密並設定 Cookie
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                    {
                        HttpOnly = true,
                        //Secure = Request.IsSecureConnection // 確保在HTTPS下傳輸
                    };
                    Response.Cookies.Add(authCookie);

                    return RedirectToAction("dashboard", "PhyFit");

                }
                else
                {
                    ViewBag.ErrorMessage = "帳號或密碼錯誤";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("其他錯誤: " + ex.Message);
                return RedirectToAction("Error404", "Error");
            }
        }

        private bool IsIdentityNumber(string account)
        {
            return account.Length == 10 && account.All(c => char.IsLetterOrDigit(c));
            // 根據實際情況設置身份證號碼的格式檢查
            // 這裡假設身份證號碼為數字和字母組成，並且長度為特定的數字
            //return Regex.IsMatch(account, "^[A-Za-z][0-9]{9}$"); // 假設身份證是1個字母+9個數字
        }

        public JsonResult GetUserRole()
        {
            var userRole = Session["UserRole"]?.ToString();
            return Json(new { userRole }, JsonRequestBehavior.AllowGet);
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

                    return RedirectToAction("Login", "Account");
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
            return RedirectToAction("Login", "Account");
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
                var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, Request.Url.Scheme);

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

        #region 運動員資料編輯頁
        public ActionResult AthleteEdit()
        {
            // 確保用戶已登入且為運動員
            if (!User.Identity.IsAuthenticated || Session["UserRole"]?.ToString() != "Athlete")
            {
                return RedirectToAction("Login", "Account"); // 如果未登入或不是運動員，重定向到登入頁
            }

            // 使用 User.Identity.Name 查找運動員的帳號
            var athleteAccount = User.Identity.Name;

            // 查詢運動員資料，通過 AthleteAccount 來匹配
            var dto = _db.Athletes.FirstOrDefault(a => a.AthleteName == athleteAccount);

            if (dto == null)
            {
                return RedirectToAction("Error404", "Error"); // 如果查不到對應的運動員資料
            }

            // 獲取可用的教練列表
            var coaches = _db.Coaches.Where(c => c.IsActive).ToList();

            ViewBag.Coaches = new SelectList(coaches, "ID", "CoachName", dto.CoachID);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AthleteEdit(Athletes athlete)
        {
            if (ModelState.IsValid)
            {
                // 查找原始資料
                var existingAthlete = _db.Athletes.Find(athlete.ID);

                if (existingAthlete != null)
                {
                    // 更新需要的欄位，避免更新敏感資料
                    existingAthlete.AthleteName = athlete.AthleteName; //姓名
                    existingAthlete.AthleteSchool = athlete.AthleteSchool; //學校
                    existingAthlete.TeamName = athlete.TeamName; //隊伍名稱
                    existingAthlete.Birthday = athlete.Birthday; //生日
                    //existingAthlete.IdentityNumber = athlete.IdentityNumber; //身分證字號
                    existingAthlete.CoachID = athlete.CoachID; // 更新教練ID 
                    existingAthlete.LastUpdated = DateTime.Now; // 設置為當前時間

                    _db.Entry(existingAthlete).State = EntityState.Modified;
                    _db.SaveChanges();

                    return RedirectToAction("dashboard", "PhyFit");
                }
            }

            // 如果模型狀態無效，重新加載教練資料
            var coaches = _db.Coaches.Where(c => c.IsActive).ToList();
            ViewBag.Coaches = new SelectList(coaches, "ID", "CoachName", athlete.CoachID);

            return View(athlete);
        }
        #endregion

        #region 教練資料編輯頁
        public ActionResult CoachEdit() 
        {
            // 確保用戶已登入且為教練
            if (!User.Identity.IsAuthenticated || Session["UserRole"]?.ToString() != "Coach")
            {
                return RedirectToAction("Login", "Account"); //如果未登入或不是教練，重定向到登入頁
            }

            // 根據登入的教練帳號獲取教練的資料
            var coachName = User.Identity.Name; //假設教練的帳號是 User.Identity.Name
            var coach = _db.Coaches.FirstOrDefault(c => c.CoachName == coachName);

            if (coach == null)
            {
                return RedirectToAction("Error404", "Error"); //如果查不到對應的教練資料
            }

            return View(coach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CoachEdit(Coaches coach)
        {
            if (ModelState.IsValid)
            {
                // 查找原始資料
                var existingCoach = _db.Coaches.Find(coach.ID);
                if (existingCoach != null)
                {
                    // 更新需要的欄位
                    existingCoach.CoachName = coach.CoachName; //教練名字
                    existingCoach.Email = coach.Email; //教練信箱
                    existingCoach.PhoneNumber = coach.PhoneNumber; //電話號碼
                    existingCoach.SchoolName = coach.SchoolName; //學校名稱
                    existingCoach.Title = coach.Title; //職稱
                    existingCoach.TeamName = coach.TeamName; //隊伍名稱
                    existingCoach.SportsSpecific = coach.SportsSpecific; //專項
                    existingCoach.LastUpdated = DateTime.Now; // 設置為當前時間

                    _db.Entry(existingCoach).State = EntityState.Modified;
                    _db.SaveChanges();

                    return RedirectToAction("Dashboard", "Coach"); // 編輯後重定向到教練的儀表板
                }
            }

            // 如果模型狀態無效，重新返回視圖
            return View(coach); // 重新返回視圖，並顯示錯誤信息
        }
        #endregion
    }
}