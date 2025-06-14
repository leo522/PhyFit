using System;
using System.Linq;
using System.Web.Mvc;
using PhysicalFit.Utility;
using PhysicalFit.Models;

namespace PhysicalFit.Controllers
{
    public class AccountController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        #region 忘記密碼 - 簡單重設版本
        public ActionResult ForgotPassword() => View();

        [HttpPost]
        public ActionResult ForgotPassword(string Email, string NewPassword)
        {
            if (!EmailHelper.IsValidEmail(Email))
            {
                ViewBag.ErrorMessage = "Email 格式不正確";
                return View();
            }

            if (!SecurityHelper.IsValidPassword(NewPassword))
            {
                ViewBag.ErrorMessage = "密碼格式不符（至少6碼、含大小寫與數字）";
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "查無此 Email 對應帳號";
                LogHelper.LogToDb("ForgotPassword", $"查無帳號對應 email：{Email}", "WARN");
                return View();
            }

            user.Password = SecurityHelper.ComputeSha256(NewPassword);
            _db.SaveChanges();

            LogHelper.LogToDb("ForgotPassword", $"成功重設密碼：{user.Email}");
            TempData["SuccessMessage"] = "密碼已成功重設，請重新登入";
            return RedirectToAction("Login", "Auth");
        }
        #endregion

        #region 忘記密碼 - Token 驗證版本（可擴充寄信連結流程）
        public ActionResult ResetPassword(string token)
        {
            var resetRequest = _db.PasswordResetRequests.FirstOrDefault(r => r.Token == token && r.ExpiryDate > DateTime.Now);

            if (resetRequest == null)
            {
                ViewBag.ErrorMessage = "連結已過期或無效";
                LogHelper.LogToDb("ResetPassword", "Token 已過期或無效", "WARN");
                return View("Error");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid || !SecurityHelper.IsValidPassword(model.NewPassword))
            {
                ViewBag.ErrorMessage = "密碼格式不符";
                return View(model);
            }

            var request = _db.PasswordResetRequests.FirstOrDefault(r => r.Token == model.Token && r.ExpiryDate > DateTime.Now);
            if (request == null)
            {
                ViewBag.ErrorMessage = "連結已過期或無效";
                return View("Error");
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "無效的帳號";
                return View("Error");
            }

            user.Password = SecurityHelper.ComputeSha256(model.NewPassword);
            request.changeDate = DateTime.Now;
            _db.PasswordResetRequests.Remove(request);
            _db.SaveChanges();

            LogHelper.LogToDb("ResetPassword", $"使用者密碼成功重設，Email：{user.Email}");
            TempData["SuccessMessage"] = "密碼已更新，請重新登入";
            return RedirectToAction("Login", "Auth");
        }
        #endregion

        #region 臨時密碼 - 運動員
        public ActionResult ForgotPwd() => View();

        [HttpPost]
        public ActionResult ForgotPwd(string AthleteID)
        {
            var encryptedID = SecurityHelper.ComputeSha256(AthleteID.ToUpper());
            var user = _db.Users.FirstOrDefault(u => u.Account == encryptedID);

            if (user == null)
            {
                TempData["ErrorMessage"] = "找不到該帳號";
                LogHelper.LogToDb("ForgotPwd", $"找不到學生帳號：{AthleteID}", "WARN");
                return View();
            }

            var tempPwd = SecurityHelper.GenerateTemporaryPassword();
            user.Password = SecurityHelper.ComputeSha256(tempPwd);
            user.IsTemporaryPassword = true;
            _db.SaveChanges();

            LogHelper.LogToDb("ForgotPwd", $"發送臨時密碼給：{AthleteID}");
            TempData["SuccessMessage"] = $"臨時密碼為：{tempPwd}，請登入後立即修改";
            return RedirectToAction("Login", "Auth");
        }
        #endregion

        #region 臨時密碼重設
        public ActionResult ResetPwd(int userId)
        {
            var user = _db.Users.Find(userId);
            if (user == null)
            {
                LogHelper.LogToDb("ResetPwd", $"找不到用戶 ID：{userId}", "WARN");
                return RedirectToAction("Error404", "Error");
            }

            return View(new ResetPwdViewModel { UserId = user.UID });
        }

        [HttpPost]
        public ActionResult ResetPwd(ResetPwdViewModel model)
        {
            var user = _db.Users.Find(model.UserId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "找不到該用戶";
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "密碼與確認不一致";
                return View(model);
            }

            if (!SecurityHelper.IsValidPassword(model.NewPassword))
            {
                TempData["ErrorMessage"] = "密碼格式錯誤（至少6碼，需含大小寫字母與數字）";
                return View(model);
            }

            user.Password = SecurityHelper.ComputeSha256(model.NewPassword);
            user.IsTemporaryPassword = false;
            _db.SaveChanges();

            LogHelper.LogToDb("ResetPwd", $"使用者密碼成功重設 ID：{user.UID}");
            TempData["SuccessMessage"] = "密碼已成功更新，請重新登入";
            return RedirectToAction("Login", "Auth");
        }
        #endregion

