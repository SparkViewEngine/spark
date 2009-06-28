using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.FileSystem;
using Spark.Tests.Stubs;

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
            var context = new StubViewContext() { ControllerName = "vbhome", ViewName = "index", Output = new StringBuilder() };
            _factory.RenderView(context);
            return context.Output.ToString();
        }

        [Test]
        public void CompileAndRunVisualBasicView()
        {
            _viewFolder.Add("vbhome\\index.spark", "Hello world");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo("Hello world"));
        }

        [Test]
        public void ShouldWriteTabAndCrlf()
        {
            _viewFolder.Add("vbhome\\index.spark", "Hello\r\n\tworld");
            var contents = Render("index");
            Assert.That(contents, Is.EqualTo("Hello\r\n\tworld"));
        }

        [Test]
        public void CodeStatementChunks()
        {
            _viewFolder.Add("vbhome\\index.spark", @"
#Dim foo = 'hi there'
<%Dim bar = 'hello again'%>
${foo} ${bar}");

            var contents = Render("index");
            Assert.That(contents.Trim(), Is.EqualTo("hi there hello again"));
        }

    }
}
