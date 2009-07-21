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

            public void Store(string identifier, CacheExpires expires, object item)
            {
                _cache.Insert(identifier, item);
            }
        }
    }
}
