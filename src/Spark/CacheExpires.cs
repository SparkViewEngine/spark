using System;

namespace Spark
{
    /// <summary>
    /// Represents when a cached entry should expire.
    /// </summary>
    public class CacheExpires
    {
        /// <summary>
        /// Constructor for a non expiring cached entry.
        /// </summary>
        public CacheExpires()
        {
            Absolute = NoAbsoluteExpiration;
            Sliding = NoSlidingExpiration;
        }

        /// <summary>
        /// Constructor for a non cached entry expiring at a specified time.
        /// </summary>
        /// <param name="absolute">The time when to invalidate the cached entry.</param>
        public CacheExpires(DateTime absolute)
        {
            Absolute = absolute;
            Sliding = NoSlidingExpiration;
        }

        /// <summary>
        /// Constructor for a cached entry that stays cached as long as it keeps being used.
        /// </summary>
        /// <param name="sliding">The timespan of sliding expirations.</param>
        public CacheExpires(TimeSpan sliding)
        {
            Absolute = NoAbsoluteExpiration;
            Sliding = sliding;
        }

        /// <summary>
        /// Constructor for a cached entry that stays cached as long as it keeps being used.
        /// </summary>
        /// <param name="sliding">The number of seconds of sliding expirations.</param>
        public CacheExpires(double sliding)
            : this(TimeSpan.FromSeconds(sliding))
        {
        }

        public DateTime Absolute { get; set; }
        public TimeSpan Sliding { get; set; }

        public static DateTime NoAbsoluteExpiration => DateTime.MaxValue;

        public static TimeSpan NoSlidingExpiration => TimeSpan.Zero;

        /// <summary>
        /// Cached entry never to expire.
        /// </summary>
        public static CacheExpires Empty { get; } = new();
    }
}