using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Extensions;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Bindings
{
    [TestFixture]
    public class BindingExecutionTester
    {
        private InMemoryViewFolder _viewFolder;
        private StubViewFactory _factory;

        [SetUp]
        public void Init()
        {
            var settings = new SparkSettings().SetPageBaseType(typeof(StubSparkView));

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder, InMemoryViewFolder>()
                .BuildServiceProvider();

            this._viewFolder = (InMemoryViewFolder)sp.GetService<IViewFolder>();

            var viewEngine = sp.GetService<ISparkViewEngine>();

            this._factory = new StubViewFactory
            {
                Engine = viewEngine
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
        public void ElementReplacedWithSimpleString()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>""world""</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello/></p>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>world</p>"));
        }

        [Test]
        public void ElementReplacedWithMacroCall()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World()</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello/></p><macro name='World'>success</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success</p>"));
        }

        [Test]
        public void ElementReplacedWithMacroCallAndAnArgument()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='alpha'/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success alpha!</p>"));
        }

        [Test]
        public void BindingRefersToAttributeWithCode()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='${3+4}'/></p><macro name='World' beta='int'>success ${beta}!</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success 7!</p>"));
        }


        [Test]
        public void BindingRefersToAttributeWithMixedCodeAndText()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one ${3+4} two ${SiteRoot} three'/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success one 7 two /TestApp three!</p>"));
        }

        [Test]
        public void BindingRefersToAttributeWithMixedCodeAndTextWithOptional()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@@foo')</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one ${3+4} two ${SiteRoot} three'/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success one 7 two /TestApp three!</p>"));
        }

        [Test]
        public void BindingRefersToAttributeWithMixedCodeAndTextWithOptionalNotSupplied()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@@foo')</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello /></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success !</p>"));
        }

        [Test]
        public void BindingRefersToAttributeWithMixedCodeAndTextWithAttributeNotSupplied()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello /></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p><hello></hello></p>"));
        }

        [Test]
        public void BindingRefersToAttributeWithUnescapedCode() {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World(@foo)</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo=""'one '+(3+4)+' two '+SiteRoot+' three'""/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success one 7 two /TestApp three!</p>"));
        }

        [Test]
        public void CorrectBindingUsedBasedOnAttributesPresent()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>""foo is "" + '@foo' + ""!""</element>
<element name='hello'>""bar is "" + '@bar' + ""!""</element>
</bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello bar='two'/></p>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>foo is one!, bar is two!</p>"));
        }

        [Test]
        public void WildcardBindAsObjectInitializer()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>Callback(new{'@*'})</element>
</bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello bar='two'/>, <hello foo='-${SiteRoot}-' bar='four'/></p><viewdata Callback='System.Func[[object,string]]'/>");

            Func<object, string> cb = x => x.ToString();
            var contents = this.Render("index", new StubViewData { { "Callback", cb } });

            // default to anon object's ToString() style
            Assert.That(contents, Is.EqualTo(@"<p>{ foo = one }, { bar = two }, { foo = -/TestApp-, bar = four }</p>"));
        }

        [Test]
        public void NamedReferencesAreNotUsedByWildcardReferences()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>Callback('@foo', new{'@*'})</element>
<element name='hello'>Callback(""nada"", new{'@*'})</element>
</bindings>");

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello bar='two'/>, <hello foo='-${SiteRoot}-' bar='four'/></p><viewdata Callback='System.Func[[string,object,string]]'/>");

            Func<string, object, string> cb = (a, x) => '[' + a + ']' + x.ToString();
            var contents = this.Render("index", new StubViewData { { "Callback", cb } });

            // default to anon object's ToString() style
            Assert.That(contents, Is.EqualTo(@"<p>[one]{ }, [nada]{ bar = two }, [-/TestApp-]{ bar = four }</p>"));
        }

        [Test]
        public void WildcardReferencesWillNotUseElementsMatchedByLongerPrefix()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>Callback('@foo', new{'@*'}, new{'@route.*'})</element>
</bindings>");

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello foo='one' bar='two'/>, <hello foo='one' bar='${2}' route.id='three' /></p><viewdata Callback='System.Func[[string,object,object,string]]'/>");

            Func<string, object, object, string> cb = (a, x, y) => '[' + a + ']' + x.ToString() + y.ToString();
            var contents = this.Render("index", new StubViewData { { "Callback", cb } });

            // default to anon object's ToString() style
            Assert.That(contents, Is.EqualTo(@"<p>[one]{ }{ }, [one]{ bar = two }{ }, [one]{ bar = 2 }{ id = three }</p>"));
        }

        [Test]
        public void StatementPhraseWillBeExecutedInsteadOfOutput()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>#Output.Write(4+5);</element>
