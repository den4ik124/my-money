using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BudgetHistory.Application.DTOs
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public IEnumerable<UserDto> Users { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime DateOfCreation { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }
    }
}