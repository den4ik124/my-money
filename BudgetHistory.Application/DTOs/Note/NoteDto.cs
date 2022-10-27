using System;

namespace BudgetHistory.Application.DTOs.Note
{
    public class NoteDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; }
        public DateTime DateOfCreation { get; set; }
        public DateTime DateOfLastModification { get; set; }
        public decimal Balance { get; set; }
        public bool IsDeleted { get; set; }
        public string? Comment { get; set; }
    }
}