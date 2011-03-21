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
            _virtualBaseDir = virtualBaseDir.TrimEnd(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar) + "/";
        }

        public string VirtualBaseDir
        {
            get { return _virtualBaseDir; }
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
                    var hash = HostingEnvironment.VirtualPathProvider.GetFileHash(_file.VirtualPath, new[] { _file.VirtualPath });
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
            if (string.IsNullOrEmpty(path))
                return VirtualBaseDir;

            return HostingEnvironment.VirtualPathProvider.CombineVirtualPaths(VirtualBaseDir, path);
        }
    }
	
}
