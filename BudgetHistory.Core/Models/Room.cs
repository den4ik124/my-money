using System;
using System.Collections.Generic;

namespace BudgetHistory.Core.Models
{
    public class Room
    {
        public Guid Id { get; set; }
        public IEnumerable<User> Users { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}