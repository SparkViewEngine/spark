using System;
using System.Web.Caching;
using System.Web.Routing;
using Spark.Caching;

namespace Spark.Web.Mvc
{
    public class DefaultCacheServiceProvider : ICacheServiceProvider
    {
        public ICacheService GetCacheService(RequestContext context)
        {
            if (context.HttpContext != null && context.HttpContext.Cache != null)
                return new DefaultCacheService(context.HttpContext.Cache);
            return null;
        }
    }
}
