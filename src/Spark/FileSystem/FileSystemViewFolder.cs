// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spark.FileSystem
{
    public class FileSystemViewFolder : IViewFolder
    {
        private readonly string _basePath;

        public FileSystemViewFolder(string basePath)
        {
            _basePath = basePath;
        }

        public string BasePath
        {
            get { return _basePath; }
        }


        public IViewFile GetViewSource(string path)
        {
            string fullPath = Path.Combine(_basePath, path);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("View source file not found.", fullPath);

            return new FileSystemViewFile(fullPath);
        }

        public IList<string> ListViews(string path)
        {
            if (!Directory.Exists(Path.Combine(_basePath, path)))
                return new string[0];

            var files = Directory.GetFiles(Path.Combine(_basePath, path));
            return files.ToList().ConvertAll(viewPath => Path.GetFileName(viewPath));
        }

        public bool HasView(string path)
        {
            return File.Exists(Path.Combine(_basePath, path));
        }
    }
}
