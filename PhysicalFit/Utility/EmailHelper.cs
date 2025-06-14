using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;

namespace PhysicalFit.Utility
{
    #region 信箱共用方法
    public static class EmailHelper
    {
        private static readonly string EmailPattern =
            @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";

        /// 寄送 Email，可附加檔案
        public static void SendEmail(string toEmail, string subject, string body, string attachmentPath = null)
        {
            var fromEmail = "@tiss.org.tw";
            var fromPassword = ""; // 應從設定檔讀取
            var displayName = "運科中心資訊組";

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

            foreach (var email in toEmail.Split(','))
            {
                var trimmedEmail = email.Trim();
                if (IsValidEmail(trimmedEmail))
                {
                    mailMessage.To.Add(trimmedEmail);
                }
                else
                {
                    throw new FormatException($"無效的 Email 格式：{trimmedEmail}");
                }
            }

            if (!string.IsNullOrEmpty(attachmentPath))
            {
                mailMessage.Attachments.Add(new Attachment(attachmentPath));
            }

            smtpClient.Send(mailMessage);
        }

        /// 驗證單一 Email 格式
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, EmailPattern);
        }
    }
    #endregion
}