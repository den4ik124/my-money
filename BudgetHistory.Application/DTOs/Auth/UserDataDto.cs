using System.Collections.Generic;

namespace BudgetHistory.Application.DTOs.Auth
{
    public class UserDataDto
    {
        public string Id { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }

        public IList<string> Roles { get; set; }
    }
}