        //#region 避免使用者完全沒互動導致 session 過期
        //[HttpGet]
        //public ActionResult KeepAlive()
        //{
        //    return new HttpStatusCodeResult(200);
        //}
        //#endregion

        //#region 註冊角色選擇
        //public ActionResult Register()
        //{
        //    return View();
        //}
        //#endregion

        //#region 小學學校代碼查詢
        //[HttpGet]
        //public JsonResult GetSchoolByCode(string code)
        //{
        //    LogHelper.LogToDb("GetSchoolByCode", $"查詢小學代碼開頭：{code}");

        //    var dto = _db.PrimarySchoolList.FirstOrDefault(s => s.SchoolCode.ToString().StartsWith(code));

        //    if (dto != null)
        //    {
        //        return Json(new { SchoolName = dto.SchoolName, CityName = dto.CityName }, JsonRequestBehavior.AllowGet);
        //    }

        //    return Json("", JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        //#region 國中學校代碼查詢
        //[HttpGet]
        //public JsonResult GetJuinorSchoolByCode(string code)
        //{
        //    LogHelper.LogToDb("GetJuinorSchoolByCode", $"查詢國中代碼開頭：{code}");

        //    var dto = _db.JuniorHighSchoolList.FirstOrDefault(j => j.SchoolCode.ToString().StartsWith(code));

        //    if (dto != null)
        //    {
        //        return Json(new { SchoolName = dto.SchoolName, CityName = dto.CityName }, JsonRequestBehavior.AllowGet);
        //    }

        //    return Json("", JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        //#region 模糊查詢學校名稱
        //[HttpGet]
        //public JsonResult GetSchoolByName(string name)
        //{
        //    LogHelper.LogToDb("GetSchoolByName", $"模糊查詢學校名稱：{name}");

        //    var primarySchoolResults = _db.PrimarySchoolList
        //             .Where(s => s.SchoolName.Contains(name))
        //             .Select(s => new { s.SchoolCode, s.SchoolName, s.CityName })
        //             .ToList();

        //    var juniorHighSchoolResults = _db.JuniorHighSchoolList
        //                                     .Where(j => j.SchoolName.Contains(name))
        //                                     .Select(j => new { j.SchoolCode, j.SchoolName, j.CityName})
        //                                     .ToList();

        //    var generalHighSchoolResults = _db.GeneralHighSchoolList
        //             .Where(g => g.SchoolName.Contains(name))
        //             .Select(g => new { g.SchoolCode, g.SchoolName, g.CityName })
        //             .ToList();

        //    var universitySchoolResults = _db.UniversitySchoolList
        //         .Where(u => u.SchoolName.Contains(name))
        //         .Select(u => new { u.SchoolCode, u.SchoolName, u.CityName })
        //         .ToList();

        //    var results = primarySchoolResults
        //        .Concat(juniorHighSchoolResults)
        //        .Concat(generalHighSchoolResults)
        //        .Concat(universitySchoolResults)
        //        .ToList();

        //    if (results.Any())
        //    {
        //        return Json(results, JsonRequestBehavior.AllowGet);
        //    }

        //    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        //#region 取得單位名稱
        //public JsonResult GetOrganizations()
        //{
        //    var data = _db.Organization
        //                  .Select(o => new { o.ID, o.OrgName })
        //                  .ToList();

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        //#endregion

        //#region 教練註冊帳號
        //public ActionResult RegisterCoach(string schoolID, int? orgID)
        //{
        //    var model = new RegisterCoachViewModel();

        //    if (!string.IsNullOrEmpty(schoolID))
        //    {
        //        model.SchoolID = schoolID;
        //        string schoolName = _db.PrimarySchoolList
        //            .Where(s => s.SchoolCode.ToString() == schoolID)
        //            .Select(s => s.SchoolName)
        //            .FirstOrDefault();

        //        if (string.IsNullOrEmpty(schoolName))
        //        {
        //            schoolName = _db.JuniorHighSchoolList
        //                            .Where(s => s.SchoolCode.ToString() == schoolID)
        //                            .Select(s => s.SchoolName)
        //                            .FirstOrDefault();
        //        }
        //        if (string.IsNullOrEmpty(schoolName))
        //        {
        //            schoolName = _db.GeneralHighSchoolList
        //                            .Where(s => s.SchoolCode.ToString() == schoolID)
        //                            .Select(s => s.SchoolName)
        //                            .FirstOrDefault();
        //        }
        //        if (string.IsNullOrEmpty(schoolName))
        //        {
        //            schoolName = _db.UniversitySchoolList
        //                            .Where(s => s.SchoolCode.ToString() == schoolID)
        //                            .Select(s => s.SchoolName)
        //                            .FirstOrDefault();
        //        }
        //        model.CoachSchool = schoolName;
        //    }
        //    else if (orgID.HasValue)
        //    {
        //        var org = _db.Organization.FirstOrDefault(o => o.ID == orgID.Value);
        //        if (org != null)
        //        {
        //            model.Organize = org.OrgName;
        //        }
        //    }

        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult RegisterCoach(RegisterCoachViewModel model)
        //{
        //    var existingCoach = _db.Coaches
        //        .FirstOrDefault(c => c.Email == model.CoachEmail || c.CoachAccount == model.CoachAccount);

