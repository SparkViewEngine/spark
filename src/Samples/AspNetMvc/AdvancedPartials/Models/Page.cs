using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdvancedPartials.Models
{
    public class Page<TItem> : IEnumerable<TItem>
    {
        public IEnumerable<TItem> Items { get; set; }
        public int ItemCount { get; set; }

        public int CurrentPage { get; set; }
        public int PageCount { get; set; }

        public int FirstItemIndex { get; set; }

        public bool HasPreviousPage { get { return CurrentPage > 1; } }
        public bool HasNextPage { get { return CurrentPage < PageCount; } }


        public IEnumerator<TItem> GetEnumerator() { return Items.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
