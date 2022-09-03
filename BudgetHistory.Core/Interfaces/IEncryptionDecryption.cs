namespace BudgetHistory.Core.Interfaces
{
    public interface IEncryptionDecryption
    {
        string Encrypt(string data, string secretKey);

        string Decrypt(string encryptedData, string secretKey);
    }
}