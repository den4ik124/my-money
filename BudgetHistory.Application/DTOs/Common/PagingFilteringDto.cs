using Notebook.Application.Core;
using System.ComponentModel.DataAnnotations;

namespace Notebook.Application.DTOs.Common
{
    public class PagingFilteringDto
    {
        [Required]
        public PageInfo PageInfo { get; set; }

        public DatesRangeDto TimePeriod { get; set; }
    }
}