        //    if (existingCoach != null)
        //    {
        //        ModelState.AddModelError("", "該電子郵件或帳號已被使用。");
        //        TempData["ErrorMessage"] = "該電子郵件或帳號已被使用。";

        //        LogHelper.LogToDb("RegisterCoach", $"註冊失敗：帳號或 Email 已存在，帳號：{model.CoachAccount}", "WARN");

        //        return View(model);
        //    }

        //    var newCoach = new Coaches
        //    {
        //        CoachName = model.CoachName,
        //        Email = model.CoachEmail,
        //        CoachAccount = model.CoachAccount,
        //        CoachPwd = ComputeSha256Hash(model.Coachpwd),
        //        PhoneNumber = model.CoachPhone,
        //        SchoolID = model.SchoolID,
        //        SchoolName = model.CoachSchool,
        //        Organize = model.Organize,
        //        Title = "教練",
        //        TeamName = model.CoachTeam,
        //        SportsSpecific = model.CoachSpecialty,
        //        RegistrationDate = DateTime.Now,
        //        IsActive = true
        //    };
        //    _db.Coaches.Add(newCoach);
        //    _db.SaveChanges();

        //    var newUser = new Users
        //    {
        //        Name = model.CoachName,
        //        Account = model.CoachAccount,
        //        Password = ComputeSha256Hash(model.Coachpwd),
        //        PhoneNumber = model.CoachPhone,
        //        Email = model.CoachEmail,
        //        RegistrationDate = DateTime.Now,
        //        IsActive = true,
        //        LastLoginDate = DateTime.Now,
        //        CreatedDate = DateTime.Now,
        //        CoachID = newCoach.ID,
        //    };
        //    _db.Users.Add(newUser);
        //    _db.SaveChanges();

        //    LogHelper.LogToDb("RegisterCoach", $"註冊新教練帳號：{model.CoachAccount}");

        //    return RedirectToAction("Login", "Account");
        //}
        //#endregion

        //#region 學生運動員註冊
        //public ActionResult RegisterAthlete(string schoolID, int? orgID)
        //{
        //    var model = new Athletes();

        //    if (!string.IsNullOrEmpty(schoolID))
        //    {
        //        string schoolName = _db.PrimarySchoolList
        //                    .Where(s => s.SchoolCode.ToString() == schoolID)
        //                    .Select(s => s.SchoolName)
        //                    .FirstOrDefault();

        //        if (string.IsNullOrEmpty(schoolName))
        //        {
        //            schoolName = _db.JuniorHighSchoolList
        //                            .Where(s => s.SchoolCode.ToString() == schoolID)
        //                            .Select(s => s.SchoolName)
        //                            .FirstOrDefault();
        //        }
        //        if (string.IsNullOrEmpty(schoolName))
        //        {
        //            schoolName = _db.GeneralHighSchoolList
        //                            .Where(s => s.SchoolCode.ToString() == schoolID)
        //                            .Select(s => s.SchoolName)
        //                            .FirstOrDefault();
        //        }
        //        if (string.IsNullOrEmpty(schoolName))
        //        {
        //            schoolName = _db.UniversitySchoolList
        //                            .Where(s => s.SchoolCode.ToString() == schoolID)
        //                            .Select(s => s.SchoolName)
        //                            .FirstOrDefault();
        //        }

        //        model.AthleteSchool = schoolName;
        //    }
        //    else if (orgID.HasValue)
        //    {
        //        var org = _db.Organization.FirstOrDefault(o => o.ID == orgID.Value);
        //        if (org != null)
        //        {
        //            model.AthleteOrganize = org.OrgName;
        //        }
        //    }

        //    var coaches = _db.Coaches
        //        .Where(c => c.IsActive &&
        //            (
        //                (!string.IsNullOrEmpty(model.AthleteSchool) && c.SchoolName == model.AthleteSchool)
        //                || (!string.IsNullOrEmpty(model.AthleteOrganize) && c.Organize == model.AthleteOrganize)
        //            ))
        //        .ToList();

