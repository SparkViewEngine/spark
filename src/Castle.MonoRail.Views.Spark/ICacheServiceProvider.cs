
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Providers;
using Spark;

namespace Castle.MonoRail.Views.Spark
{
    public interface ICacheServiceProvider : IProvider
    {
        ICacheService GetCacheService(IEngineContext context);
    }
}
