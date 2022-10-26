using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BudgetHistory.API.Controllers
{
    [Authorize]
    public class TestController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IEncryptionDecryption encryptionDecryptionService;
        private readonly IConfiguration configuration;

        public TestController(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryptionService, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.encryptionDecryptionService = encryptionDecryptionService;
            this.configuration = configuration;
        }

        [HttpPost("recalculate-balances/{roomId}")]
        public async Task<IActionResult> RecalculateBalances(Guid roomId)
        {
            var noteRepos = unitOfWork.GetGenericRepository<Note>();
            var roomRepos = unitOfWork.GetGenericRepository<Room>();

            var room = await roomRepos.GetById(roomId);
            var roomPassword = encryptionDecryptionService.Decrypt(room.Password, configuration.GetSection(AppSettings.SecretKey).Value);

            var groups = noteRepos.GetQuery(note => !note.IsDeleted && note.RoomId == room.Id).AsEnumerable().GroupBy(note => note.Currency);

            var joinedList = new List<Note>();

            foreach (var currencyGroup in groups)
            {
                var itemsCount = currencyGroup.Count();
                var notesInGroup = currencyGroup.ToArray();
                for (int i = 0; i < itemsCount; i++)
                {
                    notesInGroup[i].DecryptValues(encryptionDecryptionService, roomPassword);
                    if (i == 0)
                    {
                        notesInGroup[i].Balance = notesInGroup[i].Value;
                    }
                    else
                    {
                        notesInGroup[i].Balance = notesInGroup[i - 1].Balance + notesInGroup[i].Value;
                    }
                    notesInGroup[i].EncryptValues(encryptionDecryptionService, roomPassword);
                }

                joinedList.AddRange(notesInGroup);
            }

            await unitOfWork.BeginTransactionAsync();

            foreach (var item in joinedList)
            {
                if (!noteRepos.Update(item))
                {
                    unitOfWork.RollbackTransaction();
                    return BadRequest($"Проблемы при обновлении id: {item.Id}");
                }
            }
            unitOfWork.TransactionCommit();
            await unitOfWork.CompleteAsync();

            return Ok(joinedList.OrderBy(note => note.DateOfCreation));
        }
    }
}