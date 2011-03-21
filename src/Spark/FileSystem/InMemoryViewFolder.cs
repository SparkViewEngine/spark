// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Spark.FileSystem
{
    public class InMemoryViewFolder : Dictionary<string, byte[]>, IViewFolder
    {
        private static readonly IEqualityComparer<string> _pathComparer = new PathComparer(StringComparer.InvariantCultureIgnoreCase);

        public InMemoryViewFolder()
            : base(_pathComparer)
        {
        }

        private static byte[] GetBytes(string value)
        {
            using(var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(value);
                }
                return stream.ToArray();
            }
        }

        public void Add(string key, string value)
        {
            Add(key, GetBytes(value));
        }

        public void Set(string key, string value)
        {
            this[key] = GetBytes(value);
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

        private class PathComparer : IEqualityComparer<string>
        {
            private readonly StringComparer _baseComparer;

            public PathComparer(StringComparer baseComparer)
            {
                _baseComparer = baseComparer;
            }

            static string Adjust(string obj)
            {
                return obj == null ? null : obj.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            public bool Equals(string x, string y)
            {
                return _baseComparer.Equals(Adjust(x), Adjust(y));
            }

            public int GetHashCode(string obj)
            {
                return _baseComparer.GetHashCode(Adjust(obj));
            }
        }
    }
}
