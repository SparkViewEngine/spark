using System;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;

namespace Spark.Tests
{
    [TestFixture]
    public class InMemoryServiceTest
    {
        [Test]
        public void TestStoreValueThenRetrieveIt()
        {
            var service = new InMemoryCacheService(new MemoryCache(new MemoryCacheOptions()));

            var item = new { };

            service.Store("identifier", CacheExpires.Empty, null, item);

            var retrieved = service.Get("identifier");

            Assert.That(retrieved, Is.SameAs(item));
        }

        [Test]
        public void TestStoreValueThenRetrieveItAfterAbsoluteExpiration()
        {
            var service = new InMemoryCacheService(new MemoryCache(new MemoryCacheOptions()));

            var item = new { };

            service.Store("identifier", new CacheExpires(DateTime.UtcNow.AddMilliseconds(50)), null, item);

            Thread.Sleep(100);

            var retrieved = service.Get("identifier");

            Assert.That(retrieved, Is.Null);
        }

        [Test]
        public void TestStoreValueThenRetrieveItWhenExpirationSlides()
        {
            var service = new InMemoryCacheService(new MemoryCache(new MemoryCacheOptions()));

            var item = new { };

            service.Store("identifier", new CacheExpires(TimeSpan.FromMilliseconds(75)), null, item);

            object retrieved;

            for (var i = 0; i < 3; i++)
            {
                Thread.Sleep(50);

                _ = service.Get("identifier");
            }

            retrieved = service.Get("identifier");

            Assert.That(retrieved, Is.Not.Null);

            Assert.That(retrieved, Is.SameAs(item));
        }

        [Test]
        public void TestStoreValueWithSignal()
        {
            var service = new InMemoryCacheService(new MemoryCache(new MemoryCacheOptions()));

            var item = new { };

            var signal = new CacheSignal();

            service.Store("identifier", null, signal, item);

            var retrieved = service.Get("identifier");

            Assert.That(retrieved, Is.SameAs(item));

            signal.FireChanged();

            retrieved = service.Get("identifier");

            Assert.That(retrieved, Is.Null);
        }
    }
}
