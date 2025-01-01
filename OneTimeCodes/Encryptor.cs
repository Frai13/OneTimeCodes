using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OneTimeCodes
{
    internal static class Encryptor
    {
        internal static bool Encrypt(byte[] key, byte[] iv, byte[] salt, string filePath)
        {
            if (!File.Exists(filePath)) return false;

            string inputPath = filePath;
            string dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string outputPath = $"{filePath}_{dateStr}";

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (FileStream fsOutput = new FileStream(outputPath, FileMode.Create))
                    {
                        fsOutput.Write(salt, 0, salt.Length);

                        using (CryptoStream cs = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        using (FileStream fsInput = new FileStream(inputPath, FileMode.Open))
                        {
                            fsInput.CopyTo(cs);
                        }
                    }
                }

                File.Delete(inputPath);
                File.Move(outputPath, inputPath);
            }
            catch (Exception)
            {
                File.Delete(outputPath);
                throw;
            }

            return true;
        }

        internal static string Decrypt(byte[] key, byte[] iv, string filePath)
        {
            if (!File.Exists(filePath)) return "";

            using (FileStream fsInput = new FileStream(filePath, FileMode.Open))
            {
                byte[] salt = new byte[16];
                fsInput.Read(salt, 0, salt.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (CryptoStream cs = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        cs.CopyTo(ms);
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
