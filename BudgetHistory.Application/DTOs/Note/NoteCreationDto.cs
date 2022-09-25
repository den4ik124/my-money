using System;

namespace BudgetHistory.Application.DTOs.Note
{
    public class NoteCreationDto
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; }
    }
}