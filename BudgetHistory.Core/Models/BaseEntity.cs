using System;

namespace BudgetHistory.Core.Models
{
    public class BaseEntity<T> : IEntity<T>
    {
        public T Id { get; set; }
        object IEntity.Id { get => this.Id; set => throw new NotImplementedException(); }
    }
}