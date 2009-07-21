namespace Spark
{
    public interface ICacheManager
    {
    }

    public interface ICacheService
    {
        object Get(string identifier);
        void Store(string identifier, CacheExpires expires, object item);
    }
}
