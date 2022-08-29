using System;

namespace Notebook.Core
{
    public class Room
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateOfCreation { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}