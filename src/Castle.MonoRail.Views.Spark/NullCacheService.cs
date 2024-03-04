namespace Spark
{
    public class NullCacheService : ICacheService
    {
        public object Get(string identifier)
        {
            return null;
        }

        public void Store(string identifier, CacheExpires expires, ICacheSignal signal, object item)
        {
        }
    }
}