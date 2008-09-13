using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Castle.MonoRail.Views.Spark.Wrappers
{
    public class ViewSourceLoaderWrapper : IViewFolder
    {
        private readonly IViewSourceLoaderContainer _container;

        public ViewSourceLoaderWrapper(IViewSourceLoaderContainer container)
        {
            _container = container;
        }

        public IViewFile GetViewSource(string path)
        {
            var viewSource = _container.ViewSourceLoader.GetViewSource(Path.ChangeExtension(path, ".spark"));
            return new ViewSourceWrapper(viewSource);
        }

        public IList<string> ListViews(string path)
        {
            return _container.ViewSourceLoader.ListViews(path);
        }

        public bool HasView(string path)
        {
            return _container.ViewSourceLoader.HasSource(path);
        }
    }
}