</bindings>");

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello/></p>");
            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>9</p>"));
        }

        [Test]
        public void TwoPhraseBindingMayWrapOtherMaterial()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'><start>@a</start><end>@b</end></element>
</bindings>");

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='3' b='5'>world</hello></p>");
            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>3world5</p>"));
        }

        [Test]
        public void ChildReferenceWillSpoolAndProvideContentAsString()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>'@a' + 'child::*' + '@b'</element>
</bindings>");

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='3' b='5'>world</hello></p>");
            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>3world5</p>"));
        }
        
        [Test, Ignore("The child::* is always treated as text. So this test does not represent the current capabilities.")]
        public void ChildReferenceWillSpoolAndProvideContentAsCode() {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>'@a' + child::* + '@b'</element>
</bindings>");

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='3' b='5'>(8+7)+""4""${55}</hello></p>");
            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>31545</p>"));
        }

        [Test]
        public void ChildReferenceWillNotMatchSelfClosingElements()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>'@a' + 'child::*' + '@b'</element>
<element name='hello'>'@a' + ""no text"" + '@b'</element>
</bindings>");

            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='1' b='2'>world</hello><hello a='3' b='4'></hello><hello a='5' b='6'/></p>");

            var contents = this.Render("index");

            Assert.That(contents, Is.EqualTo(@"<p>1world2345no text6</p>"));
        }
        
        [Test]
        public void CurleyBracesExpandAsDictionaryInitialization()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>new System.Collections.Generic.Dictionary&lt;string,object&gt;{{'@*'}}.Count</element>
</bindings>");
            
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='foo' b='bar'/><hello/><hello></hello><hello x1='' x2='' x3='' x4='' x5=''/></p>");

            var contents = this.Render("index");

            Assert.That(contents, Is.EqualTo(@"<p>2005</p>"));
        }

        [Test]
        public void ExpressionNextToBindingShouldMaintainWhiteSpace()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='Text'>child::*</element></bindings>");
            this._viewFolder.Add(Path.Combine("home","index.spark"), @"<p>${'1234'}  <Text>Smith St</Text></p>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>1234  Smith St</p>"));
        }

        [Test]
        public void ExpressionNextToBindingShouldMaintainWhiteSpaceWithLoops()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='Text'>child::*</element></bindings>");
            this._viewFolder.Add(Path.Combine("home","index.spark"), @"<var names=""new [] {'alpha', 'beta', 'gamma'}""/>
<ul>
<for each=""var name in names"">
    <li>${name} <Text>is</Text> okay too I suppose. </li>
</for>
</ul>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"
<ul>
    <li>alpha is okay too I suppose. </li>
    <li>beta is okay too I suppose. </li>
    <li>gamma is okay too I suppose. </li>
</ul>"));
        }

        [Test]
        public void ExpressionNextToBindingShouldMaintainWhiteSpaceWithLoopsInternal()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='Text'>child::*</element></bindings>");
            this._viewFolder.Add(Path.Combine("home","index.spark"), @"<var names=""new [] {'alpha', 'beta', 'gamma'}""/>
<ul>
    <li each=""var name in names"">${name} <Text>is</Text> okay too I suppose. </li>
</ul>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"
<ul>
    <li>alpha is okay too I suppose. </li>
    <li>beta is okay too I suppose. </li>
    <li>gamma is okay too I suppose. </li>
</ul>"));
        }

        [Test]
        public void BindingShouldMaintainNewLine()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='Text'>child::*</element></bindings>");
            this._viewFolder.Add(
                Path.Combine("home", "index.spark"),
                @"
<p>
    <Text>John St</Text>
</p>");

            var contents = this.Render("index");
            Assert.That(
                contents,
                Is.EqualTo(
                    @"
<p>
    John St
</p>"));
        }

        [Test]
        public void BindingNextToBindingShouldMaintainNewLine()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='Text'>child::*</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<p>
    <Text>John St</Text>
    <Text>Smith St</Text>
</p>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"
<p>
    John St
    Smith St
</p>"));
        }

        [Test]
        public void BindingShouldMaintainNewLineWithNoChild()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='Text'>'@tt'</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<p>
    <Text tt=""John St"" />
</p>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"
<p>
    John St
</p>"));
        }

        [Test]
        public void BindingShouldMaintainNewLineWithNoChildAndCode()
        {
            this._viewFolder.Add("bindings.xml", @"<bindings><element name='Text'>'@tt'</element></bindings>");
            this._viewFolder.Add(Path.Combine("home", "index.spark"), @"
<p>
    <var t=""23"" />
    <Text tt=""${t}"" />
</p>");

            var contents = this.Render("index");
            Assert.That(contents, Is.EqualTo(@"
<p>
    23
</p>"));
        }
    }
}
