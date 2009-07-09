using System.Collections.Generic;

namespace Spark.Tests.Stubs
{
    public class StubCacheService : ICacheService
    {
        private IDictionary<string, object> _cache = new Dictionary<string, object>();

        public object Get(string identifier)
        {
            object item;
            return _cache.TryGetValue(identifier, out item) ? item : null;
        }

        public void Store(string identifier, object item)
        {
            _cache[identifier] = item;
        }
    }
}