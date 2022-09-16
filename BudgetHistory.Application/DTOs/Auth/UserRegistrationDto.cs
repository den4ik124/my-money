using System.ComponentModel.DataAnnotations;

namespace BudgetHistory.Application.DTOs.Auth
{
    public class UserRegistrationDto
    {
        [MaxLength(50)]
        public string UserName { get; set; }
        
        [MaxLength(50)]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}