using BudgetHistory.Core.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BudgetHistory.Core.Services
{
    public class EncryptionDecryptionService : IEncryptionDecryption
    {
        public string Encrypt(string data, string secretKey)
        {
            byte[] keyBytes = CheckPasswordLength(secretKey);

            byte[] iv = new byte[16];
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.WriteLine(data);
                        }
                    }
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
        }

        public string Decrypt(string encryptedData, string secretKey)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(encryptedData);

            var keyBytes = CheckPasswordLength(secretKey);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd().Replace("\r\n", String.Empty);
        }

        private static byte[] CheckPasswordLength(string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            if (keyBytes.Length > 32)
            {
                throw new Exception("Слишком длинный пароль!");
            }

            var keyBytes32 = new byte[32];
            for (int i = 0; i < keyBytes.Length; i++)
            {
                keyBytes32[i] = keyBytes[i];
            }

            return keyBytes32;
        }
    }
}