using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class ImportAndIncludeTester
    {
        private ISparkView CreateView(IViewFolder viewFolder, string template)
        {
            CompiledViewHolder.Current = new CompiledViewHolder();

            var settings = new SparkSettings().SetPageBaseType(typeof(StubSparkView));

            var engine = new SparkViewEngine(settings)
                             {
                                 ViewFolder = viewFolder
                             };

            return engine.CreateInstance(new SparkViewDescriptor().AddTemplate(template));
        }

        [Test]
        public void ImportExplicitFile()
        {
            var view = CreateView(new InMemoryViewFolder
                                 {
                                     {"importing\\index.spark", "<p><use import='extra.spark'/>hello ${name}</p>"},
                                     {"importing\\extra.spark", "this is imported <global name='\"world\"'/>"}
                                 }, "importing\\index.spark");

            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }

        [Test]
        public void ImportExplicitFileFromShared()
        {
            var view = CreateView(new InMemoryViewFolder
                                 {
                                     {"importing\\index.spark", "<p><use import='extra.spark'/>hello ${name}</p>"},
                                     {"shared\\extra.spark", "this is imported <global name='\"world\"'/>"}
                                 }, "importing\\index.spark");
            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }

        [Test]
        public void ImportExplicitWithoutExtension()
        {
            var view = CreateView(new InMemoryViewFolder
                                 {
                                     {"importing\\index.spark", "<p>${foo()} ${name}</p><use import='extra'/><use import='another'/>"},
                                     {"importing\\another.spark", "<macro name='foo'>hello</macro>"},
                                     {"shared\\extra.spark", "this is imported <global name='\"world\"'/>"}
                                 }, "importing\\index.spark");

            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }

        [Test]
        public void ImportImplicit()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {"importing\\index.spark", "<p>${foo()} ${name}</p>"},
                                          {"importing\\_global.spark", "<macro name='foo'>hello</macro>"},
                                          {"shared\\_global.spark", "this is imported <global name='\"world\"'/>"}
                                      }, "importing\\index.spark");

            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }


        [Test, Ignore("Not impl yet")]
        public void IncludeFile()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {"including\\index.spark", "<p><include href='stuff.spark'/></p>"},
                                          {"including\\stuff.spark", "<macro name='foo'>hello</macro>"}
                                      }, "including\\index.spark");
            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("include"));

        }
    }
}
