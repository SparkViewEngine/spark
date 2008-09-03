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
using System.IO;
using System.Linq;
using MvcContrib.ViewFactories;
using NUnit.Framework;
using Spark.FileSystem;

namespace MvcContrib.SparkViewEngine.Tests
{
    [TestFixture]
    public class ViewSourceLoaderWrapperTester
    {
        [Test]
        public void ResultsAreTheSame()
        {
            IViewSourceLoader loader = new FileSystemViewSourceLoader("MvcContrib.Tests.Views");
            IViewFolder wrapper = new ViewSourceLoaderWrapper(loader);

            Assert.AreEqual(loader.HasView("Home\\foreach.spark"), wrapper.HasView("Home\\foreach.spark"));
            Assert.AreEqual(loader.HasView("Home\\nosuchfile.spark"), wrapper.HasView("Home\\nosuchfile.spark"));

            var loaderViews = loader.ListViews("Shared");
            var wrapperViews = wrapper.ListViews("Shared");
            Assert.AreEqual(loaderViews.Count(), wrapperViews.Count);

            foreach (var viewName in loaderViews)
            {
                Assert.That(wrapperViews.Contains(viewName));
            }

            var loaderView = loader.GetViewSource("Home\\foreach.spark");
            var wrapperView = wrapper.GetViewSource("Home\\foreach.spark");

            Assert.AreEqual(loaderView.LastModified, wrapperView.LastModified);

            var loaderReader = new StreamReader(loaderView.OpenViewStream());
            var wrapperReader = new StreamReader(wrapperView.OpenViewStream());
            Assert.AreEqual(loaderReader.ReadToEnd(), wrapperReader.ReadToEnd());
        }
    }
}