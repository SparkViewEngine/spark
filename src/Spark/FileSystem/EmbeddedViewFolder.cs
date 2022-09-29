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
using System.Reflection;

namespace Spark.FileSystem
{
    public class EmbeddedViewFolder : InMemoryViewFolder
    {
        private readonly Assembly _assembly;
        
        public EmbeddedViewFolder(Assembly assembly, string resourcePath)
        {
            _assembly = assembly;
            LoadAllResources(assembly, resourcePath);
        }

        public Assembly Assembly => _assembly;

        private void LoadAllResources(Assembly assembly, string path)
        {
            foreach(var resourceName in assembly.GetManifestResourceNames().Where(name=>name.StartsWith(path + ".", StringComparison.InvariantCultureIgnoreCase)))
            {
                using(var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    var contents = new byte[stream.Length];
                    stream.Read(contents, 0, contents.Length);

                    var relativePath = resourceName.Substring(path.Length + 1);
                    relativePath = relativePath.Replace('.', Path.DirectorySeparatorChar);
                    var lastDelimiter = relativePath.LastIndexOf(Path.DirectorySeparatorChar);
                    if (lastDelimiter >= 0)
                    {
                        relativePath = relativePath.Substring(0, lastDelimiter) + "." +
                                       relativePath.Substring(lastDelimiter + 1);
                    }
                    Add(relativePath, contents);
                }
            }
        }
    }
}
