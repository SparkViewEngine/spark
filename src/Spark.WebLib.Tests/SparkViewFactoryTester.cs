//-------------------------------------------------------------------------
// <copyright file="SparkViewFactoryTester.cs">
// Copyright 2008-2024 Louis DeJardin
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
// <author>Gauthier Segay</author>
// <author>Jacob Proffitt</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Spark.Extensions;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

using Rhino.Mocks;
using Spark.Compiler;
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
        private SparkSettings settings;

        [SetUp]
        public void Init()
        {
            settings = new SparkSettings().SetBaseClassTypeName("Spark.Tests.Stubs.StubSparkView");

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(new FileSystemViewFolder("Spark.Tests.Views"))
                .BuildServiceProvider();

            engine = (SparkViewEngine)sp.GetService<ISparkViewEngine>();

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

            factory.RenderView(MakeViewContext("Index", null));

            mocks.VerifyAll();
        }

        [Test]
        public void ForEachTest()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("foreach", null));

            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain(@"<li class=""odd"">1: foo</li>"));
                Assert.That(content, Does.Contain(@"<li class=""even"">2: bar</li>"));
                Assert.That(content, Does.Contain(@"<li class=""odd"">3: baaz</li>"));
            });
        }

        [Test]
        public void GlobalSetTest()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("globalset", null));

            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>default: Global set test</p>"));
                Assert.That(content, Does.Contain("<p>7==7</p>"));
            });
        }

        [Test]
        public void MasterTest()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("childview", "layout"));

            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<title>Standalone Index View</title>"));
                Assert.That(content, Does.Contain("<h1>Standalone Index View</h1>"));
                Assert.That(content, Does.Contain("<p>no header by default</p>"));
                Assert.That(content, Does.Contain("<p>no footer by default</p>"));
            });
        }

        [Test]
        public void CaptureNamedContent()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("namedcontent", "layout"));

            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>main content</p>"));
                Assert.That(content, Does.Contain("<p>this is the header</p>"));
                Assert.That(content, Does.Contain("<p>footer part one</p>"));
                Assert.That(content, Does.Contain("<p>footer part two</p>"));
            });
        }

        [Test]
        public void UsingPartialFile()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("usingpartial", null));

            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<li>Partial where x=\"zero\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"one\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"two\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"three\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"four\"</li>"));
            });
        }

        [Test]
        public void UsingPartialWithRenderElement()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("usingpartial-render-element", null));

            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<li>Partial where x=\"zero\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"one\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"two\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"three\"</li>"));
                Assert.That(content, Does.Contain("<li>Partial where x=\"four\"</li>"));
            });
        }

        [Test]
        public void UsingPartialFileImplicit()
        {
            mocks.ReplayAll();

            factory.RenderView(MakeViewContext("usingpartialimplicit", null));

            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<li class=\"odd\">one</li>"));
                Assert.That(content, Does.Contain("<li class=\"even\">two</li>"));
            });
        }

        [Test]
        public void UsingNamespace()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("usingnamespace", null);

            factory.RenderView(viewContext);

            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>Foo</p>"));
                Assert.That(content, Does.Contain("<p>Bar</p>"));
                Assert.That(content, Does.Contain("<p>Hello</p>"));
            });
        }

        [Test]
        public void IfElseElements()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ifelement", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("<if"));
            Assert.That(content, Does.Not.Contain("<else"));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>argis5</p>"));
                Assert.That(content, Does.Not.Contain("<p>argis6</p>"));
            });
            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>argisstill5</p>"));
                Assert.That(content, Does.Not.Contain("<p>argisnotstill5</p>"));
            });
            Assert.That(content, Does.Not.Contain("<p>argisnow6</p>"));
            Assert.That(content, Does.Contain("<p>argisstillnot6</p>"));
        }

        [Test]
        public void IfElseAttributes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ifattribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("<if"));
            Assert.That(content, Does.Not.Contain("<else"));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>argis5</p>"));
                Assert.That(content, Does.Not.Contain("<p>argis6</p>"));
            });
            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>argisstill5</p>"));
                Assert.That(content, Does.Not.Contain("<p>argisnotstill5</p>"));
            });
            Assert.That(content, Does.Not.Contain("<p>argisnow6</p>"));
            Assert.That(content, Does.Contain("<p>argisstillnot6</p>"));
        }

        [Test]
        public void UnlessElements()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("unlesselement", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("<unless"));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>argisnot6</p>"));
                Assert.That(content, Does.Not.Contain("<p>argis5</p>"));
            });
        }

        [Test]
        public void UnlessAttributes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("unlessattribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("<unless"));

            Assert.Multiple(() =>
            {
                Assert.That(content, Does.Contain("<p>argisnot6</p>"));
                Assert.That(content, Does.Not.Contain("<p>argis5</p>"));
            });
        }

        [Test]
        public void ChainingElseIfElement()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("elseifelement", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Tests.Contains.InOrder(
                "<p>Hi Bob!</p>",
                "<p>Administrator James</p>",
                "<p>Test user.</p>",
                "<p>Anonymous user.</p>"));
        }

        [Test]
        public void ChainingElseIfElement2()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("elseifelement2", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<p>Hi Bob!</p>",
                "<p>Administrator James</p>",
                "<p>Test user.</p>",
                "<p>Anonymous user.</p>"));
        }

        [Test]
        public void ChainingElseIfAttribute()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("elseifattribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content, Contains.InOrder(
                "<p>Hi Bob!</p>",
                "<p>Administrator James</p>",
                "<p>Test user.</p>",
                "<p>Anonymous user.</p>"));
        }

        [Test]
        public void EachAttribute()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("eachattribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            Assert.That(content, Contains.InOrder(
                "<td>Bob</td>",
                "<td>James</td>",
                "<td>SpecialName</td>",
                "<td>Anonymous</td>"));
        }

        [Test]
        public void EachAttributeWhitespace()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("eachattribute-whitespace", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            var content = sb.ToString();
            var expected =
@"<ul>
  <li>1</li>
  <li>2</li>
  <li>3</li>
</ul>

<ul>
</ul>

<p>
  1,2,3,
</p>

<p>
</p>

<p>
  <span>1</span>
  <span>2</span>
  <span>3</span>
</p>

<p>
 ?: <img src=""1.jpg""/><img src=""2.jpg""/><img src=""3.jpg""/>
</p>

<p><img src=""1.jpg""/><img src=""2.jpg""/><img src=""3.jpg""/></p>

<p>
  <span>abc</span>
</p>

<p>
</p>

<p>
  abc
</p>

<p>
</p>

<p>
  <div>abc</div>
</p>

<p>
  <div>def</div>
</p>
";

            // Ignore differences in line-ending style.
            content = content.Replace("\r\n", "\n").Replace("\r", "\n");
            expected = expected.Replace("\r\n", "\n").Replace("\r", "\n");

            Assert.That(content, Is.EqualTo(expected));
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

            Assert.That(
                content,
                Contains.InOrder(
                    "<p>Bob</p>",
                    "<p>Alpha</p>",
                    "<p>Bob</p>",
                    "<p>Beta</p>",
                    "<p>Bob</p>",
                    "<p>Gamma</p>",
                    "<span class=\"yadda\">Rating: 5</span>"));
        }

        [Test]
        public void TestForEachIndex()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("foreachindex", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<p>0: Alpha</p>",
                    "<p>1: Beta</p>",
                    "<p>2: Gamma</p>",
                    "<p>3: Delta</p>",
                    "<li ",
                    "class='even'>Alpha</li>",
                    "<li ",
                    "class='odd'>Beta</li>",
                    "<li ",
                    "class='even'>Gamma</li>",
                    "<li ",
                    "class='odd'>Delta</li>"));
        }

        [Test]
        public void ForEachMoreAutoVariable()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("foreach-moreautovariables", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString().Replace(" ", "").Replace("\t", "").Replace("\r\n", "");

            Assert.That(content, Contains.InOrder(
                "<tr><td>one</td><td>0</td><td>4</td><td>True</td><td>False</td><td>False</td><td>True</td></tr>",
                "<tr><td>two</td><td>1</td><td>4</td><td>False</td><td>False</td><td>True</td><td>False</td></tr>",
                "<tr><td>three</td><td>2</td><td>4</td><td>False</td><td>False</td><td>False</td><td>True</td></tr>",
                "<tr><td>four</td><td>3</td><td>4</td><td>False</td><td>True</td><td>True</td><td>False</td></tr>"));
        }

        [Test]
        public void ConditionalTestElement()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("testelement", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();
            Assert.That(content, Contains.InOrder(
                            "<p>out-1</p>",
                            "<p>out-2</p>",
                            "<p>out-3</p>",
                            "<p>out-4</p>",
                            "<p>out-5</p>",
                            "<p>out-6</p>"));

            Assert.That(content, Does.Not.Contain("fail"));
        }

        [Test]
        public void ConditionalTestElementNested()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("testelementnested", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString().Replace(" ", "").Replace("\r", "").Replace("\n", "");

            Assert.That(content, Is.EqualTo("<p>a</p><p>b</p><p>c</p><p>d</p><p>e</p><p>f</p>"));
        }

        [Test]
        public void PartialFilesCanHaveSpecialElements()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("partialspecialelements", null, new StubViewData { { "foo", "alpha" } });
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "Hi there, alpha.",
                "Hi there, alpha."));
        }

        [Test]
        public void StatementTerminatingStrings()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("statement-terminating-strings", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString().Replace(" ", "").Replace("\r", "").Replace("\n", "");

            Assert.That(content, Is.EqualTo("<p>a:1</p><p>b:2</p><p>c:3%></p><p>d:<%4%></p><p>e:5%></p><p>f:<%6%></p>"));
        }

        [Test]
        public void ExpressionHasVerbatimStrings()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("expression-has-verbatim-strings", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString().Replace(" ", "").Replace("\r", "").Replace("\n", "");

            Assert.That(content, Is.EqualTo("<p>a\\\"b</p><p>c\\\"}d</p>"));
        }

        [Test]
        public void RelativeApplicationPaths()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("relativeapplicationpaths", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<img src=\"/TestApp/content/images/etc.png\"/>",
                    "<script src=\"/TestApp/content/js/etc.js\"></script>",
                    "<p class=\"~/blah.css\"></p>"));
        }

        [Test]
        public void UseAssembly()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("useassembly", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Contains.InOrder("<p>SortByCategory</p>"));
        }

        [Test]
        public void AddViewDataMoreThanOnce()
        {
            mocks.ReplayAll();
            var viewData = new StubViewData { { "comment", new Comment { Text = "Hello world" } } };
            var viewContext = MakeViewContext("addviewdatamorethanonce", null, viewData);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<div>Hello world</div>",
                "<div>\r\n  Again: Hello world\r\n</div>"));
        }

        [Test]
        public void AddViewDataDifferentTypes()
        {
            mocks.ReplayAll();
            var viewData = new StubViewData { { "comment", new Comment { Text = "Hello world" } } };
            var viewContext = MakeViewContext("addviewdatadifferenttypes", null, viewData);
            Assert.That(() => factory.RenderView(viewContext), Throws.TypeOf<CompilerException>());
            mocks.VerifyAll();
        }

        [Test]
        public void RenderPartialWithContainedContent()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("render-partial-with-contained-content", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "xbox",
                    "xtop",
                    "xb1",
                    "xb2",
                    "xb3",
                    "xb4",
                    "xboxcontent",
                    "Hello World",
                    "xbottom",
                    "xb4",
                    "xb3",
                    "xb2",
                    "xb1"));
        }

        [Test]
        public void RenderPartialWithSegmentContent()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("render-partial-with-segment-content", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "xbox",
                    "xtop",
                    "xb1",
                    "xb2",
                    "xb3",
                    "xb4",
                    "xboxcontent",
                    "title=\"My Tooltip\"",
                    "<h3>This is a test</h3>",
                    "Hello World",
                    "xbottom",
                    "xb4",
                    "xb3",
                    "xb2",
                    "xb1"));
        }

        [Test]
        public void RenderPartialWithSectionAsHtml5ContentByDefault()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("render-partial-section-or-ignore", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "xbox",
                    "xtop",
                    "xb1",
                    "xb2",
                    "xb3",
                    "xb4",
                    "xboxcontent",
                    "<section name=\"top\">",
                    "<h3>This is a test</h3>",
                    "</section>",
                    "<section name=\"tooltip\">",
                    "My Tooltip",
                    "</section>",
                    "<p>Hello World</p>",
                    "xbottom",
                    "xb4",
                    "xb3",
                    "xb2",
                    "xb1"));
        }

        [Test]
        public void RenderPartialWithSectionAsSegmentContentFromSettings()
        {
            settings.SetParseSectionTagAsSegment(true);

            mocks.ReplayAll();
            var viewContext = MakeViewContext("render-partial-section-or-ignore", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "xbox",
                    "xtop",
                    "xb1",
                    "xb2",
                    "xb3",
                    "xb4",
                    "xboxcontent",
                    "title=\"My Tooltip\"",
                    "<h3>This is a test</h3>",
                    "Hello World",
                    "xbottom",
                    "xb4",
                    "xb3",
                    "xb2",
                    "xb1"));
        }

        [Test]
        public void RenderPartialWithDotInName()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("render-dotted-partial", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<p>",
                    "this.is.some.text:",
                    "test456",
                    "</p>"));

            Assert.That(content, Does.Not.Contain("<PartialWith.Dot"));
        }

        [Test]
        public void CaptureContentAsVariable()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("capture-content-as-variable", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "new-var-new-def-set-var"));
        }


        [Test]
        public void CaptureContentBeforeAndAfter()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("capture-content-before-and-after", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<p>beforedataafter</p>"));
        }

        [Test]
        public void ConstAndReadonlyGlobals()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("const-and-readonly-globals", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString().Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");

            Assert.That(content, Is.EqualTo("<ol><li>3</li><li>4</li><li>5</li><li>6</li><li>7</li></ol>"));
        }

        [Test]
        public void PrefixContentNotation()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("prefix-content-notation", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString().Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace(" ", "");

            Assert.That(content, Does.Contain("onetwothree"));
        }

        [Test]
        public void DynamicAttributes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("dynamic-attributes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<div>",
                    @"<elt1 a1=""1"" a3=""3""></elt1>",
                    @"<elt2 a1=""1"" a3=""3""></elt2>",
                    @"<elt3 a1=""1"" a2="""" a3=""3""></elt3>",
                    @"<elt4 a1=""1"" a2=""2"" a3=""3""></elt4>",

                    @"<elt5 a1=""1"" a2="" beta"" a3=""3""></elt5>",
                    @"<elt6 a1=""1"" a2=""alpha beta"" a3=""3""></elt6>",
                    @"<elt7 a1=""1"" a2=""alpha"" a3=""3""></elt7>",
                    @"<elt8 a1=""1"" a3=""3""></elt8>",

                    @"<elt9 a1=""1"" a2="" beta"" a3=""3""></elt9>",
                    @"<elt10 a1=""1"" a2=""alpha beta"" a3=""3""></elt10>",

                    @"<elt11 a1=""1"" a3=""3""></elt11>",
                    @"<elt12 a1=""1"" a2=""onetwo"" a3=""3""></elt12>",
                    "</div>"));
        }

        [Test]
        public void XMLDeclAndProcessingInstruction()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("xmldecl-and-processing-instruction", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<?xml version=\"1.0\" encoding=\"utf-8\" ?>",
                    "<?php yadda yadda yadda ?>"));
        }


        [Test]
        public void ForEachAutovariablesUsedInline()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("foreach-autovariables-used-inline", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<li",
                    "class=",
                    "selected",
                    "blah",
                    "</li>",
                    "blah",
                    "blah"));
        }

        [Test]
        public void AlternateViewdataSyntax()
        {
            mocks.ReplayAll();
            var viewData = new StubViewData<IList<string>> { { "my-data", "alpha" } };
            viewData.Model = new[] { "beta", "gamma", "delta" };

            var viewContext = MakeViewContext("alternate-viewdata-syntax", null, viewData);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<p>",
                    "alpha",
                    "</p>",
                    "<p>",
                    "beta",
                    "</p>",
                    "<p>",
                    "gamma",
                    "</p>",
                    "<p>",
                    "delta",
                    "</p>"));
        }

        [Test]
        public void DefaultValuesDontCollideWithExistingLocals()
        {
            mocks.ReplayAll();

            var viewContext = MakeViewContext("DefaultValuesDontCollideWithExistingLocals", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("default"));

            Assert.That(content, Contains.InOrder("ok1", "ok2"));
            Assert.That(content, Does.Not.Contain("fail"));
        }

        [Test]
        public void DefaultValuesDontReplaceGlobals()
        {
            mocks.ReplayAll();

            var viewContext = MakeViewContext("DefaultValuesDontReplaceGlobals", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("default"));

            Assert.That(content, Contains.InOrder("ok1", "ok2"));
            Assert.That(content, Does.Not.Contain("fail"));
        }

        [Test]
        public void DefaultValuesDontReplaceViewData()
        {
            mocks.ReplayAll();
            var viewData = new StubViewData { { "x1", 5 }, { "x2", 5 } };
            var viewContext = MakeViewContext("DefaultValuesDontReplaceViewData", null, viewData);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("default"));

            Assert.That(content, Contains.InOrder("ok1", "ok2"));
            Assert.That(content, Does.Not.Contain("fail"));
        }


        [Test]
        public void DefaultValuesActAsLocal()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("DefaultValuesActAsLocal", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("default"));

            Assert.That(content, Contains.InOrder("ok1", "ok2"));
            Assert.That(content, Does.Not.Contain("fail"));
        }

        [Test]
        public void DefaultValuesStandInForNullViewData()
        {
            mocks.ReplayAll();
            var viewData = new StubViewData();
            var viewContext = MakeViewContext("DefaultValuesStandInForNullViewData", null, viewData);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("default"));

            Assert.That(content, Contains.InOrder("ok1", "ok2"));
            Assert.That(content, Does.Not.Contain("fail"));
        }

        [Test]
        public void NullExceptionHandledAutomatically()
        {
            mocks.ReplayAll();
            var viewData = new StubViewData();
            var viewContext = MakeViewContext("NullExceptionHandledAutomatically", null, viewData);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("default"));

            Assert.That(content, Contains.InOrder(
                "<p>name kaboom *${user.Name}*</p>",
                "<p>name silently **</p>",
                "<p>name fixed *fred*</p>"));
        }

        [Test]
        public void CodeCommentsCanHaveQuotes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("CodeCommentsCanHaveQuotes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("broken"));
            Assert.That(content, Contains.InOrder("one", "two", "three", "four", "five"));
        }

        [Test]
        public void ConditionalAttributeDelimitedBySpaces()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ConditionalAttributeDelimitedBySpaces", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Does.Not.Contain("broken"));
            Assert.That(content, Contains.InOrder(
                "<h1 class=\"one three\"></h1>",
                "<h2></h2>",
                "<h3 class=\" two three\"></h3>",
                "<h4 class=\"one three\"></h4>",
                "<h5 class=\"one two\"></h5>",
                "<h6></h6>",
                "<h7 class=\"one&two<three\"></h7>"));
        }

        [Test]
        public void OnceAttribute()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("OnceAttribute", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                            "foo1",
                            "bar0",
                            "quux2",
                            "a1"));

            Assert.That(content, Does.Not.Contain("foo2"));
            Assert.That(content, Does.Not.Contain("foo3"));
            Assert.That(content, Does.Not.Contain("bar1"));
            Assert.That(content, Does.Not.Contain("bar3"));
            Assert.That(content, Does.Not.Contain("a2"));
        }

        [Test]
        public void EachAttributeWorksOnSpecialNodes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("EachAttributeWorksOnSpecialNodes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                            "<p>name-0-alpha</p>",
                            "<p>name-1-beta</p>",
                            "<p>name-2-gamma</p>",
                            "<span>one</span>",
                            "<span>two</span>",
                            "<span>three</span>"));
        }

        [Test]
        public void IfAttributeWorksOnSpecialNodes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("IfAttributeWorksOnSpecialNodes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                            "<p>name-0-alpha</p>",
                            "<p>name-2-gamma</p>",
                            "<span>one</span>",
                            "<span>three</span>"));

            Assert.That(content, Does.Not.Contain("beta"));
            Assert.That(content, Does.Not.Contain("two"));
        }

        [Test]
        public void UnlessAttributeWorksOnSpecialNodes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("UnlessAttributeWorksOnSpecialNodes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                            "<p>name-0-alpha</p>",
                            "<p>name-2-gamma</p>",
                            "<span>one</span>",
                            "<span>three</span>"));

            Assert.That(content, Does.Not.Contain("beta"));
            Assert.That(content, Does.Not.Contain("two"));
        }

        [Test]
        public void OnceAttributeWorksOnSpecialNodes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("OnceAttributeWorksOnSpecialNodes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                            "<p>name-0-alpha</p>",
                            "<span>foo1</span>",
                            "<span>bar0</span>",
                            "<span>quux2</span>"));

            Assert.That(content, Does.Not.Contain("name-1"));
            Assert.That(content, Does.Not.Contain("name-2"));
            Assert.That(content, Does.Not.Contain("foo2"));
            Assert.That(content, Does.Not.Contain("foo3"));
            Assert.That(content, Does.Not.Contain("bar1"));
            Assert.That(content, Does.Not.Contain("bar3"));
        }

        [Test]
        public void LateBoundEvalResolvesViewData()
        {
            mocks.ReplayAll();
            var viewData = new StubViewData()
                               {
                                   {"alpha", "<strong>hi</strong>"},
                                   {"beta", "yadda"}
                               };
            var viewContext = MakeViewContext("LateBoundEvalResolvesViewData", null, viewData);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<p><strong>hi</strong></p>",
                    "yadda",
                    "<p>42</p>"));
        }

        [Test]
        public void PartialInMacroMayUseDefaultElement()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("PartialInMacroMayUseDefaultElement", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            Assert.That(
                content,
                Contains.InOrder(
                    "<p>3hello</p>",
                    "<p>2hello.</p>",
                    "<p>1hello..</p>",
                    "<p>0hello...</p>"));
        }

        [Test]
        public void RecursivePartialsThrowCompilerException()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("RecursivePartialsThrowCompilerException", null);
            Assert.That(() =>
                        factory.RenderView(viewContext),
                        Throws.TypeOf<CompilerException>());
        }

        [Test]
        public void NestedPartialsCanBackRenderUpAndReRenderDown()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("NestedPartialsCanBackRenderUpAndReRenderDown", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();

            var stripped = content.Replace(" ", "").Replace("\t", "").Replace("\r\n", "");
            Assert.That(
                stripped,
                Is.EqualTo(
                    "[001][101]" +
                    "[201][102]" +
                    "[201][104][202]" +
                    "[106][002][107]" +
                    "[201][109][202]" +
                    "[111][202]" +
                    "[112][003]"));
        }

        [Test]
        public void SegmentRenderingFallbackMayRenderSegment()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("SegmentRenderingFallbackMayRenderSegment", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();
            string content = sb.ToString();
            var stripped = content.Replace(" ", "").Replace("\t", "").Replace("\r\n", "");
            Assert.That(stripped, Is.EqualTo(
                "[001]" +
                "[101][102]" +
                "[002][004]" +
                "[103][104]" +
                "[005]"));
        }

        [Test]
        public void Markdown()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("markdown", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<pre><code>", "code block on the first line", "</code></pre>",
                "<p>", "Regular text.", "</p>",
                "<pre><code>", "code block indented by spaces", "</code></pre>",
                "<p>", "Regular text.", "</p>",
                "<pre><code>", "the lines in this block",
                "all contain trailing spaces", "</code></pre>",
                "<p>", "Regular Text.", "</p>",
                "<pre><code>", "code block on the last line", "</code></pre>"));
        }

        [Test]
        public void Ignore()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ignore", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<div>",
                "Regular text ${This.isnt.code < 0}",
                "<var dummy=\"This isn't a variable\" />",
                "</div>"));
            Assert.That(content, Does.Not.Contain("<ignore>"));
            Assert.That(content, Does.Not.Contain("</ignore>"));
        }

        [Test]
        public void Escape()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("escape", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<div>",
                "${Encoded.Escaped.with.a.dollar < 0}",
                "${Encoded.Escaped.with.a.backslash < 0}",
                "${Encoded.Escaped.with.a.backtick < 0}",
                "</div>"));

            Assert.That(content, Contains.InOrder(
                "<div>",
                "!{Unencoded.Escaped.with.a.dollar < 0}",
                "!{Unencoded.Escaped.with.a.backslash < 0}",
                "!{Unencoded.Escaped.with.a.backtick < 0}",
                "</div>"));

            Assert.That(content, Contains.InOrder(
                "<div>",
                "$!{Encoded.Silent.Nulls.Escaped.with.a.dollar < 0}",
                "$!{Encoded.Silent.Nulls.Escaped.with.a.backslash < 0}",
                "$!{Encoded.Silent.Nulls.Escaped.with.a.backtick < 0}",
                "</div>"));
        }

        [Test]
        public void PreserveSingleQuotes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("preserveSingleQuotes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Is.EqualTo(@"<img attr='something; other=""value1, value2""'/>"));
        }

        [Test]
        public void PreserveDoubleQuotes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("preserveDoubleQuotes", null);
            factory.RenderView(viewContext);
            mocks.VerifyAll();

            string content = sb.ToString();

            Assert.That(content, Is.EqualTo(@"<img attr=""something; other='value1, value2'""/>"));
        }

        [Test]
        public void ShadeFileRenders()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ShadeFileRenders", null);
            factory.RenderView(viewContext, Constants.DotShade);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<html>",
                "<head>",
                "<title>",
                "offset test",
                "</title>",
                "<body>",
                "<div class=\"container\">",
                "<h1 id=\"top\">",
                "offset test",
                "</h1>",
                "</div>",
                "</body>",
                "</html>"));
        }

        [Test]
        public void ShadeEvaluatesExpressions()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ShadeEvaluatesExpressions", null);
            factory.RenderView(viewContext, Constants.DotShade);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<p>",
                "<span>",
                "8",
                "</span>",
                "<span>",
                "2", " and ", "7",
                "</span>",
                "</p>"));
        }

        [Test]
        public void ShadeSupportsAttributesAndMayTreatSomeElementsAsSpecialNodes()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ShadeSupportsAttributesAndMayTreatSomeElementsAsSpecialNodes", null);
            factory.RenderView(viewContext, Constants.DotShade);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<ul class=\"nav\">",
                "<li>Welcome</li>",
                "<li>to</li>",
                "<li>the</li>",
                "<li>Machine</li>",
                "</ul>",
                "<p>",
                "<span>4</span>",
                "</p>"));
        }

        [Test]
        public void ShadeCodeMayBeDashOrAtBraced()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ShadeCodeMayBeDashOrAtBraced", null);
            factory.RenderView(viewContext, Constants.DotShade);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<ul>",
                "<li>emocleW</li>",
                "<li>ot</li>",
                "<li>eht</li>",
                "<li>enihcaM</li>",
                "</ul>"));
        }

        [Test]
        public void ShadeTextMayContainExpressions()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ShadeTextMayContainExpressions", null);
            factory.RenderView(viewContext, Constants.DotShade);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<p>",
                "<span>8</span>",
                "<span>2 and 7</span>",
                "</p>"));
        }

        [Test]
        public void TextOrientedAttributesApplyToVarAndSet()
        {
            mocks.ReplayAll();
            ((SparkSettings)engine.Settings).AttributeBehaviour = AttributeBehaviour.TextOriented;
            var viewContext = MakeViewContext("TextOrientedAttributesApplyToVarAndSet", null);
            factory.RenderView(viewContext, Constants.DotSpark);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<p>String:HelloWorld</p>",
                "<p>Int32:42</p>"));
        }

        [Test]
        public void TextOrientedAttributesApplyToUseFile()
        {
            mocks.ReplayAll();
            ((SparkSettings)engine.Settings).AttributeBehaviour = AttributeBehaviour.TextOriented;
            var viewContext = MakeViewContext("TextOrientedAttributesApplyToUseFile", null);
            factory.RenderView(viewContext, Constants.DotSpark);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<span>Hello</span>",
                "<span>Hello World!</span>",
                "<span>42</span>"));
        }

        [Test]
        public void TextOrientedAttributesApplyToDefault()
        {
            mocks.ReplayAll();
            ((SparkSettings)engine.Settings).AttributeBehaviour = AttributeBehaviour.TextOriented;
            var viewContext = MakeViewContext("TextOrientedAttributesApplyToDefault", null);
            factory.RenderView(viewContext, Constants.DotSpark);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<p>String:World</p>",
                "<p>String:Hello World!</p>",
                "<p>Int32:42</p>"));
        }

        [Test]
        public void TextOrientedAttributesApplyToGlobal()
        {
            mocks.ReplayAll();
            ((SparkSettings)engine.Settings).AttributeBehaviour = AttributeBehaviour.TextOriented;
            var viewContext = MakeViewContext("TextOrientedAttributesApplyToGlobal", null);
            factory.RenderView(viewContext, Constants.DotSpark);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<p>String:World</p>",
                "<p>String:Hello World!</p>",
                "<p>Int32:42</p>"));
        }

        [Test]
        public void ShadeElementsMayStackOnOneLine()
        {
            mocks.ReplayAll();
            var viewContext = MakeViewContext("ShadeElementsMayStackOnOneLine", null);
            factory.RenderView(viewContext, Constants.DotShade);
            mocks.VerifyAll();

            var content = sb.ToString();

            Assert.That(content, Contains.InOrder(
                "<html>",
                "<head>",
                "<title>",
                "offset test",
                "</title>",
                "<body>",
                "<div class=\"container\">",
                "<h1 id=\"top\">",
                "offset test",
                "</h1>",
                "</div>",
                "</body>",
                "</html>"));
        }
    }
}
