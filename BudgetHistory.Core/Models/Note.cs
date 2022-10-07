﻿using System;

namespace BudgetHistory.Core.Models
{
    public class Note : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public decimal Value { get; set; }
        public Currency Currency { get; set; }
        public DateTime DateOfCreation { get; private set; } = DateTime.UtcNow;
        public DateTime DateOfModification { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public decimal Balance { get; set; }
    }
}