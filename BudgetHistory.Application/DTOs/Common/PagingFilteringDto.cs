using BudgetHistory.Application.Core;
using System.ComponentModel.DataAnnotations;

namespace BudgetHistory.Application.DTOs.Common
{
    public class PagingFilteringDto
    {
        [Required]
        public PageInfo PageInfo { get; set; }

        public DatesRangeDto TimePeriod { get; set; }
    }
}