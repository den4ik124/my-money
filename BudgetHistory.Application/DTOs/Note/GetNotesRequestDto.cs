using BudgetHistory.Application.Core;

namespace BudgetHistory.Application.DTOs.Note
{
    public class GetNotesRequestDto
    {
        public string RoomId { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}