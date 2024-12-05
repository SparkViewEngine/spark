// Copyright 2008-2024 Louis DeJardin
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
using System.IO;

namespace Spark.FileSystem
{
    public class FileSystemViewFile : IViewFile
    {
        private readonly string _fullPath;

        public FileSystemViewFile(string fullPath)
        {
            _fullPath = fullPath;
        }

        public long LastModified
        {
            get { return File.GetLastWriteTimeUtc(_fullPath).Ticks; }
        }

        public Stream OpenViewStream()
        {
            return new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}