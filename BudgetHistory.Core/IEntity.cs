﻿namespace BudgetHistory.Core
{
    public interface IEntity
    {
        object Id { get; set; }
    }

    public interface IEntity<T> : IEntity
    {
        new T Id { get; set; }
    }
}