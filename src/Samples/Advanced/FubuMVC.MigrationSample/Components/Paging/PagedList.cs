using System;
using System.Collections.Generic;

namespace JavascriptViewResultSample.Components.Paging
{
    public class PagedList<T>
    {
        public PagedList() { }
        public PagedList(Pager pager, List<T> data, int totalRecords)
        {
            PagedInfo = new PagedInfo(pager, totalRecords);
            Data = data;
        }

        public PagedInfo PagedInfo { get; private set; }
        public List<T> Data { get; private set; }
        public List<Column<T>> Columns
        {
            get
            {
                var columns = new List<Column<T>>();
                for (var i = 0; i < PagedInfo.RecordsPerPage; i += PagedInfo.RecordsPerCol)
                {
                    var column = new Column<T>{Data = new List<T>()};
                    for (var j = 0; j < PagedInfo.RecordsPerCol; j++)
                    {
                        if (Data.Count - 1  < i + j) continue;

                        var item = Data[i + j];
                        column.Data.Add(item);
                    }
                    columns.Add(column);
                }
                return columns;
            }
        }

        public bool HasRecords
        {
            get { return PagedInfo.TotalRecords > 0; }
        }
    }

    public class Column<T>
    {
        public List<T> Data { get; set; }
    }

    public class PagedInfo
    {
        public PagedInfo() { }
        public PagedInfo(Pager pager, int totalRecords)
        {
            TotalRecords = totalRecords;
            CurrentPage = pager.CurrentPage;
            RecordsPerPage = pager.PerPage;
            RecordsPerCol = pager.PerCol;
        }

        public int RecordsPerPage { get; private set; }
        public int RecordsPerCol { get; private set; }
        public int CurrentPage { get; private set; }
        public int TotalRecords { get; private set; }

        public int ColsPerPage
        {
            get { return RecordsPerCol == 0 ? 0 : RecordsPerPage / RecordsPerCol; }
        }

        public int TotalPages
        {
            get
            {
                if (TotalRecords == 0 || RecordsPerPage == 0) return 0;
                return (int)Math.Ceiling((float)TotalRecords / RecordsPerPage);
            }
        }
    }
}