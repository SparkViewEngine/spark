using System;
using System.Collections.Generic;
using System.IO;

namespace Spark
{
    public class DefaultPartialProvider : IPartialProvider
    {
        public IEnumerable<string> GetPaths(string viewPath)
        {
            do
            {
                viewPath = Path.GetDirectoryName(viewPath);

                yield return viewPath;
                yield return Path.Combine(viewPath, Constants.Shared);
            }
            while (!String.IsNullOrEmpty(viewPath));
        }
    }
}