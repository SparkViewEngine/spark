//-------------------------------------------------------------------------
// <copyright file="SparkViewFactoryTester.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>Jacob Proffitt</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Extensions;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Caching
{
    [TestFixture]
    public class CacheElementTester
    {
        private InMemoryViewFolder _viewFolder;
        private StubViewFactory _factory;
        private StubCacheService _cacheService;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings().SetPageBaseType(typeof(StubSparkView));

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder, InMemoryViewFolder>()
                .AddSingleton<ICacheService, StubCacheService>()
                .BuildServiceProvider();

            this._viewFolder = (InMemoryViewFolder) sp.GetService<IViewFolder>();
            this._cacheService = (StubCacheService) sp.GetService<ICacheService>();

            this._factory = new StubViewFactory
            {
                Engine = (SparkViewEngine)sp.GetService<ISparkViewEngine>(),
                CacheService = this._cacheService
            };
        }

        private string Render(string viewName)
        {
            return this.Render(viewName, new StubViewData());
        }

        private string Render(string viewName, StubViewData viewData)
        {
            var context = new StubViewContext { ControllerName = "home", ViewName = viewName, Output = new StringBuilder(), Data = viewData };
            this._factory.RenderView(context);
            return context.Output.ToString()
                .Replace("\r\n\r\n", "\r\n")
                .Replace("\r\n\r\n", "\r\n");
        }

        [Test]
        public void TemplateRunsNormallyThroughCacheMiss()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<div>
<cache key='string.Empty'>
<p>${ViewData.Model()}</p>
</cache>
</div>");
            var calls = 0;
            var contents = this.Render("index", new StubViewData<Func<string>>
                                           {
                                               Model = () => (++calls).ToString()
                                           });
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void TemplateDoesNotRunThroughCacheHit()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<div>
<cache key='string.Empty'>
<p>${ViewData.Model()}</p>
</cache>
</div>");
            int calls = 0;
            var data = new StubViewData<Func<string>>
                       {
                           Model = () => (++calls).ToString()
                       };

            var contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

            contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

        }

        [Test]
        public void CacheInMacroShouldActAsSameSite()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<macro name=""foo"">
<cache><p>${ViewData.Model()}</p></cache>
</macro>
<div>
${foo()}
${foo()}
</div>");

            int calls = 0;
            var data = new StubViewData<Func<string>>
            {
                Model = () => (++calls).ToString()
            };

            var contents = this.Render("index", data);
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1</p>",
                "<p>1</p>"));

            Assert.That(calls, Is.EqualTo(1));

            contents = this.Render("index", data);
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1</p>",
                "<p>1</p>"));

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void MultipleCachesShouldActAsDifferentSite()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<div>
<cache>
<p>${ViewData.Model()}</p>
</cache>
<cache>
<p>${ViewData.Model()}</p>
</cache>
</div>");

            var calls = 0;
            var data = new StubViewData<Func<string>>
            {
                Model = () => (++calls).ToString()
            };

            var contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
<p>2</p>
</div>"));
            Assert.That(calls, Is.EqualTo(2));


            contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
<p>2</p>
</div>"));
            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void NamedContentShouldIndividuallySpoolAndCache()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<div>
