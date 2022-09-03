using System.ComponentModel.DataAnnotations;

namespace BudgetHistory.Application.DTOs.Auth
{
    public class UserLoginDto
    {
        [MaxLength(20)]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}