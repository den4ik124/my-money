using System;
using System.Collections.Generic;

namespace BudgetHistory.Application.DTOs
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public ICollection<Guid> UserIds { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}