using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetHistory.Core.Models
{
    public class Room
    {
        public Guid Id { get; set; }
        public IEnumerable<User> Users { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string Name { get; set; }

        [NotMapped]
        public string Password { get; set; }

        public string EncryptedPassword { get; set; }
        public IEnumerable<Note> Notes { get; set; }
    }
}