using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark.Web.Mvc.Wrappers
{
    public class ViewFolderWrapper : IViewFolder
    {
        private readonly IViewFolderContainer _container;

        public ViewFolderWrapper(IViewFolderContainer container)
        {
            _container = container;
        }

        public IViewFile GetViewSource(string path)
        {
            return _container.ViewFolder.GetViewSource(path);
        }

        public IList<string> ListViews(string path)
        {
            return _container.ViewFolder.ListViews(path);
        }

        public bool HasView(string path)
        {
            return _container.ViewFolder.HasView(path);
        }
    }
}
