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
        readonly HierarchyItem _root;

        public VsProjectViewFolder(ISparkSource source, IVsHierarchy hierarchy)
        {
            _source = source;
            _hierarchy = hierarchy;

            _root = new HierarchyItem(_hierarchy, 0xfffffffe);
            var child = _root.FirstChild;
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

        private HierarchyItem FindPath(string path)
        {
            if (path.StartsWith("$\\"))
            {
                return _root.FindPath(path.Substring(2));
            }
            return _views.FindPath(path);
        }


        #region IViewFolder Members

        public IViewFile GetViewSource(string path)
        {
            var item = FindPath(path);
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

        public IList<string> ListViews(string path)
        {
            var views = new List<string>();

            var item = FindPath(path);
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
            var item = FindPath(path);
            return item != null;
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
        #endregion
    }
}