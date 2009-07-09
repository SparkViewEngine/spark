using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests.Caching
{
    [TestFixture]
    public class CacheElementTester
    {

        private InMemoryViewFolder _viewFolder;
        private StubViewFactory _factory;

        [SetUp]
        public void Init()
        {
            CompiledViewHolder.Current = new CompiledViewHolder();
            _viewFolder = new InMemoryViewFolder();
            _factory = new StubViewFactory
            {
                Engine = new SparkViewEngine(
                    new SparkSettings()
                        .SetPageBaseType(typeof(StubSparkView)))
                {
                    ViewFolder = _viewFolder
                },
                CacheService = new StubCacheService()
            };
        }

        private string Render(string viewName)
        {
            return Render(viewName, new StubViewData());
        }

        private string Render(string viewName, StubViewData viewData)
        {
            var context = new StubViewContext { ControllerName = "home", ViewName = viewName, Output = new StringBuilder(), Data = viewData };
            _factory.RenderView(context);
            return context.Output.ToString()
                .Replace("\r\n\r\n", "\r\n")
                .Replace("\r\n\r\n", "\r\n");
        }

        [Test]
        public void TemplateRunsNormallyThroughCacheMiss()
        {
            _viewFolder.Add("home\\index.spark", @"
<viewdata model=""System.Func<string>""/>
<div>
<cache key='string.Empty'>
<p>${ViewData.Model()}</p>
</cache>
</div>");
            var calls = 0;
            var contents = Render("index", new StubViewData<Func<string>>
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
            _viewFolder.Add("home\\index.spark", @"
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

            var contents = Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

            contents = Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

        }

        [Test]
        public void CacheInMacroShouldActAsSameSite()
        {
            _viewFolder.Add("home\\index.spark", @"
<viewdata model=""System.Func<string>""/>
<macro name=""foo"">
<cache>
<p>${ViewData.Model()}</p>
</cache>
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

            var contents = Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

            contents = Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
<p>1</p>
</div>"));
            Assert.That(calls, Is.EqualTo(1));

        }

        [Test]
        public void MultipleCachesShouldActAsDifferentSite()
        {
            _viewFolder.Add("home\\index.spark", @"
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

            var contents = Render("index", data);
            Assert.That(contents, Is.EqualTo(@"
<div>
<p>1</p>
<p>2</p>
</div>"));
            Assert.That(calls, Is.EqualTo(2));


            contents = Render("index", data);
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
            _viewFolder.Add("home\\index.spark", @"
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

            var contents = Render("index", data);
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


            contents = Render("index", data);
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

            _viewFolder.Add("home\\index.spark", @"
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

            var contents = Render("index", data);
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

            
            contents = Render("index", data);
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
            _viewFolder.Add("home\\index.spark", @"
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

            var contents = Render("index", data);
            Assert.That(calls, Is.EqualTo(3));
            Assert.That(contents, Is.EqualTo(@"
<ul>
<li>1[1]</li>
<li>2[2]</li>
<li>3[4]</li>
</ul>"));

            contents = Render("index", data);
            Assert.That(calls, Is.EqualTo(5));
            Assert.That(contents, Is.EqualTo(@"
<ul>
<li>4[1]</li>
<li>2[2]</li>
<li>5[4]</li>
</ul>"));
        }

    }
}
