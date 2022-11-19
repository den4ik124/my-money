using BudgetHistory.Core.Extensions;
using BudgetHistory.Core.Interfaces;
using BudgetHistory.Core.Interfaces.Repositories;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionDecryption _encryptionDecryptionService;
        private readonly IRoomService _roomService;

        public TestController(IUnitOfWork unitOfWork, IEncryptionDecryption encryptionDecryptionService, IRoomService roomService)
        {
            _unitOfWork = unitOfWork;
            _encryptionDecryptionService = encryptionDecryptionService;
            _roomService = roomService;
        }

        [HttpPost("recalculate-balances/{roomId}")]
        public async Task<IActionResult> RecalculateBalances(Guid roomId)
        {
            var noteRepos = _unitOfWork.GetGenericRepository<Note>();
            var roomRepos = _unitOfWork.GetGenericRepository<Room>();

            var room = (await _roomService.GetRoomById(roomId)).Value;

            var groups = noteRepos.GetQuery(note => !note.IsDeleted && note.RoomId == room.Id,
                                            order => order.OrderBy(note => note.DateOfCreation)).AsEnumerable().GroupBy(note => note.Currency);

            var joinedList = new List<Note>();

            foreach (var currencyGroup in groups)
            {
                var itemsCount = currencyGroup.Count();
                var notesInGroup = currencyGroup.ToArray();
                for (int i = 0; i < itemsCount; i++)
                {
                    await notesInGroup[i].DecryptValues(_encryptionDecryptionService, room.Password);
                    if (i == 0)
                    {
                        notesInGroup[i].Balance = notesInGroup[i].Value;
                    }
                    else
                    {
                        notesInGroup[i].Balance = notesInGroup[i - 1].Balance + notesInGroup[i].Value;
                    }
                    await notesInGroup[i].EncryptValues(_encryptionDecryptionService, room.Password);
                }

                joinedList.AddRange(notesInGroup);
            }

            await _unitOfWork.BeginTransactionAsync();

            foreach (var item in joinedList)
            {
                if (!noteRepos.Update(item))
                {
                    _unitOfWork.RollbackTransaction();
                    return BadRequest($"Проблемы при обновлении id: {item.Id}");
                }
            }
            _unitOfWork.TransactionCommit();
            await _unitOfWork.CompleteAsync();

            return Ok(joinedList.OrderBy(note => note.DateOfCreation));
        }
    }
}