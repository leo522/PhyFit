using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PhysicalFit.Models;

namespace PhysicalFit
{
    public class LogHelper
    {
        public static void LogToDb(string action, string message, string level = "INFO", Exception ex = null)
        {
            try
            {
                using (var db = new PhFitnessEntities())
                {
                    // 建立一筆新的 log 紀錄
                    var log = new SystemLogs
                    {
                        // 從 Session 中取出 UserID，如果不存在則為 null
                        UserId = HttpContext.Current.Session["UserID"] != null ? (int)HttpContext.Current.Session["UserID"] : (int?)null,

                        // 從目前登入的使用者中取得名稱（帳號）
                        UserName = HttpContext.Current.User?.Identity?.Name,

                        // 動作名稱，如 "Login", "Register"
                        Action = action,

                        // Log 等級，如 INFO、WARN、ERROR
                        LogLevel = level,

                        // 記錄內容描述
                        Message = message,

                        // 例外錯誤內容（如果有的話）
                        Exception = ex?.ToString(),

                        // 紀錄使用者的 IP 位址
                        IPAddress = HttpContext.Current.Request.UserHostAddress,

                        // 記錄時間
                        CreatedAt = DateTime.Now
                    };

                    // 加入資料庫並儲存
                    db.SystemLogs.Add(log);
                    db.SaveChanges();
                }
            }
            catch (Exception logEx)
            {
                // 如果連 Log 自己都失敗，寫到除錯視窗
                System.Diagnostics.Debug.WriteLine("Log 寫入失敗：" + logEx.Message);
            }
        }
    }
}