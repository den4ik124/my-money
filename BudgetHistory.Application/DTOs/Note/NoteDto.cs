using BudgetHistory.Core.Models;
using System;

namespace BudgetHistory.Application.DTOs.Note
{
    public class NoteDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public decimal Value { get; set; }
        public Currency Currency { get; set; }
        public DateTime DateOfCreation { get; set; }
        public decimal Balance { get; set; }
    }
}