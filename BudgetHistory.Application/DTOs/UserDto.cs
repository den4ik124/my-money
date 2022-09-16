using BudgetHistory.Application.DTOs.Room;
using System;
using System.Collections.Generic;

namespace BudgetHistory.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public Guid AssociatedIdentityUserId { get; set; }
        public IEnumerable<RoomDto> Rooms { get; set; }
    }
}