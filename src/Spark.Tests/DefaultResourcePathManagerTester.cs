// Copyright 2008 Louis DeJardin - http://whereslou.com
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
