using System;
using System.Collections.Generic;
using System.IO;

namespace Spark
{
    public class DefaultPartialReferenceProvider : IPartialReferenceProvider
    {
        private IPartialProvider _proxy;
        public DefaultPartialReferenceProvider()
        {
            _proxy = new DefaultPartialProvider();
        }
        public IEnumerable<string> GetPaths(string viewPath, bool allowCustomReferencePath)
        {
            return _proxy.GetPaths(viewPath);
        }
    }
}