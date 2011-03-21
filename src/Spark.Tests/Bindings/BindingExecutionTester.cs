using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using Spark.Tests.Stubs;
using System.IO;

namespace Spark.Tests.Bindings
{
    [TestFixture]
    public class BindingExecutionTester
    {

        private InMemoryViewFolder _viewFolder;
        private StubViewFactory _factory;

        [SetUp]
        public void Init()
        {
            _viewFolder = new InMemoryViewFolder();

            _factory = new StubViewFactory
            {
                Engine = new SparkViewEngine(
                    new SparkSettings()
                        .SetPageBaseType(typeof(StubSparkView)))
                {
                    ViewFolder = _viewFolder
                }
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
        public void ElementReplacedWithSimpleString()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>""world""</element></bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello/></p>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>world</p>"));
        }


        [Test]
        public void ElementReplacedWithMacroCall()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World()</element></bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello/></p><macro name='World'>success</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success</p>"));
        }

        [Test]
        public void ElementReplacedWithMacroCallAndAnArgument()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='alpha'/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success alpha!</p>"));
        }

        [Test]
        public void BindingRefersToAttributeWithCode()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='${3+4}'/></p><macro name='World' beta='int'>success ${beta}!</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success 7!</p>"));
        }


        [Test]
        public void BindingRefersToAttributeWithMixedCodeAndText()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one ${3+4} two ${SiteRoot} three'/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success one 7 two /TestApp three!</p>"));
        }


        [Test]
        public void BindingRefersToAttributeWithUnescapedCode() {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World(@foo)</element></bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo=""'one '+(3+4)+' two '+SiteRoot+' three'""/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success one 7 two /TestApp three!</p>"));
        }

        [Test]
        public void CorrectBindingUsedBasedOnAttributesPresent()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>""foo is "" + '@foo' + ""!""</element>
<element name='hello'>""bar is "" + '@bar' + ""!""</element>
</bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello bar='two'/></p>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>foo is one!, bar is two!</p>"));
        }

        [Test]
        public void WildcardBindAsObjectInitializer()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>Callback(new{'@*'})</element>
</bindings>");
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello bar='two'/>, <hello foo='-${SiteRoot}-' bar='four'/></p><viewdata Callback='System.Func[[object,string]]'/>");

            Func<object, string> cb = x => x.ToString();
            var contents = Render("index", new StubViewData { { "Callback", cb } });

            // default to anon object's ToString() style
            Assert.That(contents, Is.EqualTo(@"<p>{ foo = one }, { bar = two }, { foo = -/TestApp-, bar = four }</p>"));
        }

        [Test]
        public void NamedReferencesAreNotUsedByWildcardReferences()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>Callback('@foo', new{'@*'})</element>
<element name='hello'>Callback(""nada"", new{'@*'})</element>
</bindings>");

            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello bar='two'/>, <hello foo='-${SiteRoot}-' bar='four'/></p><viewdata Callback='System.Func[[string,object,string]]'/>");

            Func<string, object, string> cb = (a, x) => '[' + a + ']' + x.ToString();
            var contents = Render("index", new StubViewData { { "Callback", cb } });

            // default to anon object's ToString() style
            Assert.That(contents, Is.EqualTo(@"<p>[one]{ }, [nada]{ bar = two }, [-/TestApp-]{ bar = four }</p>"));
        }

        [Test]
        public void WildcardReferencesWillNotUseElementsMatchedByLongerPrefix()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>Callback('@foo', new{'@*'}, new{'@route.*'})</element>
</bindings>");

            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello foo='one'/>, <hello foo='one' bar='two'/>, <hello foo='one' bar='${2}' route.id='three' /></p><viewdata Callback='System.Func[[string,object,object,string]]'/>");

            Func<string, object, object, string> cb = (a, x, y) => '[' + a + ']' + x.ToString() + y.ToString();
            var contents = Render("index", new StubViewData { { "Callback", cb } });

            // default to anon object's ToString() style
            Assert.That(contents, Is.EqualTo(@"<p>[one]{ }{ }, [one]{ bar = two }{ }, [one]{ bar = 2 }{ id = three }</p>"));
        }

        [Test]
        public void StatementPhraseWillBeExecutedInsteadOfOutput()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>#Output.Write(4+5);</element>
</bindings>");

            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello/></p>");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>9</p>"));
        }

        [Test]
        public void TwoPhraseBindingMayWrapOtherMaterial()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'><start>@a</start><end>@b</end></element>
</bindings>");

            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='3' b='5'>world</hello></p>");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>3world5</p>"));
        }

        [Test]
        public void ChildReferenceWillSpoolAndProvideContentAsString()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>'@a' + 'child::*' + '@b'</element>
</bindings>");

            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='3' b='5'>world</hello></p>");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>3world5</p>"));
        }
        
        [Test, Ignore("The child::* is always treated as text. So this test does not represent the current capabilities.")]
        public void ChildReferenceWillSpoolAndProvideContentAsCode() {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>'@a' + child::* + '@b'</element>
</bindings>");

            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='3' b='5'>(8+7)+""4""${55}</hello></p>");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>31545</p>"));
        }

        [Test]
        public void ChildReferenceWillNotMatchSelfClosingElements()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>'@a' + 'child::*' + '@b'</element>
<element name='hello'>'@a' + ""no text"" + '@b'</element>
</bindings>");

            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='1' b='2'>world</hello><hello a='3' b='4'></hello><hello a='5' b='6'/></p>");

            var contents = Render("index");

            Assert.That(contents, Is.EqualTo(@"<p>1world2345no text6</p>"));
        }
        
        [Test]
        public void CurleyBracesExpandAsDictionaryInitialization()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>new System.Collections.Generic.Dictionary&lt;string,object&gt;{{'@*'}}.Count</element>
</bindings>");
            
            _viewFolder.Add(Path.Combine("home", "index.spark"), @"<p><hello a='foo' b='bar'/><hello/><hello></hello><hello x1='' x2='' x3='' x4='' x5=''/></p>");

            var contents = Render("index");

            Assert.That(contents, Is.EqualTo(@"<p>2005</p>"));
        }
    }
}