        //    ViewBag.Coaches = new List<SelectListItem>(
        //        coaches.Select(c => new SelectListItem
        //        {
        //            Value = c.ID.ToString(),
        //            Text = c.CoachName
        //        }));

        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult RegisterAthlete(string AthleteName, DateTime? AthleteBirthday, string AthleteID, string Athletepwd, string AthleteSchool, string AthleteTeam, string AthleteCoach, string AthleteOrganize, List<int> CoachIDs, bool NoCoach = false)
        //{
        //    if (!AthleteBirthday.HasValue)
        //    {
        //        TempData["ErrorMessage"] = "請提供生日";

        //        LogHelper.LogToDb("RegisterAthlete", $"註冊失敗：未提供生日，帳號={AthleteID}", "WARN");

        //        return View();
        //    }

        //    DateTime birthdayDate = AthleteBirthday.Value.Date;

        //    if (birthdayDate.Year < 1900 || birthdayDate > DateTime.Now)
        //    {
        //        TempData["ErrorMessage"] = "生日必須為有效的西元年";

        //        LogHelper.LogToDb("RegisterAthlete", $"註冊失敗：生日格式錯誤（{birthdayDate}），帳號={AthleteID}", "WARN");

        //        return View();
        //    }

        //    string encryptedID = ComputeSha256Hash(AthleteID.ToUpper());

        //    if (_db.Athletes.Any(a => a.AthleteAccount == AthleteID))
        //    {
        //        TempData["ErrorMessage"] = "此帳號已存在，請選擇其他帳號";

        //        LogHelper.LogToDb("RegisterAthlete", $"註冊失敗：帳號已存在，帳號={AthleteID}", "WARN");

        //        return View();
        //    }

        //    if (_db.Athletes.Any(a => a.IdentityNumber == encryptedID))
        //    {
        //        TempData["ErrorMessage"] = "此身份證號碼已被註冊";

        //        LogHelper.LogToDb("RegisterAthlete", $"註冊失敗：身份證已被註冊，帳號={AthleteID}", "WARN");

        //        return View();
        //    }

        //    var newAthlete = new Athletes
        //    {
        //        AthleteAccount = encryptedID,
        //        AthletePWD = ComputeSha256Hash(Athletepwd),
        //        AthleteName = AthleteName,
        //        Birthday = birthdayDate,
        //        IdentityNumber = encryptedID,
        //        AthleteSchool = AthleteSchool,
        //        AthleteOrganize = AthleteOrganize,
        //        TeamName = AthleteTeam,
        //        RegistrationDate = DateTime.Now,
        //        IsActive = true
        //    };

        //    _db.Athletes.Add(newAthlete);
        //    _db.SaveChanges();

        //    // 綁定多位教練
        //    if (!NoCoach && CoachIDs != null)
        //    {
        //        foreach (var coachId in CoachIDs)
        //        {
        //            _db.AthleteCoachRelations.Add(new AthleteCoachRelations
        //            {
        //                AthleteID = newAthlete.ID,
        //                CoachID = coachId
        //            });
        //        }
        //        _db.SaveChanges();
        //    }

        //    var newUser = new Users
        //    {
        //        Name = AthleteName,
        //        Account = encryptedID,
        //        Password = ComputeSha256Hash(Athletepwd),
        //        RegistrationDate = DateTime.Now,
        //        IsActive = true,
        //        LastLoginDate = DateTime.Now,
        //        CreatedDate = DateTime.Now,
        //        AthleteID = newAthlete.ID
        //    };

        //    _db.Users.Add(newUser);
        //    _db.SaveChanges();

        //    LogHelper.LogToDb("RegisterAthlete", $"註冊成功：新運動員帳號={AthleteID}");

        //    return RedirectToAction("Login", "Account");
        //}
        //#endregion

        //#region 登入
        //public ActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Login(string account, string pwd)
        //{
        //    try
        //    {
        //        string hashedPwd = ComputeSha256Hash(pwd);
        //        string hashedAccount = ComputeSha256Hash(account.ToUpper());
        //        Users user = null;

        //        user = _db.Users.FirstOrDefault(u => u.Account == hashedAccount && u.Password == hashedPwd);

        //        if (user == null)
        //        {
        //            user = _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);

        //            if (user != null)
        //            {
        //                user.Account = hashedAccount;
        //                _db.SaveChanges();
        //            }
        //        }

        //        if (user != null)
        //        {
        //            if (!(user.IsActive ?? false))
        //            {
        //                ViewBag.ErrorMessage = "此帳號已被停用，請聯繫管理員";

        //                LogHelper.LogToDb("Login", $"登入失敗，帳號已被停用：{account}", "WARN");

        //                return View();
        //            }

        //            if (user.IsTemporaryPassword ?? false)
        //            {
        //                LogHelper.LogToDb("Login", $"使用者登入為臨時密碼狀態，帳號：{account}");

        //                return RedirectToAction("ResetPwd", "Account", new { userId = user.UID });
        //            }

        //            user.LastLoginDate = DateTime.Now;
        //            _db.SaveChanges();
        //            ViewBag.UserID = user.UID;
        //            Session["UserID"] = user.UID;

