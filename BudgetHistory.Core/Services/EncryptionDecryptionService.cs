using BudgetHistory.Core.Interfaces;
using BudgetHistory.Logging;
using BudgetHistory.Logging.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class EncryptionDecryptionService : IEncryptionDecryption
    {
        private readonly CustomLogger _log;

        public EncryptionDecryptionService(ICustomLoggerFactory logFactory)
        {
            _log = logFactory.CreateLogger<EncryptionDecryptionService>();
        }

        public async Task<string> Encrypt<T>(T data, string secretKey)
        {
            byte[] keyBytes = await CheckPasswordLength(secretKey);

            byte[] iv = new byte[16];

            using Aes aes = Aes.Create();

            aes.Key = keyBytes;
            aes.IV = iv;
            using var memoryStream = new MemoryStream();
            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
            {
                using var streamWriter = new StreamWriter(cryptoStream);
                streamWriter.WriteLine(data.ToString());
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public async Task<string> Decrypt(string encryptedData, string secretKey)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(encryptedData);

            var keyBytes = await CheckPasswordLength(secretKey);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd().Replace("\r\n", string.Empty);
        }

        public async Task<decimal> DecryptToDecimal(string encryptedDecimal, string secretKey)
        {
            return decimal.Parse(await Decrypt(encryptedDecimal, secretKey));
        }

        private async Task<byte[]> CheckPasswordLength(string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            if (keyBytes.Length > 32)
            {
                var errorMessage = $"{nameof(EncryptionDecryptionService)}:\nСлишком длинный пароль!";
                await _log.LogError(errorMessage);
                throw new Exception(errorMessage);
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