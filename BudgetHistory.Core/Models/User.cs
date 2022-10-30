using System;
using System.Collections.Generic;

namespace BudgetHistory.Core.Models
{
    public class User : BaseEntity<Guid>
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public Guid AssociatedIdentityUserId { get; set; }
        public IEnumerable<Room> Rooms { get; set; }
    }
}