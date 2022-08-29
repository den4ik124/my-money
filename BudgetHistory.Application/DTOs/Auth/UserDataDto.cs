using System.Collections.Generic;

namespace Notebook.Application.DTOs.Auth
{
    public class UserDataDto
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public IList<string> Roles { get; set; }
    }
}