        //            Session["UserRole"] = user.CoachID.HasValue ? "Coach" : "Athlete";

        //            var authTicket = new FormsAuthenticationTicket(
        //                1,
        //                user.Name,
        //                DateTime.Now,
        //                DateTime.Now.AddMinutes(30),
        //                false,
        //                user.CoachID.HasValue ? user.CoachID.Value.ToString() : user.AthleteID.ToString(),
        //                FormsAuthentication.FormsCookiePath);

        //            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
        //            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
        //            {
        //                HttpOnly = true
        //            };
        //            Response.Cookies.Add(authCookie);

        //            LogHelper.LogToDb("Login", $"使用者登入成功，帳號：{account}");

        //            return RedirectToAction("dashboard", "PhyFit");
        //        }
        //        else
        //        {
        //            ViewBag.ErrorMessage = "帳號或密碼錯誤";

        //            LogHelper.LogToDb("Login", $"登入失敗，帳號或密碼錯誤：{account}", "WARN");

        //            return View();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var inner = ex.InnerException != null ? ex.InnerException.Message : "無內層例外";
        //        TempData["ErrorMsg"] = inner;
        //        return RedirectToAction("Error404", "Error");
        //    }
        //}

        //private bool IsIdentityNumber(string account)
        //{
        //    return account.Length == 10 && char.IsLetter(account[0]) && account.Substring(1).All(char.IsDigit);
        //}

        //public JsonResult GetUserRole()
        //{
        //    var userRole = Session["UserRole"]?.ToString();
        //    return Json(new { userRole }, JsonRequestBehavior.AllowGet);
        //}

        //#endregion

        //#region 學校代碼登入
        //public ActionResult SchoolLogin()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult SchoolLogin(string SchoolCode)
        //{
        //    try
        //    {
        //        var dto = _db.PrimarySchoolList
        //                      .FirstOrDefault(s => s.SchoolCode.ToString() == SchoolCode);


        //        if (dto != null)
        //        {
        //            Session["LoggedIn"] = true;
        //            Session["schoolName"] = dto.SchoolName;

        //            LogHelper.LogToDb("SchoolLogin", $"成功登入學校代碼：{SchoolCode}（{dto.SchoolName}）");

        //            return RedirectToAction("Login", "Account");
        //        }
        //        else
        //        {
        //            ViewBag.ErrorMessage = "學校代碼錯誤";

        //            LogHelper.LogToDb("SchoolLogin", $"學校代碼錯誤：{SchoolCode}", "WARN");

        //            return View();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("其他錯誤: " + ex.Message);

        //        LogHelper.LogToDb("SchoolLogin", "學校代碼登入時發生例外", "ERROR", ex);

        //        return View("Error");
        //    }
        //}
        //#endregion

        //#region 密碼加密
        //private static string ComputeSha256Hash(string rawData)
        //{
        //    using (SHA256 sha256Hash = SHA256.Create())
        //    {
        //        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

        //        StringBuilder builder = new StringBuilder();
        //        for (int i = 0; i < bytes.Length; i++)
        //        {
        //            builder.Append(bytes[i].ToString("x2"));
        //        }
        //        return builder.ToString();
        //    }
        //}
        //#endregion

        //#region 登出
        //public ActionResult Logout()
        //{
        //    Session.Clear();

        //    LogHelper.LogToDb("Logout", $"使用者 {User.Identity.Name} 登出");

        //    FormsAuthentication.SignOut();

        //    Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    Response.Cache.SetNoStore();

        //    return RedirectToAction("Login", "Account");
        //}
        //#endregion

        //#region 忘記密碼-教練
        //public ActionResult ForgotPassword()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult SendResetLink(string Email, string NewPassword)
        //{
        //    try
        //    {
        //        var user = _db.Users.FirstOrDefault(u => u.Email == Email);
        //        if (user == null)
        //        {
        //            ViewBag.errormessage = "此email尚未註冊";

        //            LogHelper.LogToDb("ForgotPassword", $"查無帳號對應 email：{Email}", "WARN");

        //            if (string.IsNullOrEmpty(NewPassword) || NewPassword.Length < 6)
        //            {
        //                ViewBag.errormessage = "密碼至少6位數";
        //                return View("ForgotPassword");
        //            }
        //        }
        //        user.Password = ComputeSha256Hash(NewPassword);
        //        _db.SaveChanges();

        //        LogHelper.LogToDb("ForgotPassword", $"成功重設密碼：{user.Email}");

        //        return RedirectToAction("Login");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("其他錯誤: " + ex.Message);
        //        return View("Error");
        //    }
        //}
        ////[HttpPost]
        ////public ActionResult SendResetLink(string Email)
        ////{
        ////    try
        ////    {
        ////        var user = _db.Users.FirstOrDefault(u => u.Email == Email);

