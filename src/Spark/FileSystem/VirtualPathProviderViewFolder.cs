using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace Spark.FileSystem
{
    public class VirtualPathProviderViewFolder : IViewFolder
    {
        private readonly string _virtualBaseDir;

        public VirtualPathProviderViewFolder(string virtualBaseDir)
        {
            _virtualBaseDir = virtualBaseDir;
        }

        public IViewFile GetViewSource(string path)
        {
            var file = HostingEnvironment.VirtualPathProvider.GetFile(Combine(path));
            return new VirtualPathFile(file);
        }

        public class VirtualPathFile : IViewFile
        {
            private readonly VirtualFile _file;

            public VirtualPathFile(VirtualFile file)
            {
                _file = file;
            }

            public long LastModified
            {
                get
                {
                    var hash = HostingEnvironment.VirtualPathProvider.GetFileHash(_file.VirtualPath, new string[0]);
                    return hash.GetHashCode();
                }
            }

            public Stream OpenViewStream()
            {
                return _file.Open();
            }
        }

        public IList<string> ListViews(string path)
        {
            var directory = HostingEnvironment.VirtualPathProvider.GetDirectory(Combine(path));
            return directory.Files.OfType<VirtualFile>().Select(f => f.VirtualPath).ToArray();
        }

        public bool HasView(string path)
        {
            return HostingEnvironment.VirtualPathProvider.FileExists(Combine(path));
        }

        private string Combine(string path)
        {
            return HostingEnvironment.VirtualPathProvider.CombineVirtualPaths(_virtualBaseDir, path);
            //return _virtualBaseDir.TrimEnd('/', '\\') + '/' + path.TrimStart('/', '\\');
        }
    }
}
