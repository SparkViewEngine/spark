using Spark.FileSystem;
using System.Collections.Generic;
using System.IO;

namespace SparkSense.Parsing
{
    public class CachingViewFolder : IViewFolder
    {
        private FileSystemViewFolder _disk;
        private InMemoryViewFolder _cache;

        public CachingViewFolder(string basePath)
        {
            _cache = new InMemoryViewFolder();
            _disk = new FileSystemViewFolder(basePath);
        }
        public IViewFile GetViewSource(string path)
        {
            if (!_cache.HasView(path) || _cache[path].Length == 0)
            {
                LoadFromDisk(path);
            }
            return _cache.GetViewSource(path);
        }

        public void SetViewSource(string path, string content)
        {
            _cache.Set(path, content);
        }

        public IList<string> ListViews(string path)
        {
            return _disk.ListViews(path);
        }

        public bool HasView(string path)
        {
            return _cache.HasView(path) || _disk.HasView(path);
        }

        public void Add(string path)
        {
            if(!_cache.ContainsKey(path))
                _cache.Add(path, null);
        }

        private void LoadFromDisk(string path)
        {
            var fileContents = _disk.GetViewSource(path);
            string contents;
            using (TextReader reader = new StreamReader(fileContents.OpenViewStream()))
                contents = reader.ReadToEnd();
            _cache.Set(path, contents);
        }
    }
}
