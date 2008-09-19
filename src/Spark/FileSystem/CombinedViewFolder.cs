using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.FileSystem
{
    public class CombinedViewFolder : IViewFolder
    {
        private readonly IViewFolder _first;
        private readonly IViewFolder _second;

        public CombinedViewFolder(IViewFolder first, IViewFolder second)
        {
            _first = first;
            _second = second;
        }

        public IViewFolder First
        {
            get { return _first; }
        }

        public IViewFolder Second
        {
            get { return _second; }
        }

        public IViewFile GetViewSource(string path)
        {
            return First.HasView(path) ? First.GetViewSource(path) : Second.GetViewSource(path);
        }

        public IList<string> ListViews(string path)
        {
            return First.ListViews(path).Union(Second.ListViews(path)).Distinct().ToArray();
        }

        public bool HasView(string path)
        {
            return First.HasView(path) || Second.HasView(path);
        }
    }
}
