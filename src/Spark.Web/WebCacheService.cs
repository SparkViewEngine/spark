using System;
using System.Web.Caching;

namespace Spark
{
    public class WebCacheService : ICacheService
    {
        private readonly Cache cache;

        public WebCacheService(Cache cache)
        {
            this.cache = cache;
        }

        public object Get(string identifier)
        {
            return this.cache.Get(identifier);
        }

        public void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item)
        {
            this.cache.Insert(
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
