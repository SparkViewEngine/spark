using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using Spark.Tests.Models;
using Spark.Tests.Stubs;
using System.IO;

namespace Spark.Tests
{
    [TestFixture]
    public class VisualBasicViewTester
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
                        .SetDefaultLanguage(LanguageType.VisualBasic)
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
            var context = new StubViewContext() { ControllerName = "vbhome", ViewName = viewName, Output = new StringBuilder(), Data = viewData };
            _factory.RenderView(context);
            return context.Output.ToString();
        }



        [Test]
        public void CompileAndRunVisualBasicView()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), "Hello world");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo("Hello world"));
        }

        [Test]
        public void ShouldWriteTabAndCrlf()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), "Hello\r\n\tworld");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo("Hello\r\n\tworld"));
        }

        [Test]
        public void CodeStatementChunks()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
#Dim foo = 'hi there'
<%Dim bar = 'hello again'%>
${foo} ${bar}");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("hi there hello again"));
        }

        [Test]
        public void GlobalVariableChunks()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<global foo='5' bar=""'hello'""/>
${foo} ${bar}
#bar='there'
${bar}
");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("5 hello\r\nthere"));
        }

        [Test]
        public void TypedGlobalVariableChunks()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<global bar=""'hello'"" type=""String""/>
${bar} ${bar.Length}
");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("hello 5"));
        }

        [Test]
        public void LocalVariableChunks()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<var foo='5'/>
<var bar=""'hello'"" type='String'/>
${foo} ${bar} ${bar.Length}
");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("5 hello 5"));
        }

        [Test]
        public void DefaultValuesDontCollideWithExistingLocals()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<var x1=""5""/>

<default x1=""3""/>

<p if=""x1=5"">ok1</p>
<else>fail</else>

<var x2=""5"">
  <default x2=""3"">
    <p if=""x2=5"">ok2</p>
    <else>fail</else>
  </default>
</var>

");
            var contents = Render("index");

            Assert.That(contents, Text.Contains("ok1"));
            Assert.That(contents, Text.Contains("ok2"));
            Assert.That(contents, Text.DoesNotContain("fail"));
        }

        [Test]
        public void DefaultValuesDontReplaceGlobals()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<default x1=""3""/>

<p if=""x1=5"">ok1</p>
<else>fail</else>

<default x2=""3"">
  <p if=""x2=5"">ok2</p>
  <else>fail</else>
</default>

<global x1=""5"" x2=""5"" type=""Integer""/>

");
            var contents = Render("index");

            Assert.That(contents, Text.Contains("ok1"));
            Assert.That(contents, Text.Contains("ok2"));
            Assert.That(contents, Text.DoesNotContain("fail"));
        }


        [Test]
        public void DefaultValuesDontReplaceViewData()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<default x1=""3""/>

<p if=""x1=5"">ok1</p>
<else>fail</else>

<default x2=""3"">
  <p if=""x2=5"">ok2</p>
  <else>fail</else>
</default>

<viewdata x1=""Integer"" x2=""Integer"" />

");
            var contents = Render("index", new StubViewData { { "x1", 5 }, { "x2", 5 } });

            Assert.That(contents, Text.Contains("ok1"));
            Assert.That(contents, Text.Contains("ok2"));
            Assert.That(contents, Text.DoesNotContain("fail"));
        }


        [Test]
        public void DefaultValuesActAsLocal()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<default x1=""3""/>

<p if=""x1=3"">ok1</p>
<else>fail</else>

<default x2=""3"">
  <p if=""x2=3"">ok2</p>
  <else>fail</else>
</default>

");
            var contents = Render("index");

            Assert.That(contents, Text.Contains("ok1"));
            Assert.That(contents, Text.Contains("ok2"));
            Assert.That(contents, Text.DoesNotContain("fail"));
        }

        [Test]
        public void DefaultValuesStandInForNullViewData()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<p if=""x1=7"">ok1</p>
<else>fail</else>

<default x2=""3"">
  <p if=""x2=7"">ok2</p>
  <else>fail</else>
</default>

<default x1=""3""/>

<viewdata x1=""Integer"" x2=""Integer"" default=""7""/>

");
            var contents = Render("index");

            Assert.That(contents, Text.Contains("ok1"));
            Assert.That(contents, Text.Contains("ok2"));
            Assert.That(contents, Text.DoesNotContain("fail"));
        }

        [Test]
        public void ViewDataChunks()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<viewdata x1=""Integer"" x2=""String"" />
${x1} ${x2}
");
            var contents = Render("index", new StubViewData { { "x1", 4 }, { "x2", "hello" } });
            Assert.That(contents.Trim(), Is.EqualTo("4 hello"));
        }

        [Test]
        public void ViewDataModelChunk()
        {

            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<viewdata model=""Spark.Tests.Models.Comment Comment"" />
${Comment.Text}
");
            var comment = new Comment { Text = "hello world" };
            var contents = Render("index", new StubViewData<Comment> { Model = comment });
            Assert.That(contents.Trim(), Is.EqualTo("hello world"));
        }

        [Test]
        public void AssignChunk()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
#Dim x = 4
<set x='5'/>
${x}
");
            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("5"));
        }

        [Test]
        public void ContentNameAndUseContentChunk()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<content name='foo'>bar</content>
[<use content='foo'/>]
");
            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("[bar]"));            
        }

        [Test]
        public void RenderPartialChunk()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"[<foo/>]");
            _viewFolder.Add(Path.Combine("shared", "_foo.spark"), @"bar");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("[bar]"));    
        }

        [Test]
        public void ContentVarChunk()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<content var='foo'>bar</content>
[${foo}]
");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("[bar]"));
        }

        [Test]
        public void ContentSetChunk()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<var foo='""frap""'/>
<content set='foo' add='replace'>fred</content>
<content set='foo' add='before'>bar</content>
<content set='foo' add='after'>quad</content>
[${foo}]
");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("[barfredquad]"));
        }

        [Test]
        public void ConditionalAttributes()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"<div class=""""/>
<div class=""foo""/>

<div class=""foo?{True}""/>
<div class=""foo?{False}""/>

<div class=""foo?{True} bar?{True}""/>
<div class=""foo?{True} bar?{False}""/>
<div class=""foo?{False} bar?{True}""/>
<div class=""foo?{False} bar?{False}""/>");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo(@"<div class=""""/>
<div class=""foo""/>

<div class=""foo""/>
<div/>

<div class=""foo bar""/>
<div class=""foo""/>
<div class="" bar""/>
<div/>"));
        }

        [Test]
        public void MacroChunks()
        {
            _viewFolder.Add(Path.Combine("vbhome", "index.spark"), @"
<macro name='foo'>bar</macro>
<macro name='foo2' quux='String'>bar2${quux}bar3</macro>
${foo2('alpha')} ${foo}
");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("bar2alphabar3 bar"));
        }
    }
}
