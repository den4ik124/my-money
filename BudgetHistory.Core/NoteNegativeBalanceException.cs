using System;

namespace BudgetHistory.Core
{
    public class NoteNegativeBalanceException : Exception
    {
        public NoteNegativeBalanceException()
        {
        }

        public NoteNegativeBalanceException(string message) : base(message)
        {
        }
    }
}