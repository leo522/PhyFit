using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PhysicalFit.Utility
{
    #region 密碼加密共用方法
    public class SecurityHelper
    {
        // 預設 Salt，可視需求改為從設定檔或資料庫取得
        private static readonly string DefaultSalt = "P@ssw0rd#@!";

        /// 使用 SHA256 + salt 進行雜湊加密
        public static string ComputeSha256(string rawData, string salt = null)
        {
            if (string.IsNullOrEmpty(rawData))
                return string.Empty;

            if (string.IsNullOrEmpty(salt))
                salt = DefaultSalt;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                string combined = rawData + salt;
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combined));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// 產生隨機密碼（預設8碼）
        public static string GenerateTemporaryPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// 驗證密碼格式（至少6碼，需包含大小寫與數字）
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            if (password.Length < 6) return false;

            // 至少一個大寫、小寫與數字
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$";
            return Regex.IsMatch(password, pattern);
        }
    }
    #endregion
}