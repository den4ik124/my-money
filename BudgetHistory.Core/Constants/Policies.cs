﻿using System;

namespace BudgetHistory.Core.Constants
{
    [Flags]
    public enum Policies
    {
        AdminAccess = 1,
        ManagerAccess = 2,
        CustomerAccess = 4,
        GuestAccess = 8,
        RoomLoggedIn = 16,
    }
}