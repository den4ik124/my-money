using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Extensions
{
    public static class RoomExtensions
    {
        public static async Task<Room> EncryptValues(this Room room, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            room.EncryptedPassword = await encryptionDecryptionService.Encrypt(room.Password, secretKey);
            return room;
        }

        public static async Task<Room> DecryptValues(this Room room, IEncryptionDecryption encryptionDecryptionService, string secretKey)
        {
            room.Password = await encryptionDecryptionService.Decrypt(room.EncryptedPassword, secretKey);
            return room;
        }

        public static bool IsUserAllowableToReadData<T>(this Room room, T userId) where T : class
            => room.Users.Where(user => user.Id.ToString() == userId.ToString()).Any();
    }
}