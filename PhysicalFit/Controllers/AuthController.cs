using PhysicalFit.Models;
using PhysicalFit.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PhysicalFit.Controllers
{
    public class AuthController : Controller
    {
        private PhFitnessEntities _db = new PhFitnessEntities();

        #region 登入
        [HttpGet]
        public ActionResult Login() => View();

        [HttpPost]
        public ActionResult Login(string account, string pwd)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(pwd))
                {
                    ViewBag.ErrorMessage = "請輸入帳號與密碼";
                    return View();
                }

                var hashedPwd = SecurityHelper.ComputeSha256(pwd);
                var hashedAccount = SecurityHelper.ComputeSha256(account.ToUpper());

                var user = _db.Users.FirstOrDefault(u => u.Account == hashedAccount && u.Password == hashedPwd)
                        ?? _db.Users.FirstOrDefault(u => u.Account == account && u.Password == hashedPwd);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "帳號或密碼錯誤";
                    LogHelper.LogToDb("Login", $"登入失敗，帳號或密碼錯誤：{account}", "WARN");
                    return View();
                }

                if (!(user.IsActive ?? false))
                {
                    ViewBag.ErrorMessage = "此帳號已被停用";
                    LogHelper.LogToDb("Login", $"登入失敗，帳號停用：{account}", "WARN");
                    return View();
                }

                if (user.IsTemporaryPassword ?? false)
                {
                    LogHelper.LogToDb("Login", $"臨時密碼登入：{account}");
                    return RedirectToAction("ResetPwd", "Account", new { userId = user.UID });
                }

                // 成功登入
                user.LastLoginDate = DateTime.Now;
                _db.SaveChanges();

                Session["UserID"] = user.UID;
                Session["UserRole"] = user.CoachID.HasValue ? "Coach" : "Athlete";

                var authTicket = new FormsAuthenticationTicket(
                    1,
                    user.Name,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    user.CoachID?.ToString() ?? user.AthleteID?.ToString(),
                    FormsAuthentication.FormsCookiePath);

                string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                {
                    HttpOnly = true
                };
                Response.Cookies.Add(authCookie);

                LogHelper.LogToDb("Login", $"登入成功：{account}");
                return RedirectToAction("Dashboard", "PhyFit");
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = ex.InnerException?.Message ?? ex.Message;
                return RedirectToAction("Error404", "Error");
            }
        }
        #endregion

        #region 登出
        public ActionResult Logout()
        {
            LogHelper.LogToDb("Logout", $"使用者 {User.Identity.Name} 登出");
            Session.Clear();
            FormsAuthentication.SignOut();

            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return RedirectToAction("Login");
        }
        #endregion

        #region 延長登入時效
        [HttpGet]
        public ActionResult KeepAlive() => new HttpStatusCodeResult(200);

        public JsonResult GetUserRole()
        {
            var userRole = Session["UserRole"]?.ToString();
            return Json(new { userRole }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}