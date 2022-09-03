using System;

namespace BudgetHistory.Application.DTOs.Common
{
    public class DatesRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } = DateTime.Now;
    }
}