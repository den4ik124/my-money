using System;

namespace BudgetHistory.Application.DTOs.Room
{
    public class RoomResponseDto
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string Name { get; set; }
    }
}