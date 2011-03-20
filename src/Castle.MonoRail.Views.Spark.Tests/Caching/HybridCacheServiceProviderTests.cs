using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Views.Spark.Wrappers;
using NUnit.Framework;

using Rhino.Mocks;
using Spark;

namespace Castle.MonoRail.Views.Spark.Tests.Caching
{
    [TestFixture]
    public class HybridCacheServiceProviderTests
    {
        private IEngineContext _context;

        [SetUp]
        public void Init()
        {
            _context = MockRepository.GenerateMock<IEngineContext>();
            _context
                .Stub(x => x.Services)
                .Return(MockRepository.GenerateMock<IMonoRailServices>());
            _context.Services
                .Stub(x => x.CacheProvider)
                .Return(MockRepository.GenerateMock<ICacheProvider>());

            var httpContext = new HttpContext(new HttpRequest("", "http://localhost", ""), new HttpResponse(null));

            _context
                .Stub(x => x.UnderlyingContext)
                .Return(httpContext);

            foreach (var key in HttpRuntime.Cache.OfType<DictionaryEntry>().Select(x=>x.Key))
                HttpRuntime.Cache.Remove(Convert.ToString(key));
        }

        [Test]
        public void ShouldProvideInstanceOfExpectedType()
        {
            var cacheServiceProvider = new HybridCacheServiceProvider();
            var cacheService = cacheServiceProvider.GetCacheService(_context);
            Assert.That(cacheService, Is.TypeOf(typeof(HybridCacheService)));
        }

        [Test]
        public void StoreShouldUseTheMonorailServiceWhenExpiresAndSignalAreNotProvided()
        {
            _context.Services.CacheProvider.Expect(x => x.Store("foo1", "bar1"));
            _context.Services.CacheProvider.Expect(x => x.Store("foo2", "bar2"));
            _context.Services.CacheProvider.Expect(x => x.Store("foo3", "bar3"));
            _context.Services.CacheProvider.Expect(x => x.Store("foo4", "bar4"));

            var cacheServiceProvider = new HybridCacheServiceProvider();
            var cacheService = cacheServiceProvider.GetCacheService(_context);
            cacheService.Store("foo1", null, null, "bar1");
            cacheService.Store("foo2", new CacheExpires(), null, "bar2");
            cacheService.Store("foo3", new CacheExpires(CacheExpires.NoAbsoluteExpiration), null, "bar3");
            cacheService.Store("foo4", new CacheExpires(CacheExpires.NoSlidingExpiration), null, "bar4");

            _context.Services.CacheProvider.VerifyAllExpectations();
        }

        [Test]
        public void MonorailServiceUsedWhenCacheGetIsCalled()
        {
            _context.Services.CacheProvider.Expect(x => x.Get("hello")).Return("world");

            var cacheServiceProvider = new HybridCacheServiceProvider();
            var cacheService = cacheServiceProvider.GetCacheService(_context);
            var data = cacheService.Get("hello");
            Assert.That(data, Is.EqualTo("world"));

            _context.Services.CacheProvider.VerifyAllExpectations();
        }

        [Test]
        public void StoreShouldUseHttpContextCacheWhenExpiresOrSignalAreProvided()
        {
            _context.Services.CacheProvider
                .Expect(x => x.Store(null, null))
                .IgnoreArguments()
                .Repeat.Never();

            var signal = MockRepository.GenerateStub<ICacheSignal>();

            var cacheServiceProvider = new HybridCacheServiceProvider();
            var cacheService = cacheServiceProvider.GetCacheService(_context);
            cacheService.Store("foo1", new CacheExpires(4), null, "bar1");
            cacheService.Store("foo2", new CacheExpires(TimeSpan.FromMinutes(2)), null, "bar2");
            cacheService.Store("foo3", new CacheExpires(DateTime.UtcNow.AddSeconds(4)), null, "bar3");
            cacheService.Store("foo4", null, signal, "bar4");

            _context.Services.CacheProvider.VerifyAllExpectations();
        }

        [Test]
        public void StoreAndGetToBothMonorailAndHttpCacheWorkSideBySide()
        {
            _context.Services.CacheProvider.Expect(x => x.Store("xfoo1", "bar1"));
            _context.Services.CacheProvider.Expect(x => x.Store("xfoo2", "bar2")).Repeat.Never();
            _context.Services.CacheProvider.Expect(x => x.Get("xfoo1")).Return("bar1");
            _context.Services.CacheProvider.Expect(x => x.Get("xfoo2")).Return(null);
            _context.Services.CacheProvider.Expect(x => x.Get("xfoo3")).Return(null);

            var signal = MockRepository.GenerateStub<ICacheSignal>();

            var cacheServiceProvider = new HybridCacheServiceProvider();
            var cacheService = cacheServiceProvider.GetCacheService(_context);
            cacheService.Store("xfoo1", null, null, "bar1");
            cacheService.Store("xfoo2", null, signal, "bar2");

            Assert.That(cacheService.Get("xfoo1"), Is.EqualTo("bar1"));
            Assert.That(cacheService.Get("xfoo2"), Is.EqualTo("bar2"));
            Assert.That(cacheService.Get("xfoo3"), Is.Null);

            _context.Services.CacheProvider.VerifyAllExpectations();

        }
    }
}
