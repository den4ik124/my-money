using System.Collections.Generic;

namespace Notebook.Application.Core
{
    public class PagedList<T>
    {
        public PagedList(IEnumerable<T> items, PageInfo pageInfo)
        {
            Items = items;
            PageInfo = pageInfo;
        }

        public IEnumerable<T> Items { get; set; }

        public PageInfo PageInfo { get; }
    }
}