using System;
using System.Collections.Generic;

namespace Spark.Tests.Stubs
{
    public class StubCacheService : ICacheService
    {
        private readonly IDictionary<string, Entry> _cache = new Dictionary<string, Entry>();

        public StubCacheService()
        {
            UtcNow = new DateTime(2009, 1, 2, 3, 4, 5);
        }

        public object Get(string identifier)
        {
            Entry item;
            return _cache.TryGetValue(identifier, out item) && IsValid(item) ? item.Value : null;
        }

        private bool IsValid(Entry item)
        {
            return item.UtcExpires == CacheExpires.NoAbsoluteExpiration ||
                item.UtcExpires > UtcNow;
        }

        public void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item)
        {
            _cache[identifier] = new Entry {Value = item, UtcExpires = ToAbsolute(expires)};

            if (signal != null)
            {
                signal.Changed += (sender, e) => _cache.Remove(identifier);
            }
        }

        private DateTime ToAbsolute(CacheExpires expires)
        {
            // this is less sophisticated than the web caching implementation, but
            // it only needs to satisfy expectations of unit tests. they should always
            // use utc for abs

            if (expires == null)
                return CacheExpires.NoAbsoluteExpiration;

            if (expires.Sliding != CacheExpires.NoSlidingExpiration)
            {
                return UtcNow.Add(expires.Sliding);
            }

            return expires.Absolute;
        }

        public DateTime UtcNow { get; set; }
        public IEnumerable<string> AllKeys { get { return _cache.Keys; } }

        class Entry
        {
            public object Value { get; set; }
            public DateTime UtcExpires { get; set; }
        }
    }
}