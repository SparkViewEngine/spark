using System;
using System.Collections.Generic;

namespace Spark
{
    public class DefaultPartialReferenceProvider : IPartialReferenceProvider
    {
        private readonly Func<IPartialProvider> _getProvider = () => null;

        public DefaultPartialReferenceProvider(IPartialProvider partialProvider)
            : this(() => partialProvider)
        {
        }

        public DefaultPartialReferenceProvider(Func<IPartialProvider> getProvider)
        {
            _getProvider = getProvider;
        }

        public IEnumerable<string> GetPaths(string viewPath, bool allowCustomReferencePath)
        {
            var provider = _getProvider() ?? new DefaultPartialProvider();
            return provider.GetPaths(viewPath);
        }
    }
}