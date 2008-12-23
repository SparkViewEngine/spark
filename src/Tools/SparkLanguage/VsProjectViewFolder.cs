using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Spark.FileSystem;
using SparkLanguage.VsAdapters;
using SparkLanguagePackageLib;
using IServiceProvider = SparkLanguagePackageLib.IServiceProvider;

namespace SparkLanguage
{
    public class VsProjectViewFolder : IViewFolder
    {
        readonly ISparkSource _source;
        readonly IVsHierarchy _hierarchy;
        readonly HierarchyItem _views;

        public VsProjectViewFolder(ISparkSource source, IVsHierarchy hierarchy)
        {
            _source = source;
            _hierarchy = hierarchy;

            int root = -2;
            var rootItem = new HierarchyItem(_hierarchy, (uint)root);
            var child = rootItem.FirstChild;
            while (child != null)
            {
                if (string.Equals(child.Name, "Views", StringComparison.InvariantCultureIgnoreCase))
                {
                    _views = child;
                    break;
                }
                child = child.NextSibling;
            }
        }

        #region IViewFolder Members

        public IViewFile GetViewSource(string path)
        {
            var item = _views.FindPath(path);
            if (item == null)
                return null;

            var canonicalName = item.CanonicalName;
            var openFileText = _source.GetRunningDocumentText(canonicalName);
            if (openFileText != null)
            {
                return new OpenFile(openFileText);
            }

            return new FileSystemViewFile(item.CanonicalName);
        }

        public class OpenFile : IViewFile
        {
            private readonly string _text;

            public OpenFile(string text)
            {
                _text = text;
            }

            public long LastModified
            {
                get { return _text.GetHashCode(); }
            }

            public Stream OpenViewStream()
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(_text));
            }
        }

        public IList<string> ListViews(string path)
        {
            var views = new List<string>();

            var item = _views.FindPath(path);
            if (item != null)
            {
                for (var child = item.FirstChild; child != null; child = child.NextSibling)
                {
                    if (child.Name.EndsWith(".spark", StringComparison.InvariantCultureIgnoreCase))
                        views.Add(child.Name);
                }
            }
            return views;
        }

        public bool HasView(string path)
        {
            var item = _views.FindPath(path);
            return item != null;
        }

        #endregion
    }
}