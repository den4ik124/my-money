using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Models;
using System.Linq;

namespace BudgetHistory.Core.Extensions
{
    public static class RoomExtensions
    {
        public static Room EncryptValues(this Room room, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            room.EncryptedPassword = encryptionDecryptionService.Encrypt(room.Password, secretKey);
            return room;
        }

        public static Room DecryptValues(this Room room, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            room.Password = encryptionDecryptionService.Decrypt(room.EncryptedPassword, secretKey);
            return room;
        }

        public static bool IsUserAllowableToReadData<T>(this Room room, T userId) where T : class
            => room.Users.Where(user => user.Id.ToString() == userId.ToString()).Any();
    }
}