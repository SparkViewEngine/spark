/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.FileSystem;
using Spark.Tests.Models;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture, Category("SparkViewEngine")]
    public class SparkViewFactoryTester
    {
        private MockRepository mocks;

        private StubViewFactory factory;
        private SparkViewEngine engine;
        private StringBuilder sb;

        [SetUp]
        public void Init()
        {
            // clears cache
            CompiledViewHolder.Current = null;


            engine = new SparkViewEngine("Spark.Tests.Stubs.StubSparkView", new FileSystemViewFolder("Views"));
            factory = new StubViewFactory { Engine = engine };

            sb = new StringBuilder();


            mocks = new MockRepository();

        }

        StubViewContext MakeViewContext(string viewName, string masterName)
        {
            return new StubViewContext { ControllerName = "Home", ViewName = viewName, MasterName = masterName, Output = sb };
        }

        StubViewContext MakeViewContext(string viewName, string masterName, StubViewData data)
        {
            return new StubViewContext { ControllerName = "Home", ViewName = viewName, MasterName = masterName, Output = sb, Data = data };
        }



        [Test]
        public void RenderPlainView()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("index", null));

            mocks.VerifyAll();
        }


        [Test]
        public void ForEachTest()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("foreach", null));

            mocks.VerifyAll();

            string content = sb.ToString();
            Assert.That(content.Contains(@"<li class=""odd"">1: foo</li>"));
            Assert.That(content.Contains(@"<li class=""even"">2: bar</li>"));
            Assert.That(content.Contains(@"<li class=""odd"">3: baaz</li>"));
        }


        [Test]
        public void GlobalSetTest()
        {

            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("globalset", null));

            mocks.VerifyAll();

            string content = sb.ToString();
            Assert.That(content.Contains("<p>default: Global set test</p>"));
            Assert.That(content.Contains("<p>7==7</p>"));
        }

        [Test]
        public void MasterTest()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("childview", "layout"));

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content.Contains("<title>Standalone Index View</title>"));
            Assert.That(content.Contains("<h1>Standalone Index View</h1>"));
            Assert.That(content.Contains("<p>no header by default</p>"));
            Assert.That(content.Contains("<p>no footer by default</p>"));
        }

        [Test]
        public void CaptureNamedContent()
        {

            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("namedcontent", "layout"));

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content.Contains("<p>main content</p>"));
            Assert.That(content.Contains("<p>this is the header</p>"));
            Assert.That(content.Contains("<p>footer part one</p>"));
            Assert.That(content.Contains("<p>footer part two</p>"));
        }




        [Test, Ignore("Library no longer references asp.net mvc directly")]
        public void UsingHtmlHelper()
        {

            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("helpers", null));

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content.Contains("<p><a href=\"/Home/Sort\">Click me</a></p>"));
            Assert.That(content.Contains("<p>foo&gt;bar</p>"));
        }

        [Test]
        public void UsingPartialFile()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("usingpartial", null));

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content.Contains("<li>Partial where x=\"zero\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"one\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"two\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"three\"</li>"));
            Assert.That(content.Contains("<li>Partial where x=\"four\"</li>"));
        }

        [Test]
        public void UsingPartialFileImplicit()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("usingpartialimplicit", null));

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content.Contains("<li class=\"odd\">one</li>"));
            Assert.That(content.Contains("<li class=\"even\">two</li>"));
        }


        [Test, Ignore("Library no longer references asp.net mvc directly")]
        public void DeclaringViewDataAccessor()
        {
            mocks.ReplayAll();
            //var comments = new[] { new Comment { Text = "foo" }, new Comment { Text = "bar" } };
            var viewContext = MakeViewContext("viewdata", null/*, new { Comments = comments, Caption = "Hello world" }*/);

            factory.RenderView(viewContext);

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content.Contains("<h1>Hello world</h1>"));
            Assert.That(content.Contains("<p>foo</p>"));
            Assert.That(content.Contains("<p>bar</p>"));
        }

        [Test]
        public void MasterEmptyByDefault()
        {
            var viewFolder = mocks.CreateMock<IViewFolder>();
            Expect.Call(viewFolder.HasView("Shared\\Application.spark")).Return(false);
            SetupResult.For(viewFolder.HasView("Shared\\Foo.spark")).Return(false);

            engine.ViewFolder = viewFolder;

            mocks.ReplayAll();

            var key = engine.CreateKey("Foo", "Baaz", null);

            Assert.AreEqual("Foo", key.ControllerName);
            Assert.AreEqual("Baaz", key.ViewName);
            Assert.IsEmpty(key.MasterName);
        }

        [Test]
        public void MasterApplicationIfPresent()
        {
            var viewFolder = mocks.CreateMock<IViewFolder>();
            Expect.Call(viewFolder.HasView("Shared\\Application.spark")).Return(true);
            SetupResult.For(viewFolder.HasView("Shared\\Foo.spark")).Return(false);

            engine.ViewFolder = viewFolder;


            mocks.ReplayAll();

            var key = engine.CreateKey("Foo", "Baaz", null);


            Assert.AreEqual("Foo", key.ControllerName);
            Assert.AreEqual("Baaz", key.ViewName);
            Assert.AreEqual("Application", key.MasterName);
        }

        [Test]
        public void MasterForControllerIfPresent()
        {
            var viewFolder = mocks.CreateMock<IViewFolder>();
            SetupResult.For(viewFolder.HasView("Shared\\Application.spark")).Return(true);
            SetupResult.For(viewFolder.HasView("Shared\\Foo.spark")).Return(true);

            engine.ViewFolder = viewFolder;

            mocks.ReplayAll();


            var key = engine.CreateKey("Foo", "Baaz", null);


            Assert.AreEqual("Foo", key.ControllerName);
            Assert.AreEqual("Baaz", key.ViewName);
            Assert.AreEqual("Foo", key.MasterName);
        }


        [Test]
        public void UsingNamespace()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("usingnamespace", null);

            factory.RenderView(viewContext);

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content.Contains("<p>Foo</p>"));
            Assert.That(content.Contains("<p>Bar</p>"));
            Assert.That(content.Contains("<p>Hello</p>"));
        }

        [Test]
        public void IfElseElements()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ifelement", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();
            Assert.That(!content.Contains("<if"));
            Assert.That(!content.Contains("<else"));

            Assert.That(content.Contains("<p>argis5</p>"));
            Assert.That(!content.Contains("<p>argis6</p>"));
            Assert.That(content.Contains("<p>argisstill5</p>"));
            Assert.That(!content.Contains("<p>argisnotstill5</p>"));
            Assert.That(!content.Contains("<p>argisnow6</p>"));
            Assert.That(content.Contains("<p>argisstillnot6</p>"));
        }


        [Test]
        public void IfElseAttributes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ifattribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();
            Assert.That(!content.Contains("<if"));
            Assert.That(!content.Contains("<else"));

            Assert.That(content.Contains("<p>argis5</p>"));
            Assert.That(!content.Contains("<p>argis6</p>"));
            Assert.That(content.Contains("<p>argisstill5</p>"));
            Assert.That(!content.Contains("<p>argisnotstill5</p>"));
            Assert.That(!content.Contains("<p>argisnow6</p>"));
            Assert.That(content.Contains("<p>argisstillnot6</p>"));
        }


        [Test]
        public void ChainingElseIfElement()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("elseifelement", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            ContainsInOrder(content,
                "<p>Hi Bob!</p>",
                "<p>Administrator James</p>",
                "<p>Test user.</p>",
                "<p>Anonymous user.</p>");
        }

        [Test]
        public void ChainingElseIfElement2()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("elseifelement2", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            
            ContainsInOrder(content,
                "<p>Hi Bob!</p>",
                "<p>Administrator James</p>",
                "<p>Test user.</p>",
                "<p>Anonymous user.</p>");
        }
        [Test]
        public void ChainingElseIfAttribute()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("elseifattribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            ContainsInOrder(content,
                "<p>Hi Bob!</p>",
                "<p>Administrator James</p>",
                "<p>Test user.</p>",
                "<p>Anonymous user.</p>");
        }

        static void ContainsInOrder(string content, params string[] values)
        {
            int index = 0;
            foreach (string value in values)
            {
                int nextIndex = content.IndexOf(value, index);
                Assert.GreaterOrEqual(nextIndex, 0, string.Format("Looking for {0}", value));
                index = nextIndex + value.Length;
            }
        }


        [Test]
        public void EachAttribute()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("eachattribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            ContainsInOrder(content,
                "<td>Bob</td>",
                "<td>James</td>",
                "<td>SpecialName</td>",
                "<td>Anonymous</td>");
        }

        [Test]
        public void MarkupBasedMacros()
        {
            var data = new StubViewData
                           {
                               {"username", "Bob"}, 
                               {"comments", new[] {
                                   new Comment {Text = "Alpha"},
                                   new Comment {Text = "Beta"},
                                   new Comment {Text = "Gamma"}
                               }}
                           };

            mocks.ReplayAll();
            var viewContext = MakeViewContext("macros", null, data);

            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            ContainsInOrder(content,
                "<p>Bob</p>", "<p>Alpha</p>",
                "<p>Bob</p>", "<p>Beta</p>",
                "<p>Bob</p>", "<p>Gamma</p>",
                "<span class=\"yadda\">Rating: 5</span>");
        }

        [Test]
        public void TestForEachIndex()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("foreachindex", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();
            ContainsInOrder(content,
                "<p>0: Alpha</p>",
                "<p>1: Beta</p>",
                "<p>2: Gamma</p>",
                "<p>3: Delta</p>",
                "<li ", "class=\"even\">Alpha</li>",
                "<li ", "class=\"odd\">Beta</li>",
                "<li ", "class=\"even\">Gamma</li>",
                "<li ", "class=\"odd\">Delta</li>"
                );

        }

    }
}
