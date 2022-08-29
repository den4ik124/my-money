using Notebook.Core.Interfaces;
using System;

namespace Notebook.Core
{
    public class BaseEntity<T> : IEntity<T>
    {
        public T Id { get; set; }
        object IEntity.Id { get => this.Id; set => throw new NotImplementedException(); }
    }
}