<content name='foo'>
<p>f${ViewData.Model()}[1]</p>
</content>
cache
<cache>
<p>${ViewData.Model()}[2]c</p>
<content name='foo'>
<p>f${ViewData.Model()}[3]c</p>
</content>
<content name='bar'>
<p>b${ViewData.Model()}[4]c</p>
</content>
<p>${ViewData.Model()}[5]c</p>
</cache>
<content name='bar'>
<p>b${ViewData.Model()}[6]</p>
</content>
placed
<p>${ViewData.Model()}[7]</p>
<use content='foo'/>
<use content='bar'/>
<p>${ViewData.Model()}[8]</p>
</div>");

            var calls = 0;
            var data = new StubViewData<Func<string>>
            {
                Model = () => (++calls).ToString()
            };

            var contents = this.Render("index", data);
            Assert.That(calls, Is.EqualTo(8));
            Assert.That(contents, Is.EqualTo(@"
<div>
cache
<p>2[2]c</p>
<p>5[5]c</p>
placed
<p>7[7]</p>
<p>f1[1]</p>
<p>f3[3]c</p>
<p>b4[4]c</p>
<p>b6[6]</p>
<p>8[8]</p>
</div>"));


            contents = this.Render("index", data);
            Assert.That(calls, Is.EqualTo(12));
            Assert.That(contents, Is.EqualTo(@"
<div>
cache
<p>2[2]c</p>
<p>5[5]c</p>
placed
<p>11[7]</p>
<p>f9[1]</p>
<p>f3[3]c</p>
<p>b4[4]c</p>
<p>b10[6]</p>
<p>12[8]</p>
</div>"));
        }


        [Test]
        public void OutputWhileNamedContentActiveShouldAppearOnceAtCorrectTarget()
        {

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<ul>
<content name='foo'>
<li>${ViewData.Model()}[1]</li>
</content>
<li>${ViewData.Model()}[2]</li>
<content name='foo'>
<cache>
<li>${ViewData.Model()}[3]c</li>
<content name='foo'>
hana
</content>
<li>${ViewData.Model()}[4]c</li>
</cache>
</content>
<li>${ViewData.Model()}[5]</li>
<content name='foo'>
<li>${ViewData.Model()}[6]</li>
</content>
<use content='foo'/>
</ul>");

            var calls = 0;
            var data = new StubViewData<Func<string>>
            {
                Model = () => (++calls).ToString()
            };

            var contents = this.Render("index", data);
            Assert.That(calls, Is.EqualTo(6));
            Assert.That(contents, Is.EqualTo(@"
<ul>
<li>2[2]</li>
<li>5[5]</li>
<li>1[1]</li>
<li>3[3]c</li>
hana
<li>4[4]c</li>
<li>6[6]</li>
</ul>"));


            contents = this.Render("index", data);
            Assert.That(calls, Is.EqualTo(10));
            Assert.That(contents, Is.EqualTo(@"
<ul>
<li>8[2]</li>
<li>9[5]</li>
<li>7[1]</li>
<li>3[3]c</li>
hana
<li>4[4]c</li>
<li>10[6]</li>
</ul>"));
        }


        [Test]
        public void OnceFlagsSetWhenCacheRecordedShouldBeSetWhenCacheReplayed()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<ul>
<li once='foo'>${ViewData.Model()}[1]</li>
<cache>
<li once='bar'>${ViewData.Model()}[2]</li>
<li once='foo'>${ViewData.Model()}[3]</li>
</cache>
<li once='quux'>${ViewData.Model()}[4]</li>
<li once='bar'>${ViewData.Model()}[5]</li>
<li once='foo'>${ViewData.Model()}[6]</li>
<li once='quux'>${ViewData.Model()}[7]</li>
</ul>");

            var calls = 0;
            var data = new StubViewData<Func<string>>
            {
                Model = () => (++calls).ToString()
            };

            var contents = this.Render("index", data);
            Assert.That(calls, Is.EqualTo(3));
            Assert.That(contents, Is.EqualTo(@"
<ul>
<li>1[1]</li>
<li>2[2]</li>
<li>3[4]</li>
</ul>"));

            contents = this.Render("index", data);
            Assert.That(calls, Is.EqualTo(5));
            Assert.That(contents, Is.EqualTo(@"
<ul>
<li>4[1]</li>
<li>2[2]</li>
<li>5[4]</li>
</ul>"));
        }

        [Test]
        public void CacheFinallyShouldNotThrowExceptionWhenKeyIsBad()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<macro name='boom'>
#throw new System.ApplicationException();
</macro>
<cache key='boom()'>
foo
</cache>
");
            Assert.That(() => this.Render("index", new StubViewData()), Throws.TypeOf<ApplicationException>());
        }

        [Test]
        public void CacheAttributeUsedAsKey()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<var stuff='new[]{1,3,5,2,3,3,5,7}' count='0'/>
<for each='var x in stuff'>
<p cache='x'>${x}:${++count}</p>
</for>");

            var contents = this.Render("index");
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1:1</p>",
                "<p>3:2</p>",
                "<p>5:3</p>",
                "<p>2:4</p>",
                "<p>3:2</p>",
                "<p>3:2</p>",
                "<p>5:3</p>",
                "<p>7:5</p>"));
        }

        [Test]
        public void CacheExpiresTakesOutContentAfterTime()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>""/>
<for each='var x in new[]{1,2,3,1,2,3}'>
<cache key='x' expires='System.TimeSpan.FromSeconds(30)'>
<p>${x}:${ViewData.Model()}</p>
</cache>
</for>
<p cache.expires='40'>last:${ViewData.Model()}</p>
");

            var calls = 0;
            var data = new StubViewData<Func<string>>
            {
                Model = () => (++calls).ToString()
            };

            var contents = this.Render("index", data);
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1:1</p>",
                "<p>2:2</p>",
                "<p>3:3</p>",
                "<p>1:1</p>",
                "<p>2:2</p>",
                "<p>3:3</p>",
                "<p>last:4</p>"));

            this._cacheService.UtcNow = this._cacheService.UtcNow.AddSeconds(25);
            contents = this.Render("index", data);
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1:1</p>",
                "<p>2:2</p>",
                "<p>3:3</p>",
                "<p>1:1</p>",
                "<p>2:2</p>",
                "<p>3:3</p>",
                "<p>last:4</p>"));

            this._cacheService.UtcNow = this._cacheService.UtcNow.AddSeconds(10);
            contents = this.Render("index", data);
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1:5</p>",
                "<p>2:6</p>",
                "<p>3:7</p>",
                "<p>1:5</p>",
                "<p>2:6</p>",
                "<p>3:7</p>",
                "<p>last:4</p>"));

            this._cacheService.UtcNow = this._cacheService.UtcNow.AddSeconds(10);
            contents = this.Render("index", data);
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1:5</p>",
                "<p>2:6</p>",
                "<p>3:7</p>",
                "<p>1:5</p>",
                "<p>2:6</p>",
                "<p>3:7</p>",
                "<p>last:8</p>"));
        }

        [Test]
        public void CommaCreatesMultiPartKey()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"),
                            @"
