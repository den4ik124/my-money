using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetHistory.Core.Models
{
    public class Note : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }

        public Guid RoomId { get; set; }

        [NotMapped]
        public decimal Value { get; set; }

        public string EncryptedValue { get; set; }

        public Currency Currency { get; set; }

        public DateTime DateOfCreation { get; set; } = DateTime.UtcNow;

        public DateTime DateOfModification { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        [NotMapped]
        public decimal Balance { get; set; }

        public string EncryptedBalance { get; set; }
        public string Comment { get; set; } = String.Empty;
    }
}