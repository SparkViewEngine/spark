namespace JavascriptViewResultSample.Components.Paging
{
    public class Pager
    {
        public Pager(int? currentPage)
        {
            CurrentPage = currentPage ?? 1;
        }

        public Pager(int? currentPage, int perPage, int perCol)
            : this(currentPage)
        {
            PerPage = perPage;
            PerCol = perCol;
        }

        public Pager()
        {
        }

        public int PerPage { get; set; }
        public int PerCol { get; set; }
        internal int CurrentPage { get; set; }
        public int First
        {
            get { return (CurrentPage - 1)*PerPage; }
        }
    }
}