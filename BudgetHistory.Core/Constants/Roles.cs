using System;

namespace Notebook.Core.Constants
{
    [Flags]
    public enum Roles
    {
        SuperAdmin = 0,
        Admin = 1,
        Manager = 2,
        Customer = 4,
        Guest = 8
    }
}