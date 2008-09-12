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

        public IViewFile GetViewSource(string path)
        {
            return _first.HasView(path) ? _first.GetViewSource(path) : _second.GetViewSource(path);
        }

        public IList<string> ListViews(string path)
        {
            return _first.ListViews(path).Union(_second.ListViews(path)).Distinct().ToArray();
        }

        public bool HasView(string path)
        {
            return _first.HasView(path) || _second.HasView(path);
        }
    }
}
