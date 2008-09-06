using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Spark.FileSystem
{
    public class InMemoryViewFolder : Dictionary<string, byte[]>, IViewFolder
    {
        public InMemoryViewFolder()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        public void Add(string key, string value)
        {
            Add(key, Encoding.Default.GetBytes(value));
        }

        public void Set(string key, string value)
        {
            this[key] = Encoding.Default.GetBytes(value);
        }

        public virtual IViewFile GetViewSource(string path)
        {
            if (!HasView(path))
                throw new FileNotFoundException(string.Format("Template {0} not found", path), path);

            return new InMemoryViewFile(this, path);
        }

        public virtual IList<string> ListViews(string path)
        {
            return
                Keys.Where(key => Comparer.Equals(path, Path.GetDirectoryName(key))).ToList();
        }

        public virtual bool HasView(string path)
        {
            return ContainsKey(path);
        }

        class InMemoryViewFile : IViewFile
        {
            private readonly InMemoryViewFolder parent;
            private readonly string path;

            public InMemoryViewFile(InMemoryViewFolder parent, string path)
            {
                this.parent = parent;
                this.path = path;
            }

            public long LastModified
            {
                get { return parent[path].GetHashCode(); }
            }

            public Stream OpenViewStream()
            {   
                return new MemoryStream(parent[path], false);
            }
        }
    }
}
