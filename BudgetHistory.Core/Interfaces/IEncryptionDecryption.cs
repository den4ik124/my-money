using System.Threading.Tasks;

namespace BudgetHistory.Core.Interfaces
{
    public interface IEncryptionDecryption
    {
        Task<string> Encrypt<T>(T data, string secretKey);

        Task<string> Decrypt(string encryptedData, string secretKey);

        Task<decimal> DecryptToDecimal(string encryptedDecimal, string secretKey);
    }
}