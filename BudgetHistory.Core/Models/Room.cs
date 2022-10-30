using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetHistory.Core.Models
{
    public class Room : BaseEntity<Guid>
    {
        public IEnumerable<User> Users { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime DateOfCreation { get; set; } = DateTime.UtcNow;
        public string Name { get; set; }

        [NotMapped]
        public string Password { get; set; }

        public string EncryptedPassword { get; set; }
        public IEnumerable<Note> Notes { get; set; }
    }
}