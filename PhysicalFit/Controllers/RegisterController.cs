using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhysicalFit.Utility;
using PhysicalFit.ViewModels;

namespace PhysicalFit.Controllers
{
    public class RegisterController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        private readonly SchoolService _schoolService;

        public RegisterController()
        {
            _schoolService = new SchoolService(_db);
        }

        #region 註冊角色選擇
        [HttpGet]
        public ActionResult SelectRole()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ProceedToRegister(string role, string orgType, string schoolID, string orgID)
        {
            if (role == "Coach")
            {
                if (orgType == "School")
                    return RedirectToAction("RegisterCoach", new { schoolID });
                else
                    return RedirectToAction("RegisterCoach", new { orgID });
            }
            else if (role == "Athlete")
            {
                if (orgType == "School")
                    return RedirectToAction("RegisterAthlete", new { schoolID });
                else
                    return RedirectToAction("RegisterAthlete", new { orgID });
            }

            TempData["ErrorMessage"] = "請選擇正確的角色與單位";
            return RedirectToAction("SelectRole");
        }
        #endregion

        #region 建立教練帳號（GET）
        [HttpGet]
        public ActionResult RegisterCoach()
        {
            return View(new RegisterCoachViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterCoach(RegisterCoachViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "請確認所有欄位格式正確";
                return View(model);
            }

            if (!SecurityHelper.IsValidPassword(model.Coachpwd))
            {
                TempData["ErrorMessage"] = "密碼格式錯誤（至少6碼，需含大小寫與數字）";
                return View(model);
            }

            if (!EmailHelper.IsValidEmail(model.CoachEmail))
            {
                TempData["ErrorMessage"] = "信箱格式錯誤";
                return View(model);
            }

            var exists = _db.Coaches.Any(c => c.Email == model.CoachEmail || c.CoachAccount == model.CoachAccount);
            if (exists)
            {
                TempData["ErrorMessage"] = "此帳號或信箱已存在";
                return View(model);
            }

            var coach = new Coaches
            {
                CoachName = model.CoachName,
                Email = model.CoachEmail,
                CoachAccount = model.CoachAccount,
                CoachPwd = SecurityHelper.ComputeSha256(model.Coachpwd),
                PhoneNumber = model.CoachPhone,
                SchoolID = model.SchoolID,
                SchoolName = model.CoachSchool,
                Organize = model.Organize,
                Title = "教練",
                TeamName = model.CoachTeam,
                SportsSpecific = model.CoachSpecialty,
                RegistrationDate = DateTime.Now,
                IsActive = true
            };
            _db.Coaches.Add(coach);
            _db.SaveChanges();

            var user = new Users
            {
                Name = model.CoachName,
                Account = model.CoachAccount,
                Password = SecurityHelper.ComputeSha256(model.Coachpwd),
                PhoneNumber = model.CoachPhone,
                Email = model.CoachEmail,
                RegistrationDate = DateTime.Now,
                IsActive = true,
                LastLoginDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                CoachID = coach.ID
            };
            _db.Users.Add(user);
            _db.SaveChanges();

            LogHelper.LogToDb("RegisterCoach", $"註冊新教練帳號：{model.CoachAccount}");
            TempData["SuccessMessage"] = "註冊成功，請使用帳號密碼登入系統";
            return RedirectToAction("Login", "Auth");
        }
        #endregion

        #region 建立運動員帳號（GET）
        [HttpGet]
        public ActionResult RegisterAthlete()
        {
            return View(new AthleteRegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterAthlete(AthleteRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "請確認所有欄位格式皆正確";
                return View(model);
            }

            if (!SecurityHelper.IsValidPassword(model.Athletepwd))
            {
                TempData["ErrorMessage"] = "密碼格式錯誤（至少6碼，需含大小寫與數字）";
                return View(model);
            }

            var encryptedID = SecurityHelper.ComputeSha256(model.AthleteID.ToUpper());

            if (_db.Athletes.Any(a => a.AthleteAccount == model.AthleteID))
            {
                TempData["ErrorMessage"] = "此帳號已存在";
                return View(model);
            }

            if (_db.Athletes.Any(a => a.IdentityNumber == encryptedID))
            {
                TempData["ErrorMessage"] = "此身份證字號已註冊";
                return View(model);
            }

            var athlete = new Athletes
            {
                AthleteAccount = encryptedID,
                AthletePWD = SecurityHelper.ComputeSha256(model.Athletepwd),
                AthleteName = model.AthleteName,
                Birthday = model.AthleteBirthday.Value,
                IdentityNumber = encryptedID,
                AthleteSchool = model.AthleteSchool,
                AthleteOrganize = model.AthleteOrganize,
                TeamName = model.AthleteTeam,
                RegistrationDate = DateTime.Now,
                IsActive = true
            };
            _db.Athletes.Add(athlete);
            _db.SaveChanges();

            if (!model.NoCoach && model.CoachIDs != null)
            {
                foreach (var coachId in model.CoachIDs)
                {
                    _db.AthleteCoachRelations.Add(new AthleteCoachRelations
                    {
                        AthleteID = athlete.ID,
                        CoachID = coachId
                    });
                }
                _db.SaveChanges();
            }

            var user = new Users
            {
                Name = model.AthleteName,
                Account = encryptedID,
                Password = SecurityHelper.ComputeSha256(model.Athletepwd),
                RegistrationDate = DateTime.Now,
                IsActive = true,
                LastLoginDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                AthleteID = athlete.ID
            };
            _db.Users.Add(user);
            _db.SaveChanges();

            LogHelper.LogToDb("RegisterAthlete", $"註冊成功：帳號={model.AthleteID}");
            TempData["SuccessMessage"] = "註冊成功，請使用帳號登入";
            return RedirectToAction("Login", "Auth");
        }
        #endregion
    }
}