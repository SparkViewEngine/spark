using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Spark.FileSystem
{
    public class EmbeddedViewFolder : InMemoryViewFolder
    {
        private readonly Assembly _assembly;
        private readonly string _resourcePath;

        public EmbeddedViewFolder(Assembly assembly, string resourcePath)
        {
            _assembly = assembly;
            _resourcePath = resourcePath;
            LoadAllResources(assembly, resourcePath);
        }

        public Assembly Assembly
        {
            get { return _assembly; }
        }

        private void LoadAllResources(Assembly assembly, string path)
        {
            foreach(var resourceName in assembly.GetManifestResourceNames().Where(name=>name.StartsWith(path + ".", StringComparison.InvariantCultureIgnoreCase)))
            {
                using(var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    var contents = new byte[stream.Length];
                    stream.Read(contents, 0, contents.Length);

                    var relativePath = resourceName.Substring(path.Length + 1);
                    relativePath = relativePath.Replace('.', '\\');
                    var lastDelimiter = relativePath.LastIndexOf('\\');
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
