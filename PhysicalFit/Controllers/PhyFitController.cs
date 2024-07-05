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

        #region 首頁
        public ActionResult Home()
        {
            Session["ReturnUrl"] = Request.Url.ToString();

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
    }
}