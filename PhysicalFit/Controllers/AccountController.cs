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
            // 檢查電子郵件和帳號是否已存在
            var existingCoach = _db.Coaches
                .FirstOrDefault(c => c.Email == model.CoachEmail || c.CoachAccount == model.CoachAccount);

            if (existingCoach != null)
            {
                ModelState.AddModelError("", "該電子郵件或帳號已被使用。");
                TempData["ErrorMessage"] = "該電子郵件或帳號已被使用。";
                return View(model); // 返回註冊頁面，並顯示錯誤訊息
            }

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
                CoachID = newCoach.ID, // 設定外鍵連結到Coaches表
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
            if (!AthleteBirthday.HasValue)
            {
                TempData["ErrorMessage"] = "請提供生日";
                return View();
            }

            DateTime birthdayDate = AthleteBirthday.Value.Date;

            if (birthdayDate.Year < 1900 || birthdayDate > DateTime.Now)
            {
                TempData["ErrorMessage"] = "生日必須為有效的西元年";
                return View();
            }

            string encryptedID = ComputeSha256Hash(AthleteID.ToUpper());

            if (_db.Athletes.Any(a => a.AthleteAccount == AthleteID))
            {
                TempData["ErrorMessage"] = "此帳號已存在，請選擇其他帳號";
                return View();
            }

            if (_db.Athletes.Any(a => a.IdentityNumber == encryptedID))
            {
                TempData["ErrorMessage"] = "此身份證號碼已被註冊";
                return View();
            }

            // 處理運動員註冊
            var newAthlete = new Athletes
            {
                AthleteAccount = encryptedID,
                AthletePWD = ComputeSha256Hash(Athletepwd),
                AthleteName = AthleteName,
                Birthday = birthdayDate,
                IdentityNumber = encryptedID,
                AthleteSchool = AthleteSchool,
                TeamName = AthleteTeam,
                CoachID = _db.Coaches.FirstOrDefault(c => c.CoachName == AthleteCoach)?.ID,
                RegistrationDate = DateTime.Now,
                IsActive = true
            };
            _db.Athletes.Add(newAthlete);
            _db.SaveChanges();

            var newUser = new Users
            {
                Name = AthleteName,
                Account = encryptedID,
                Password = ComputeSha256Hash(Athletepwd),
                PhoneNumber = null,
                Email = null,
                RegistrationDate = DateTime.Now,
                IsActive = true,
                LastLoginDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                AthleteID = newAthlete.ID,
            };
            _db.Users.Add(newUser);
            _db.SaveChanges();

            return RedirectToAction("Login", "Account");
        }
        #endregion

        #region 登入
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string account, string pwd)
        {
            try
            {
                string hashedPwd = ComputeSha256Hash(pwd); //將密碼加密
                string hashedAccount = ComputeSha256Hash(account.ToUpper()); //身分證號碼加密
                Users user = null;
          
                //user = _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);
                user = _db.Users.FirstOrDefault(u => u.Account == hashedAccount && u.Password == hashedPwd); //先用加密的方式登入

                if (user == null)
                {
                    //如果找不到加密的帳號，嘗試用明碼帳號(未加密)進行查詢
                    user = _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);

                    if (user != null)
                    {
                        // 如果找到明碼帳號，則更新帳號為加密的版本
                        user.Account = hashedAccount; //將帳號更新為加密後
                        _db.SaveChanges();
                    }
                }

                if (user != null) //驗證使用者
                {
                    if (!(user.IsActive ?? false)) //如果IsActive是null，則視為false
                    {
                        ViewBag.ErrorMessage = "此帳號已被停用，請聯繫管理員";
                        return View();
                    }

                    if (user.IsTemporaryPassword ?? false) // 檢查是否是臨時密碼
                    {
                        // 如果是臨時密碼，重定向到重置密碼頁面
                        return RedirectToAction("ResetPwd", "Account", new { userId = user.UID });
                    }

                    user.LastLoginDate = DateTime.Now; // 更新用戶的最後登入時間
                    _db.SaveChanges();
                    ViewBag.UserID = user.UID; // 輸出 UID
                    Session["UserID"] = user.UID; // 將Users表的UID保存到Session

                    Session["UserRole"] = user.CoachID.HasValue ? "Coach" : "Athlete"; //設定用戶角色
                    
                    //if (user.CoachID.HasValue) // 保存 UserID 到 Session
                    //{
                    //    Session["UserID"] = user.CoachID.Value; // 如果是教練，保存 CoachID
                    //}
                    //else
                    //{
                    //    Session["UserID"] = user.AthleteID; // 如果是運動員，保存 AthleteID
                    //}

                    // 設定 FormsAuthentication Ticket
                    var authTicket = new FormsAuthenticationTicket(
                        1,
                        user.Name,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30),
                        false,
                        user.CoachID.HasValue ? user.CoachID.Value.ToString() : user.AthleteID.ToString(),
                        FormsAuthentication.FormsCookiePath);

                    // 加密並設定 Cookie
                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                    {
                        HttpOnly = true // 確保Cookie只能透過伺服器訪問
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


        // 身份證號碼格式檢查
        private bool IsIdentityNumber(string account)
        {
            // 檢查長度是否為10並且第一個字符為字母，後面9個字符為數字
            return account.Length == 10 && char.IsLetter(account[0]) && account.Substring(1).All(char.IsDigit);
        }

        // 獲取當前用戶角色
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
            Session.Clear(); // 清除所有的 Session 資訊

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

        #region 忘記密碼-教練
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

        #region 臨時密碼-學生
        // 顯示忘記密碼頁面
        public ActionResult ForgotPwd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPwd(string AthleteID)
        {
            string encryptedID = ComputeSha256Hash(AthleteID.ToUpper());
            var user = _db.Users.FirstOrDefault(u => u.Account == encryptedID);

            if (user != null)
            {
                string tempPassword = GenerateTemporaryPassword(); // 生成臨時密碼
                user.Password = ComputeSha256Hash(tempPassword); // 更新為臨時密碼
                user.IsTemporaryPassword = true; // 標記為臨時密碼
                _db.SaveChanges();

                // 可以在這裡將臨時密碼發送給學生，或透過其他方式通知他們
                TempData["SuccessMessage"] = $"臨時密碼為: {tempPassword}"; // 顯示臨時密碼訊息
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = "找不到該帳號";
            return View();
        }

        private string GenerateTemporaryPassword()
        {
            // 生成隨機臨時密碼
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        #endregion

        #region 重置密碼-學生
        public ActionResult ResetPwd(int userId)
        {
            var user = _db.Users.Find(userId);
            if (user == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var model = new ResetPwdViewModel
            {
                UserId = user.UID,

            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ResetPwd(ResetPwdViewModel model)
        {
            var user = _db.Users.Find(model.UserId);

            if (user != null)
            {
                if (model.NewPassword == model.ConfirmPassword) // 確保兩次輸入的密碼一致
                {
                    user.Password = ComputeSha256Hash(model.NewPassword); // 設定新密碼
                    user.IsTemporaryPassword = false; // 移除臨時密碼標記
                    _db.SaveChanges();

                    TempData["SuccessMessage"] = "密碼已成功重置";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    TempData["ErrorMessage"] = "密碼不一致，請重試";
                    return View(model); // 返回視圖並顯示錯誤
                }
            }
            else
            {
                TempData["ErrorMessage"] = "找不到該用戶";
                return View(model); // 返回視圖
            }
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

            var athleteAccount = User.Identity.Name; //使用 User.Identity.Name 查找運動員的帳號

            var dto = _db.Athletes.FirstOrDefault(a => a.AthleteName == athleteAccount);  //查詢運動員資料，通過 AthleteAccount 來匹配

            if (dto == null)
            {
                return RedirectToAction("Error404", "Error"); //如果查不到對應的運動員資料
            }

            // 確保日期格式符合 yyyy-MM-dd
            //ViewBag.Birthday = dto.Birthday.ToString("yyyy-MM-dd");

            // 模糊查詢教練名單，根據運動員學校名稱的部分內容進行匹配
            var coaches = _db.Coaches
                             .Where(c => c.IsActive && c.SchoolName.Contains(dto.AthleteSchool))
                             .ToList();


            // 獲取可用的教練列表
            //var coaches = _db.Coaches.Where(c => c.IsActive).ToList();

            ViewBag.Coaches = new SelectList(coaches, "ID", "CoachName", dto.CoachID);

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AthleteEdit(Athletes athlete, string NewPassword, string ConfirmPassword)
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

                    // 處理新密碼更新邏輯
                    if (!string.IsNullOrEmpty(NewPassword) && NewPassword == ConfirmPassword)
                    {
                        existingAthlete.AthletePWD = ComputeSha256Hash(NewPassword); // 更新密碼
                    }

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