using System;
using Castle.MonoRail.Framework;
using Spark;
using Spark.Caching;

namespace Castle.MonoRail.Views.Spark.Wrappers
{
    public class HybridCacheService : ICacheService
    {
        private readonly ICacheProvider _monorailCacheProvider;
        private readonly ICacheService _fallbackCacheService;

        public HybridCacheService(IEngineContext context)
        {
            if (context == null ||
                context.Services == null ||
                context.Services.CacheProvider == null)
            {
                _monorailCacheProvider = new NullCacheProvider();
            }
            else
            {
                _monorailCacheProvider = context.Services.CacheProvider;
            }


            if (context == null ||
                context.UnderlyingContext == null ||
                context.UnderlyingContext.Cache == null)
            {
                _fallbackCacheService = new NullCacheService();
            }
            else
            {
                _fallbackCacheService = new WebCacheService(context.UnderlyingContext.Cache);
            }
        }

        class NullCacheProvider : ICacheProvider
        {
            public void Service(IMonoRailServices serviceProvider) { }
            public bool HasKey(string key) { return false; }
            public object Get(string key) { return null; }
            public void Store(string key, object data) { }
            public void Delete(string key) { }
        }

        public object Get(string identifier)
        {
            // try the MR cache service, and fallback to the httpcontext cache
            return
                _monorailCacheProvider.Get(identifier) ??
                _fallbackCacheService.Get(identifier);
        }

        public void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item)
        {
            if (IsSimpleCaching(expires, signal))
            {
                // use MR cache service for simple identifier-only caching
                _monorailCacheProvider.Store(identifier, item);
            }
            else
            {
                // use the httpcontext cache when storing with expires or signal args in effect
                _fallbackCacheService.Store(identifier, expires, signal, item);
            }
        }


        private static bool IsSimpleCaching(CacheExpires expires, ICacheSignal signal)
        {
            // signal provided - complex caching
            if (signal != null)
                return false;

            // no signal, no expires - simple caching
            if (expires == null)
                return true;

            // expires absolute has meaningful value - complex caching
            if (expires.Absolute != CacheExpires.NoAbsoluteExpiration &&
                expires.Absolute != DateTime.MaxValue)
            {
                return false;
            }

            // expires sliding has meaningful value - complex caching
            if (expires.Sliding != CacheExpires.NoSlidingExpiration &&
                expires.Sliding != TimeSpan.MaxValue &&
                expires.Sliding != TimeSpan.Zero)
            {
                return false;
            }

            // no signal, no meaningful expires value - simple caching
            return true;
        }
    }
}