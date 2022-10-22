using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Models;

namespace BudgetHistory.Core.Extensions
{
    public static class NoteExtensions
    {
        public static Note EncryptValues(this Note note, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            note.EncryptedValue = encryptionDecryptionService.Encrypt(note.Value, secretKey);
            note.EncryptedBalance = encryptionDecryptionService.Encrypt(note.Balance, secretKey);
            return note;
        }

        public static Note DecryptValues(this Note note, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            note.Value = encryptionDecryptionService.DecryptToDecimal(note.EncryptedValue, secretKey);
            note.Balance = encryptionDecryptionService.DecryptToDecimal(note.EncryptedBalance, secretKey);
            return note;
        }
    }
}