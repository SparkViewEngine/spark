using System;

namespace Spark
{
    public class CacheExpires
    {
        private static CacheExpires _empty = new CacheExpires();

        public CacheExpires()
        {
            Absolute = NoAbsoluteExpiration;
            Sliding = NoSlidingExpiration;
        }

        public CacheExpires(DateTime absolute)
        {
            Absolute = absolute;
            Sliding = NoSlidingExpiration;
        }

        public CacheExpires(TimeSpan sliding)
        {
            Absolute = NoAbsoluteExpiration;
            Sliding = sliding;
        }

        public CacheExpires(double sliding)
            : this(TimeSpan.FromSeconds(sliding))
        {
        }

        public DateTime Absolute { get; set; }
        public TimeSpan Sliding { get; set; }

        public static DateTime NoAbsoluteExpiration { get { return System.Web.Caching.Cache.NoAbsoluteExpiration; } }
        public static TimeSpan NoSlidingExpiration { get { return System.Web.Caching.Cache.NoSlidingExpiration; } }
        public static CacheExpires Empty { get { return _empty; } }
    }
}