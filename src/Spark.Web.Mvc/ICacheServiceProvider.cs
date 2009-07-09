using System.Web.Routing;

namespace Spark.Web.Mvc
{
    public interface ICacheServiceProvider
    {
        ICacheService GetCacheService(RequestContext context);
    }
}