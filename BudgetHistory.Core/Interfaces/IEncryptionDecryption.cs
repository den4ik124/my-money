namespace BudgetHistory.Core.Interfaces
{
    public interface IEncryptionDecryption
    {
        string Encrypt<T>(T data, string secretKey);

        string Decrypt(string encryptedData, string secretKey);

        public decimal DecryptToDecimal(string encryptedDecimal, string secretKey);
    }
}