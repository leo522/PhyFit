using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PhysicalFit
{
    #region 身份證字號AES對稱加密
    public static class EncryptionHelper
    {
        private static readonly byte[] Key;
        private static readonly byte[] IV;
        private const string KeyFilePath = @"D:\key\key.txt"; // 儲存金鑰的文件
        private const string IvFilePath = @"D:\key\iv.txt";   // 儲存 IV 的文件

        static EncryptionHelper()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(KeyFilePath));

            if (File.Exists(KeyFilePath) && File.Exists(IvFilePath))
            {
                Key = Convert.FromBase64String(File.ReadAllText(KeyFilePath));
                IV = Convert.FromBase64String(File.ReadAllText(IvFilePath));
            }
            else
            {
                using (Aes aes = Aes.Create())
                {
                    aes.GenerateKey();
                    aes.GenerateIV();
                    Key = aes.Key;
                    IV = aes.IV;

                    File.WriteAllText(KeyFilePath, Convert.ToBase64String(Key));
                    File.WriteAllText(IvFilePath, Convert.ToBase64String(IV));
                }
            }
        }

        public static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
    #endregion
}