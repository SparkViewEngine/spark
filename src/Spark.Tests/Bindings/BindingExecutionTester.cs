using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using Spark.Tests.Stubs;

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
            _viewFolder.Add("home\\index.spark", @"<p><hello/></p>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>world</p>"));
        }


        [Test]
        public void ElementReplacedWithMacroCall()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World()</element></bindings>");
            _viewFolder.Add("home\\index.spark", @"<p><hello/></p><macro name='World'>success</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success</p>"));
        }

        [Test]
        public void ElementReplacedWithMacroCallAndAnArgument()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            _viewFolder.Add("home\\index.spark", @"<p><hello foo='alpha'/></p><macro name='World' beta='string'>success ${beta}!</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success alpha!</p>"));
        }

        [Test]
        public void BindingRefersToAttributeWithCode()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            _viewFolder.Add("home\\index.spark", @"<p><hello foo='${3+4}'/></p><macro name='World' beta='int'>success ${beta}!</macro>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>success 7!</p>"));
        }


        [Test]
        public void BindingRefersToAttributeWithMixedCodeAndText()
        {
            _viewFolder.Add("bindings.xml", @"<bindings><element name='hello'>World('@foo')</element></bindings>");
            _viewFolder.Add("home\\index.spark", @"<p><hello foo='one ${3+4} two ${SiteRoot} three'/></p><macro name='World' beta='string'>success ${beta}!</macro>");

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
            _viewFolder.Add("home\\index.spark", @"<p><hello foo='one'/>, <hello bar='two'/></p>");

            var contents = Render("index");
            Assert.That(contents, Is.EqualTo(@"<p>foo is one!, bar is two!</p>"));
        }

        [Test]
        public void WildcardBindAsObjectInitializer()
        {
            _viewFolder.Add("bindings.xml", @"<bindings>
<element name='hello'>Callback(new{'@*'})</element>
</bindings>");
            _viewFolder.Add("home\\index.spark", @"<p><hello foo='one'/>, <hello bar='two'/>, <hello foo='-${SiteRoot}-' bar='four'/></p><viewdata Callback='System.Func[[object,string]]'/>");

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

            _viewFolder.Add("home\\index.spark", @"<p><hello foo='one'/>, <hello bar='two'/>, <hello foo='-${SiteRoot}-' bar='four'/></p><viewdata Callback='System.Func[[string,object,string]]'/>");

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

            _viewFolder.Add("home\\index.spark", @"<p><hello foo='one'/>, <hello foo='one' bar='two'/>, <hello foo='one' bar='${2}' route.id='three' /></p><viewdata Callback='System.Func[[string,object,object,string]]'/>");

            Func<string, object, object, string> cb = (a, x, y) => '[' + a + ']' + x.ToString() + y.ToString();
            var contents = Render("index", new StubViewData { { "Callback", cb } });

            // default to anon object's ToString() style
            Assert.That(contents, Is.EqualTo(@"<p>[one]{ }{ }, [one]{ bar = two }{ }, [one]{ bar = 2 }{ id = three }</p>"));
        }
    }
}
