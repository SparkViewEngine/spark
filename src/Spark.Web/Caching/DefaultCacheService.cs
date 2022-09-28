using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace Spark.Caching
{
    public class DefaultCacheService : ICacheService
    {
        private readonly Cache _cache;

        public DefaultCacheService(Cache cache)
        {
            _cache = cache;
        }

        public object Get(string identifier)
        {
            return _cache.Get(identifier);
        }

        public void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item)
        {
            _cache.Insert(
                identifier,
                item,
                SignalDependency.For(signal),
                (expires ?? CacheExpires.Empty).Absolute,
                (expires ?? CacheExpires.Empty).Sliding);
        }

        class SignalDependency : CacheDependency
        {
            private readonly ICacheSignal _signal;

            SignalDependency(ICacheSignal signal)
            {
                _signal = signal;
                _signal.Changed += SignalChanged;
            }

            ~SignalDependency()
            {
                _signal.Changed -= SignalChanged;
            }

            public static CacheDependency For(ICacheSignal signal)
            {
                return signal == null ? null : new SignalDependency(signal);
            }

            void SignalChanged(object sender, EventArgs e)
            {
                NotifyDependencyChanged(this, e);
            }

            protected override void DependencyDispose()
            {
                _signal.Changed -= SignalChanged;
            }
        }

    }

}