        ////        if (user == null)
        ////        {
        ////            ViewBag.ErrorMessage = "此Email尚未註冊";

        ////            LogHelper.LogToDb("ForgotPassword", $"查無帳號對應 Email：{Email}", "WARN");

        ////            return View("ForgotPassword");
        ////        }

        ////        var resetToken = Guid.NewGuid().ToString();

        ////        var resetPW = new PasswordResetRequests
        ////        {
        ////            Email = Email,
        ////            Token = resetToken,
        ////            ExpiryDate = DateTime.Now.AddMinutes(5),
        ////            UserAccount = user.Name,
        ////            changeDate = DateTime.Now
        ////        };
        ////        _db.PasswordResetRequests.Add(resetPW);
        ////        _db.SaveChanges();

        ////        var resetLink = Url.Action("ResetPassword", "Account", new { token = resetToken }, Request.Url.Scheme);

        ////        var emailBody = $"請點擊以下連結重置您的密碼：{resetLink}，連結有效時間為5分鐘";

        ////        SendEmail(Email, "重置密碼", emailBody);

        ////        ViewBag.Message = "重置密碼連結已發送至您的郵箱";

        ////        LogHelper.LogToDb("ForgotPassword", $"發送重設密碼連結給：{Email}");

        ////        return View("ForgotPassword");
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        LogHelper.LogToDb("ForgotPassword", $"寄送密碼重設連結發生錯誤，Email：{Email}", "ERROR", ex);

        ////        Console.WriteLine("其他錯誤: " + ex.Message);
        ////        return View("Error");
        ////    }
        ////}

        //private void SendEmail(string toEmail, string subject, string body, string attachmentPath = null)
        //{
        //    var fromEmail = "@tiss.org.tw";
        //    var fromPassword = "";
        //    var displayName = "運科中心資訊組";


        //    var smtpClient = new SmtpClient("smtp.gmail.com")
        //    {
        //        Port = 587,
        //        Credentials = new NetworkCredential(fromEmail, fromPassword),
        //        EnableSsl = true,
        //    };

        //    var mailMessage = new MailMessage
        //    {
        //        From = new MailAddress(fromEmail, displayName),
        //        Subject = subject,
        //        Body = body,
        //        IsBodyHtml = true,
        //    };

        //    foreach (var email in toEmail.Split(','))
        //    {
        //        mailMessage.To.Add(email.Trim());
        //    }

        //    if (!string.IsNullOrEmpty(attachmentPath))
        //    {
        //        Attachment attachment = new Attachment(attachmentPath);
        //        mailMessage.Attachments.Add(attachment);
        //    }

        //    try
        //    {
        //        smtpClient.Send(mailMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("郵件發送失敗: " + ex.Message);
        //    }
        //}

        //public ActionResult ResetPassword(string token)
        //{
        //    try
        //    {
        //        var resetRequest = _db.PasswordResetRequests.SingleOrDefault(r => r.Token == token && r.ExpiryDate > DateTime.Now);

        //        if (resetRequest == null)
        //        {
        //            ViewBag.ErrorMessage = "無效或過期的要求";

        //            LogHelper.LogToDb("ResetPassword", "無效或過期的密碼重設請求", "WARN");

        //            return View("Error");
        //        }

        //        var model = new ResetPasswordViewModel
        //        {
        //            Token = token
        //        };

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.LogToDb("ResetPassword", "取得密碼重設頁面時發生錯誤", "ERROR", ex);

        //        Console.WriteLine("其他錯誤: " + ex.Message);
        //        return View("Error");
        //    }

        //}

        //[HttpPost]
        //public ActionResult ResetPassword(ResetPasswordViewModel model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            var errors = ModelState.Values.SelectMany(v => v.Errors);
        //            foreach (var error in errors)
        //            {
        //                Console.WriteLine(error.ErrorMessage);
        //            }
        //            return View(model);
        //        }

        //        var resetRequest = _db.PasswordResetRequests
        //            .FirstOrDefault(r => r.Token == model.Token && r.ExpiryDate > DateTime.Now);

        //        if (resetRequest == null)
        //        {
        //            ViewBag.ErrorMessage = "無效或過期的要求";

        //            LogHelper.LogToDb("ResetPassword", "無效或過期的密碼重設請求", "WARN");

        //            return View("Error");
        //        }

        //        var user = _db.Users
        //            .FirstOrDefault(u => u.Email == resetRequest.Email);

        //        if (user == null)
        //        {
        //            ViewBag.ErrorMessage = "無效的帳號";

        //            LogHelper.LogToDb("ResetPassword", $"密碼重設時找不到對應帳號：{resetRequest.Email}", "WARN");

        //            return View("Error");
        //        }

        //        user.Password = ComputeSha256Hash(model.NewPassword);

        //        resetRequest.UserAccount = user.Name;
        //        resetRequest.changeDate = DateTime.Now;