<viewdata model=""System.Func<string>""/>
<for each='var x in new[]{1,2,3,1,2,3}'>
<p cache='x,xIndex'>${x}:${ViewData.Model()}</p>
</for>");

            var calls = 0;
            var data = new StubViewData<Func<string>>
                       {
                           Model = () => (++calls).ToString()
                       };

            var contents = this.Render("index", data);
            Assert.That(contents, Tests.Contains.InOrder(
                "<p>1:1</p>",
                "<p>2:2</p>",
                "<p>3:3</p>",
                "<p>1:4</p>",
                "<p>2:5</p>",
                "<p>3:6</p>"));

            Assert.That(this._cacheService.AllKeys.Count(x => x.Substring(32) == "1\u001f0"), Is.EqualTo(1));
            Assert.That(this._cacheService.AllKeys.Count(x => x.Substring(32) == "2\u001f1"), Is.EqualTo(1));
            Assert.That(this._cacheService.AllKeys.Count(x => x.Substring(32) == "3\u001f2"), Is.EqualTo(1));
            Assert.That(this._cacheService.AllKeys.Count(x => x.Substring(32) == "1\u001f3"), Is.EqualTo(1));
            Assert.That(this._cacheService.AllKeys.Count(x => x.Substring(32) == "2\u001f4"), Is.EqualTo(1));
            Assert.That(this._cacheService.AllKeys.Count(x => x.Substring(32) == "3\u001f5"), Is.EqualTo(1));
        }


        [Test]
        public void SignalWillExpireOutputCachingEntry()
        {
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<viewdata model=""System.Func<string>"" datasignal='Spark.ICacheSignal'/>
<div>
<cache key='string.Empty' signal='datasignal'>
<p>${ViewData.Model()}</p>
</cache>
</div>");
            var signal = new CacheSignal();
            var calls = 0;
            var data = new StubViewData<Func<string>>
            {
                Model = () => (++calls).ToString()
            };
            data["datasignal"] = signal;

            var contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

            contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

            signal.FireChanged();

            contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>2</p>
</div>"));
            Assert.That(calls, Is.EqualTo(2));

            contents = this.Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>2</p>
</div>"));
            Assert.That(calls, Is.EqualTo(2));

        }
    }
}
