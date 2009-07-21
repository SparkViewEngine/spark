using System;
using System.Web.Caching;
using System.Web.Routing;

namespace Spark.Web.Mvc
{
    public class DefaultCacheServiceProvider : ICacheServiceProvider
    {
        public ICacheService GetCacheService(RequestContext context)
        {
            if (context.HttpContext != null && context.HttpContext.Cache != null)
                return new CacheService(context.HttpContext.Cache);
            return null;
        }

        class CacheService : ICacheService
        {
            private readonly Cache _cache;

            public CacheService(Cache cache)
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
                    expires.Absolute, 
                    expires.Sliding);
            }
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