        //        _db.PasswordResetRequests.Remove(resetRequest);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.LogToDb("ResetPassword", "密碼重設時發生錯誤", "ERROR", ex);

        //        Console.WriteLine("其他錯誤: " + ex.Message);
        //        return View("Error");
        //    }

        //    try
        //    {
        //        _db.SaveChanges();

        //        LogHelper.LogToDb("ResetPassword", $"使用者密碼成功重設，帳號：{model.Token}");
        //    }
        //    catch (DbEntityValidationException ex)
        //    {
        //        foreach (var validationErrors in ex.EntityValidationErrors)
        //        {
        //            foreach (var validationError in validationErrors.ValidationErrors)
        //            {
        //                Console.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
        //            }
        //        }
        //        LogHelper.LogToDb("ResetPassword", "密碼儲存時發生驗證錯誤", "ERROR", ex);

        //        throw;
        //    }

        //    ViewBag.Message = "您的密碼已成功重置";
        //    return RedirectToAction("Login");
        //}
        //#endregion

        //#region 臨時密碼-學生
        //public ActionResult ForgotPwd()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult ForgotPwd(string AthleteID)
        //{
        //    string encryptedID = ComputeSha256Hash(AthleteID.ToUpper());
        //    var user = _db.Users.FirstOrDefault(u => u.Account == encryptedID);

        //    if (user != null)
        //    {
        //        string tempPassword = GenerateTemporaryPassword();
        //        user.Password = ComputeSha256Hash(tempPassword);
        //        user.IsTemporaryPassword = true;
        //        _db.SaveChanges();

        //        LogHelper.LogToDb("ForgotPwd", $"發送臨時密碼給：{AthleteID}");

        //        TempData["SuccessMessage"] = $"臨時密碼為: {tempPassword}";
        //        return RedirectToAction("Login", "Account");
        //    }

        //    LogHelper.LogToDb("ForgotPwd", $"找不到學生帳號：{AthleteID}", "WARN");

        //    TempData["ErrorMessage"] = "找不到該帳號";
        //    return View();
        //}

        //private string GenerateTemporaryPassword()
        //{
        //    var random = new Random();
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        //    return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        //}
        //#endregion

        //#region 重置密碼-學生
        //public ActionResult ResetPwd(int userId)
        //{
        //    var user = _db.Users.Find(userId);
        //    if (user == null)
        //    {
        //        LogHelper.LogToDb("ResetPwd", $"找不到用戶 ID：{userId}", "WARN");

        //        return RedirectToAction("Error404", "Error");
        //    }

        //    var model = new ResetPwdViewModel
        //    {
        //        UserId = user.UID,

        //    };

        //    return View(model);
        //}

        //[HttpPost]
        //public ActionResult ResetPwd(ResetPwdViewModel model)
        //{
        //    var user = _db.Users.Find(model.UserId);

        //    if (user != null)
        //    {
        //        if (model.NewPassword == model.ConfirmPassword)
        //        {
        //            user.Password = ComputeSha256Hash(model.NewPassword);
        //            user.IsTemporaryPassword = false;
        //            _db.SaveChanges();

        //            LogHelper.LogToDb("ResetPwd", $"使用者重設密碼成功，ID：{model.UserId}");

        //            TempData["SuccessMessage"] = "密碼已成功重置";
        //            return RedirectToAction("Login", "Account");
        //        }
        //        else
        //        {
        //            LogHelper.LogToDb("ResetPwd", $"密碼重設失敗（不一致），ID：{model.UserId}", "WARN");

        //            TempData["ErrorMessage"] = "密碼不一致，請重試";
        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        LogHelper.LogToDb("ResetPwd", $"找不到用戶 ID：{model.UserId}", "WARN");

        //        TempData["ErrorMessage"] = "找不到該用戶";
        //        return View(model);
        //    }
        //}
        //#endregion

        //#region 運動員資料編輯頁
        //public ActionResult AthleteEdit()
        //{
        //    if (!User.Identity.IsAuthenticated || Session["UserRole"]?.ToString() != "Athlete")
        //    {
        //        LogHelper.LogToDb("AthleteEdit", "未登入或非運動員身分存取", "WARN");
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var athleteAccount = User.Identity.Name;
        //    var dto = _db.Athletes.FirstOrDefault(a => a.AthleteName == athleteAccount);

        //    if (dto == null)
        //    {
        //        LogHelper.LogToDb("AthleteEdit", $"找不到運動員資料：{athleteAccount}", "ERROR");
        //        return RedirectToAction("Error404", "Error");
        //    }

        //    LogHelper.LogToDb("AthleteEdit", $"載入運動員編輯頁成功：{athleteAccount}");

        //    ViewBag.Coaches = GetFilteredCoaches(dto);

        //    return View(dto);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AthleteEdit(Athletes athlete, string NewPassword, string ConfirmPassword, List<int> CoachIDs, bool NoCoach = false)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var existingAthlete = _db.Athletes.Find(athlete.ID);

