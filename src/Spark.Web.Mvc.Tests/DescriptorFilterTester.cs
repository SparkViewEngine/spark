using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;

using Rhino.Mocks;
using Spark.Web.Mvc.Descriptors;

namespace Spark.Web.Mvc.Tests
{
    [TestFixture]
    public class DescriptorFilterTester
    {
        private ControllerContext _context;
        private Dictionary<string, object> _extra;

        [SetUp]
        public void SetUp()
        {
            _context = new ControllerContext(
                MockRepository.GenerateStub<HttpContextBase>(),
                new RouteData(),
                MockRepository.GenerateStub<ControllerBase>());

            _extra = new Dictionary<string, object>();
        }

        [Test]
        public void AreaFilterUsesRouteDataTokensByDefault()
        {
            var filter = new AreaDescriptorFilter();

            _context.RouteData.DataTokens.Add("area", "foo");
            filter.ExtraParameters(_context, _extra);

            Assert.That(_extra["area"], Is.EqualTo("foo"));
        }


        [Test]
        public void AreaFilterUsesRouteValuesForBackCompat() {
            var filter = new AreaDescriptorFilter();

            _context.RouteData.Values.Add("area", "foo");
            filter.ExtraParameters(_context, _extra);

            Assert.That(_extra["area"], Is.EqualTo("foo"));
        }

        [Test]
        public void AreaFilterAddsNameToStartOfPath()
        {
            var filter = new AreaDescriptorFilter();

            _extra["area"] = "quux";
            var locations = filter.PotentialLocations(
                new[]
                    {
                        @"foo\bar.spark",
                        @"shared\bar.spark",
                    }, _extra);

            Assert.That(locations.SequenceEqual(
                            new[]
                                {
                                    @"quux\foo\bar.spark",
                                    @"quux\shared\bar.spark",
                                    @"foo\bar.spark",
                                    @"shared\bar.spark",
                                }));
        }

        [Test]
        public void ThemeFilterDelegateCanExtractParameter()
        {
            var filter = ThemeDescriptorFilter.For(context => "foo");
            filter.ExtraParameters(_context, _extra);
            Assert.That(_extra["theme"], Is.EqualTo("foo"));
        }

        [Test]
        public void ThemeFilterAddsThemesAndNameToPath()
        {
            var filter = ThemeDescriptorFilter.For(x => null);

            _extra["theme"] = "blue";
            var locations = filter.PotentialLocations(
                new[]
                    {
                        @"foo\bar.spark",
                        @"shared\bar.spark",
                    }, _extra);

            Assert.That(locations.SequenceEqual(
                            new[]
                                {
                                    @"themes\blue\foo\bar.spark",
                                    @"themes\blue\shared\bar.spark",
                                    @"foo\bar.spark",
                                    @"shared\bar.spark",
                                }));
        }

        [Test]
        public void LanguageFilterDelegateCanExtractParameter()
        {
            var filter = LanguageDescriptorFilter.For(context => "foo");
            filter.ExtraParameters(_context, _extra);
            Assert.That(_extra["language"], Is.EqualTo("foo"));
        }

        [Test]
        public void LanguageFilterChangesExtensionOnceOrTwice()
        {
            var filter = LanguageDescriptorFilter.For(x => null);

            _extra["language"] = "en";
            var locations = filter.PotentialLocations(
                new[]
                    {
                        @"foo\bar.spark",
                        @"shared\bar.spark",
                    }, _extra);

            Assert.That(locations.SequenceEqual(
                            new[]
                                {
                                    @"foo\bar.en.spark",
                                    @"foo\bar.spark",
                                    @"shared\bar.en.spark",
                                    @"shared\bar.spark",
                                }));

            _extra["language"] = "en-us";
            locations = filter.PotentialLocations(
                new[]
                    {
                        @"foo\bar.spark",
                        @"shared\bar.spark",
                    }, _extra);

            Assert.That(locations.SequenceEqual(
                            new[]
                                {
                                    @"foo\bar.en-us.spark",
                                    @"foo\bar.en.spark",
                                    @"foo\bar.spark",
                                    @"shared\bar.en-us.spark",
                                    @"shared\bar.en.spark",
                                    @"shared\bar.spark",
                                }));
        }
    }
}
