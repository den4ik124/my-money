using BudgetHistory.Abstractions.Interfaces;
using BudgetHistory.Core.Models;
using System.Threading.Tasks;

namespace BudgetHistory.Business.Extensions
{
    public static class NoteExtensions
    {
        public static async Task<Note> EncryptValues(this Note note, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            note.EncryptedValue = await encryptionDecryptionService.Encrypt(note.Value, secretKey);
            note.EncryptedBalance = await encryptionDecryptionService.Encrypt(note.Balance, secretKey);
            return note;
        }

        public static async Task<Note> DecryptValues(this Note note, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            note.Value = await encryptionDecryptionService.DecryptToDecimal(note.EncryptedValue, secretKey);
            note.Balance = await encryptionDecryptionService.DecryptToDecimal(note.EncryptedBalance, secretKey);
            return note;
        }
    }
}