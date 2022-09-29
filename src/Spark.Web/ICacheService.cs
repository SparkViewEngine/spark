using System;

namespace Spark
{
    public interface ICacheService
    {
        object Get(string identifier);
        void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item);
    }
}
