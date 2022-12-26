using BudgetHistory.Abstractions.Interfaces;
using BudgetHistory.Core.Models;
using System.Threading.Tasks;

namespace BudgetHistory.Business.Extensions
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
    }
}