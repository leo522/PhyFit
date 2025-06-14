using PhysicalFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhysicalFit.Utility;

namespace PhysicalFit.Controllers
{
    public class ProfileController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        #region 教練個資編輯
        public ActionResult CoachEdit()
        {
            if (!User.Identity.IsAuthenticated || Session["UserRole"]?.ToString() != "Coach")
            {
                LogHelper.LogToDb("CoachEdit", "未授權使用者嘗試存取教練編輯頁", "WARN");
                return RedirectToAction("Login", "Auth");
            }

            string coachName = User.Identity.Name;
            var coach = _db.Coaches.FirstOrDefault(c => c.CoachName == coachName);

            if (coach == null)
            {
                LogHelper.LogToDb("CoachEdit", $"找不到教練資料：{coachName}", "ERROR");
                return RedirectToAction("Error404", "Error");
            }

            return View(coach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CoachEdit(Coaches model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "請確認表單格式正確";
                return View(model);
            }

            var coach = _db.Coaches.Find(model.ID);
            if (coach == null)
            {
                TempData["ErrorMessage"] = "找不到教練資料";
                return View(model);
            }

            coach.CoachName = model.CoachName;
            if (!EmailHelper.IsValidEmail(model.Email))
            {
                ViewBag.ErrorMessage = "信箱格式錯誤";
                return View(model);
            }
            coach.PhoneNumber = model.PhoneNumber;
            coach.TeamName = model.TeamName;
            coach.SchoolName = model.SchoolName;
            coach.Title = model.Title;
            coach.SportsSpecific = model.SportsSpecific;
            coach.LastUpdated = DateTime.Now;

            _db.SaveChanges();

            LogHelper.LogToDb("CoachEdit", $"教練 {coach.CoachName} 成功更新個資");
            TempData["SuccessMessage"] = "資料已成功更新";
            return RedirectToAction("Dashboard", "PhyFit");
        }
        #endregion

        #region 運動員個資編輯
        public ActionResult AthleteEdit()
        {
            if (!User.Identity.IsAuthenticated || Session["UserRole"]?.ToString() != "Athlete")
            {
                LogHelper.LogToDb("AthleteEdit", "未授權使用者存取運動員編輯頁", "WARN");
                return RedirectToAction("Login", "Auth");
            }

            string athleteName = User.Identity.Name;
            var athlete = _db.Athletes.FirstOrDefault(a => a.AthleteName == athleteName);

            if (athlete == null)
            {
                LogHelper.LogToDb("AthleteEdit", $"找不到運動員資料：{athleteName}", "ERROR");
                return RedirectToAction("Error404", "Error");
            }

            ViewBag.Coaches = GetCoachesBySchoolOrOrg(athlete);
            return View(athlete);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AthleteEdit(Athletes model, string NewPassword, string ConfirmPassword, List<int> CoachIDs, bool NoCoach = false)
        {
            var athlete = _db.Athletes.Find(model.ID);

            if (athlete == null)
            {
                TempData["ErrorMessage"] = "找不到運動員資料";
                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword != ConfirmPassword)
                {
                    TempData["ErrorMessage"] = "密碼不一致";
                    ViewBag.Coaches = GetCoachesBySchoolOrOrg(model);
                    return View(model);
                }

                if (!SecurityHelper.IsValidPassword(NewPassword))
                {
                    TempData["ErrorMessage"] = "密碼格式不符（至少6碼含大小寫與數字）";
                    ViewBag.Coaches = GetCoachesBySchoolOrOrg(model);
                    return View(model);
                }

                athlete.AthletePWD = SecurityHelper.ComputeSha256(NewPassword);
            }

            athlete.AthleteName = model.AthleteName;
            athlete.Birthday = model.Birthday;
            athlete.AthleteSchool = model.AthleteSchool;
            athlete.AthleteOrganize = model.AthleteOrganize;
            athlete.TeamName = model.TeamName;
            athlete.LastUpdated = DateTime.Now;

            var existingRelations = _db.AthleteCoachRelations.Where(r => r.AthleteID == athlete.ID);
            _db.AthleteCoachRelations.RemoveRange(existingRelations);

            if (!NoCoach && CoachIDs != null)
            {
                foreach (var coachId in CoachIDs)
                {
                    _db.AthleteCoachRelations.Add(new AthleteCoachRelations
                    {
                        AthleteID = athlete.ID,
                        CoachID = coachId
                    });
                }
            }

            _db.SaveChanges();
            LogHelper.LogToDb("AthleteEdit", $"運動員 {athlete.AthleteName} 成功更新個資");

            TempData["SuccessMessage"] = "資料已成功更新";
            return RedirectToAction("Dashboard", "PhyFit");
        }
        #endregion

        #region 教練選單載入邏輯
        private List<SelectListItem> GetCoachesBySchoolOrOrg(Athletes athlete)
        {
            var query = _db.Coaches.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(athlete.AthleteSchool))
            {
                query = query.Where(c => c.SchoolName == athlete.AthleteSchool);
            }
            else if (!string.IsNullOrEmpty(athlete.AthleteOrganize))
            {
                query = query.Where(c => c.Organize == athlete.AthleteOrganize);
            }

            return query.Select(c => new SelectListItem
            {
                Value = c.ID.ToString(),
                Text = c.CoachName
            }).ToList();
        }
        #endregion
    }
}