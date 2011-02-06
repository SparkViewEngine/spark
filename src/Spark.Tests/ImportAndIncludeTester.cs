// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class ImportAndIncludeTester
    {
        private ISparkView CreateView(IViewFolder viewFolder, string template)
        {
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
                                     {string.Format("importing{0}index.spark", Path.DirectorySeparatorChar), "<p><use import='extra.spark'/>hello ${name}</p>"},
                                     {string.Format("importing{0}extra.spark", Path.DirectorySeparatorChar), "this is imported <global name='\"world\"'/>"}
                                 }, string.Format("importing{0}index.spark", Path.DirectorySeparatorChar));

            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }

        [Test]
        public void ImportExplicitFileFromShared()
        {
            var view = CreateView(new InMemoryViewFolder
                                 {
                                     {string.Format("importing{0}index.spark", Path.DirectorySeparatorChar), "<p><use import='extra.spark'/>hello ${name}</p>"},
                                     {string.Format("shared{0}extra.spark", Path.DirectorySeparatorChar), "this is imported <global name='\"world\"'/>"}
                                 }, string.Format("importing{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }

        [Test]
        public void ImportExplicitWithoutExtension()
        {
            var view = CreateView(new InMemoryViewFolder
                                 {
                                     {string.Format("importing{0}index.spark", Path.DirectorySeparatorChar), "<p>${foo()} ${name}</p><use import='extra'/><use import='another'/>"},
                                     {string.Format("importing{0}another.spark", Path.DirectorySeparatorChar), "<macro name='foo'>hello</macro>"},
                                     {string.Format("shared{0}extra.spark", Path.DirectorySeparatorChar), "this is imported <global name='\"world\"'/>"}
                                 }, string.Format("importing{0}index.spark", Path.DirectorySeparatorChar));

            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }

        [Test]
        public void ImportImplicit()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("importing{0}index.spark", Path.DirectorySeparatorChar), "<p>${foo()} ${name}</p>"},
                                          {string.Format("importing{0}_global.spark", Path.DirectorySeparatorChar), "<macro name='foo'>hello</macro>"},
                                          {string.Format("shared{0}_global.spark", Path.DirectorySeparatorChar), "this is imported <global name='\"world\"'/>"}
                                      }, string.Format("importing{0}index.spark", Path.DirectorySeparatorChar));

            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
            Assert.IsFalse(contents.Contains("import"));
        }


        [Test]
        public void IncludeFile()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='stuff.spark'/></p>"},
                                          {string.Format("including{0}stuff.spark", Path.DirectorySeparatorChar), "hello world"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
        }

        [Test, ExpectedException(typeof(CompilerException))]
        public void MissingFileThrowsException()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='stuff.spark'/></p>"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            view.RenderView();
        }


        [Test]
        public void MissingFileWithEmptyFallbackIsBlank()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='stuff.spark'><fallback/></include></p>"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p></p>", contents);
        }

        [Test]
        public void MissingFileWithFallbackUsesContents()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='stuff.spark'><fallback>hello world</fallback></include></p>"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>hello world</p>", contents);
        }

        [Test]
        public void ValidIncludeFallbackDisappears()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='stuff.spark'><fallback>hello world</fallback></include></p>"},
                                          {string.Format("including{0}stuff.spark", Path.DirectorySeparatorChar), "another file"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>another file</p>", contents);
        }

        [Test]
        public void FallbackContainsAnotherInclude()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='stuff.spark'><fallback><include href='other.spark'/></fallback></include></p>"},
                                          {string.Format("including{0}other.spark", Path.DirectorySeparatorChar), "other file"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>other file</p>", contents);
        }

        [Test]
        public void IncludeRelativePath()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='../lib/other.spark'/></p>"},
                                          {string.Format("lib{0}other.spark", Path.DirectorySeparatorChar), "other file"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>other file</p>", contents);
        }
        [Test]
        public void IncludeInsideAnInclude()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='../lib/other.spark'/></p>"},
                                          {string.Format("lib{0}other.spark", Path.DirectorySeparatorChar), "other <include href='third.spark'/> file"},
                                          {string.Format("lib{0}third.spark", Path.DirectorySeparatorChar), "third file"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>other third file file</p>", contents);
        }

        [Test]
        public void UsingXmlns()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p xmlns:x='http://www.w3.org/2001/XInclude'><include/><x:include href='../lib/other.spark'/></p>"},
                                          {string.Format("lib{0}other.spark", Path.DirectorySeparatorChar), "other file"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p xmlns:x=\"http://www.w3.org/2001/XInclude\"><include/>other file</p>", contents);
        }

        [Test]
        public void IncludingAsText()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='item.spark' parse='text'/></p>"},
                                          {string.Format("including{0}item.spark", Path.DirectorySeparatorChar), "<li>at&t</li>"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p>&lt;li&gt;at&amp;t&lt;/li&gt;</p>", contents);
        }

        [Test]
        public void IncludingAsHtmlWithDollar()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {string.Format("including{0}index.spark", Path.DirectorySeparatorChar), "<p><include href='jquery.templ.htm' parse='html'/></p>"},
                                          {string.Format("including{0}jquery.templ.htm", Path.DirectorySeparatorChar), "<h4>${Title}</h4>"}
                                      }, string.Format("including{0}index.spark", Path.DirectorySeparatorChar));
            var contents = view.RenderView();
            Assert.AreEqual("<p><h4>${Title}</h4></p>", contents);
        }
    }
}
