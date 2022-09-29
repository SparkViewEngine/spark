// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using NUnit.Framework;
using Spark.Web;

namespace Spark
{
    [TestFixture]
    public class DefaultResourcePathManagerTester
    {
        [Test]
        public void NormalUrlPathsAreUnchanged()
        {
            var manager = new DefaultResourcePathManager(new ApplicationBaseSparkSettings());
            var path = manager.GetResourcePath("", "/content/js/jquery.1.2.6.js");
            Assert.AreEqual("/content/js/jquery.1.2.6.js", path);
        }

        [Test]
        public void SiteRootPrependedByDefault()
        {
            var manager = new DefaultResourcePathManager(new ApplicationBaseSparkSettings());
            var path = manager.GetResourcePath("/my/webapp", "/content/js/jquery.1.2.6.js");
            Assert.AreEqual("/my/webapp/content/js/jquery.1.2.6.js", path);
        }

        [Test]
        public void SiteRootPrependedByDefaultEnsureSlashBetweenSiteRootAndPath()
        {
            var manager = new DefaultResourcePathManager(new ApplicationBaseSparkSettings());

            var path = manager.GetResourcePath("/my/webapp", "content/js/jquery.1.2.6.js");
            Assert.AreEqual("/my/webapp/content/js/jquery.1.2.6.js", path);

            path = manager.GetResourcePath("/my/webapp/", "/content/js/jquery.1.2.6.js");
            Assert.AreEqual("/my/webapp/content/js/jquery.1.2.6.js", path);
        }
        
        [Test]
        public void TildePrefixedPathMorphAsSiteRootPrependedByDefault()
        {
            var manager = new DefaultResourcePathManager(new ApplicationBaseSparkSettings());
            var path = manager.GetResourcePath("/my/webapp", "~/content/js/jquery.1.2.6.js");
            Assert.AreEqual("/my/webapp/content/js/jquery.1.2.6.js", path);   
        }

        [Test]
        public void WhenResourceMappingWithNoStopAttributeThenNextMatchOrDefaultMatchWillBeProcessedWithItOutput()
        {
            var settings = new ApplicationBaseSparkSettings()
                .AddResourceMapping("/js", "~/content/js", false);

            var manager = new DefaultResourcePathManager(settings);

            var path = manager.GetResourcePath("/my/webapp", "/js/jquery.1.2.6.js");
            Assert.AreEqual("/my/webapp/content/js/jquery.1.2.6.js", path);

            settings.AddResourceMapping("/ftpmirror", "/ftp/mymyrror.com", false);
            settings.AddResourceMapping("/ftp/", "ftp://");

            var path2 = manager.GetResourcePath("/my/webapp", "/ftpmirror/1.zip");
            Assert.AreEqual("ftp://mymyrror.com/1.zip", path2);
        }
        
        [Test]
        public void ReplacingJustSomePrefixes()
        {
            var settings = new ApplicationBaseSparkSettings()
                .AddResourceMapping("/content/js", "http://my.cdn.com/myaccount/content/js");

            var manager = new DefaultResourcePathManager(settings);

            var path = manager.GetResourcePath("/my/webapp", "/content/js/jquery.1.2.6.js");
            Assert.AreEqual("http://my.cdn.com/myaccount/content/js/jquery.1.2.6.js", path);

            var path2 = manager.GetResourcePath("/my/webapp", "/content/css/yadda.css");
            Assert.AreEqual("/my/webapp/content/css/yadda.css", path2);
        }

        [Test]
        public void AllTypesOfPathSlashesShouldCombineWithSingleForwardSlash()
        {
            var manager = new DefaultResourcePathManager(new ApplicationBaseSparkSettings());
            var path1 = manager.PathConcat("foo", "bar");
            var path2 = manager.PathConcat("foo/", "bar");
            var path3 = manager.PathConcat("foo", "/bar");
            var path4 = manager.PathConcat("foo/", "/bar");

            Assert.That(path1, Is.EqualTo("foo/bar"));
            Assert.That(path2, Is.EqualTo("foo/bar"));
            Assert.That(path3, Is.EqualTo("foo/bar"));
            Assert.That(path4, Is.EqualTo("foo/bar"));
        }

        [Test]
        public void ReplacingJustSomePrefixesThatHaveTildeNoStop()
        {
            var settings = new ApplicationBaseSparkSettings()
                .AddResourceMapping("~/content/js", "http://my.cdn.com/myaccount/content/js", false);

            var manager = new DefaultResourcePathManager(settings);

            var path = manager.GetResourcePath("/my/webapp", "~/content/js/jquery.1.2.6.js");
            Assert.AreEqual("http://my.cdn.com/myaccount/content/js/jquery.1.2.6.js", path);

            var path2 = manager.GetResourcePath("/my/webapp", "~/content/css/yadda.css");
            Assert.AreEqual("/my/webapp/content/css/yadda.css", path2);
        }
    }
}