        //        if (existingAthlete != null)
        //        {
        //            existingAthlete.AthleteName = athlete.AthleteName;
        //            existingAthlete.AthleteSchool = athlete.AthleteSchool;
        //            existingAthlete.TeamName = athlete.TeamName;
        //            existingAthlete.Birthday = athlete.Birthday;
        //            existingAthlete.LastUpdated = DateTime.Now;

        //            if (!string.IsNullOrEmpty(NewPassword) && NewPassword == ConfirmPassword)
        //            {
        //                existingAthlete.AthletePWD = ComputeSha256Hash(NewPassword);
        //            }

        //            var oldRelations = _db.AthleteCoachRelations.Where(r => r.AthleteID == athlete.ID);
        //            _db.AthleteCoachRelations.RemoveRange(oldRelations);

        //            if (!NoCoach && CoachIDs != null)
        //            {
        //                foreach (var coachId in CoachIDs)
        //                {
        //                    _db.AthleteCoachRelations.Add(new AthleteCoachRelations
        //                    {
        //                        AthleteID = athlete.ID,
        //                        CoachID = coachId
        //                    });
        //                }
        //            }

        //            _db.SaveChanges();

        //            LogHelper.LogToDb("AthleteEdit", $"運動員 {athlete.AthleteName} 資料編輯成功");

        //            return RedirectToAction("dashboard", "PhyFit");
        //        }
        //        else
        //        {
        //            LogHelper.LogToDb("AthleteEdit", $"找不到運動員 ID：{athlete.ID}", "WARN");
        //        }
        //    }
        //    else
        //    {
        //        LogHelper.LogToDb("AthleteEdit", "ModelState 驗證失敗", "WARN");
        //    }

        //    ViewBag.Coaches = GetFilteredCoaches(athlete);

        //    return View(athlete);
        //}

        //private List<SelectListItem> GetFilteredCoaches(Athletes athlete)
        //{
        //    var query = _db.Coaches.Where(c => c.IsActive);

        //    if (!string.IsNullOrEmpty(athlete.AthleteSchool))
        //    {
        //        query = query.Where(c => c.SchoolName == athlete.AthleteSchool);
        //    }
        //    else if (!string.IsNullOrEmpty(athlete.AthleteOrganize))
        //    {
        //        query = query.Where(c => c.Organize == athlete.AthleteOrganize);
        //    }

        //    return query.Select(c => new SelectListItem
        //    {
        //        Value = c.ID.ToString(),
        //        Text = c.CoachName
        //    }).ToList();
        //}
        //#endregion

        //#region 教練資料編輯頁
        //public ActionResult CoachEdit() 
        //{
        //    if (!User.Identity.IsAuthenticated || Session["UserRole"]?.ToString() != "Coach")
        //    {
        //        LogHelper.LogToDb("CoachEdit", "未授權使用者嘗試存取教練編輯頁", "WARN");

        //        return RedirectToAction("Login", "Account");
        //    }

        //    var coachName = User.Identity.Name;
        //    var coach = _db.Coaches.FirstOrDefault(c => c.CoachName == coachName);

        //    if (coach == null)
        //    {
        //        LogHelper.LogToDb("CoachEdit", $"找不到教練資料：{coachName}", "ERROR");

        //        return RedirectToAction("Error404", "Error");
        //    }

        //    LogHelper.LogToDb("CoachEdit", $"教練進入編輯頁面：{coachName}");

        //    return View(coach);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CoachEdit(Coaches coach)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var existingCoach = _db.Coaches.Find(coach.ID);
        //        if (existingCoach != null)
        //        {
        //            existingCoach.CoachName = coach.CoachName;
        //            existingCoach.Email = coach.Email;
        //            existingCoach.PhoneNumber = coach.PhoneNumber;
        //            existingCoach.SchoolName = coach.SchoolName;
        //            existingCoach.Title = coach.Title;
        //            existingCoach.TeamName = coach.TeamName;
        //            existingCoach.SportsSpecific = coach.SportsSpecific;
        //            existingCoach.LastUpdated = DateTime.Now;

        //            _db.Entry(existingCoach).State = EntityState.Modified;
        //            _db.SaveChanges();

        //            LogHelper.LogToDb("CoachEdit", $"教練 {coach.CoachName} 成功更新個人資料");

        //            return RedirectToAction("dashboard", "PhyFit");
        //        }
        //        else
        //        {
        //            LogHelper.LogToDb("CoachEdit", $"教練更新失敗，找不到 ID 為 {coach.ID} 的紀錄", "ERROR");
        //        }
        //    }
        //    else
        //    {
        //        LogHelper.LogToDb("CoachEdit", $"教練 {coach.CoachName} 更新失敗，ModelState 無效", "WARN");
        //    }
        //    return View(coach);
        //}
        //#endregion
    }
}