using System;
using System.ComponentModel.DataAnnotations;

namespace Notebook.Application.Core
{
    public class PageInfo
    {
        [Required]
        public int Page { get; set; } = 1;

        public long Items { get; set; }

        [Required]
        public int Size { get; set; } = 10;

        public int TotalPagesCount { get => (int)Math.Ceiling(decimal.Divide(this.Items, this.Size)); }
    }
}