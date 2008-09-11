using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Spark.Tests
{
    [TestFixture]
    public class DefaultResourcePathManagerTester
    {
        [Test]
        public void NormalUrlPathsAreUnchanged()
        {
            var manager = new DefaultResourcePathManager(new SparkSettings());
            var path = manager.GetResourcePath("", "/content/js/jquery.1.2.6.js");
            Assert.AreEqual("/content/js/jquery.1.2.6.js", path);
        }

        [Test]
        public void SiteRootPrependedByDefault()
        {
            var manager = new DefaultResourcePathManager(new SparkSettings());
            var path = manager.GetResourcePath("/my/webapp", "/content/js/jquery.1.2.6.js");
            Assert.AreEqual("/my/webapp/content/js/jquery.1.2.6.js", path);
        }

        [Test]
        public void ReplacingJustSomePrefixes()
        {
            var settings = new SparkSettings()
                .AddResourceMapping("/content/js", "http://my.cdn.com/myaccount/content/js");

            var manager = new DefaultResourcePathManager(settings);

            var path = manager.GetResourcePath("/my/webapp", "/content/js/jquery.1.2.6.js");
            Assert.AreEqual("http://my.cdn.com/myaccount/content/js/jquery.1.2.6.js", path);

            var path2 = manager.GetResourcePath("/my/webapp", "/content/css/yadda.css");
            Assert.AreEqual("/my/webapp/content/css/yadda.css", path2);
        }
    }
}
