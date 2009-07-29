using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Spark;

namespace Castle.MonoRail.Views.Spark.Wrappers
{
    public class HybridCacheServiceProvider : ICacheServiceProvider
    {
        public void Service(IMonoRailServices serviceProvider)
        {
        }

        public ICacheService GetCacheService(IEngineContext context)
        {
            return new HybridCacheService(context);
        }
    }
}
