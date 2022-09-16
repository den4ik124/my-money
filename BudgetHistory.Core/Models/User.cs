using System;
using System.Collections.Generic;

namespace BudgetHistory.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public Guid AssociatedIdentityUserId { get; set; }
        public IEnumerable<Room> Rooms { get; set; }
    }
}