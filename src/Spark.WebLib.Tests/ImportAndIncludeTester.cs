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
// 

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Spark.Compiler;
using Spark.Extensions;
using Spark.FileSystem;
using Spark.Tests;
using Spark.Tests.Stubs;

namespace Spark
{
    [TestFixture]
    public class ImportAndIncludeTester
    {
        private ISparkView CreateView(IViewFolder viewFolder, string template)
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(StubSparkView));

            var sp = new ServiceCollection()
                .AddSpark(settings)
                .AddSingleton<IViewFolder>(viewFolder)
                .BuildServiceProvider();

            var engine = (SparkViewEngine)sp.GetService<ISparkViewEngine>();

            return engine.CreateInstance(new SparkViewDescriptor().AddTemplate(template));
        }

        [Test]
        public void ImportExplicitFile()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    { Path.Combine("importing", "index.spark"), "<p><use import='extra.spark'/>hello ${name}</p>" },
                    { Path.Combine("importing", "extra.spark"), "this is imported <global name='\"world\"'/>" }
                },
                Path.Combine("importing", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>hello world</p>"));
            Assert.That(contents, Does.Not.Contain("import"));
        }

        [Test]
        public void ImportExplicitFileFromShared()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    { Path.Combine("importing", "index.spark"), "<p><use import='extra.spark'/>hello ${name}</p>" },
                    { Path.Combine("shared", "extra.spark"), "this is imported <global name='\"world\"'/>" }
                },
                Path.Combine("importing", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>hello world</p>"));
            Assert.That(contents, Does.Not.Contain("import"));
        }

        [Test]
        public void ImportExplicitWithoutExtension()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    {
                        Path.Combine("importing", "index.spark"),
                        "<p>${foo()} ${name}</p><use import='extra'/><use import='another'/>"
                    },
                    { Path.Combine("importing", "another.spark"), "<macro name='foo'>hello</macro>" },
                    { Path.Combine("shared", "extra.spark"), "this is imported <global name='\"world\"'/>" }
                },
                Path.Combine("importing", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>hello world</p>"));
            Assert.That(contents, Does.Not.Contain("import"));
        }

        [Test]
        public void ImportImplicit()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    { Path.Combine("importing", "index.spark"), "<p>${foo()} ${name}</p>" },
                    { Path.Combine("importing", "_global.spark"), "<macro name='foo'>hello</macro>" },
                    { Path.Combine("shared", "_global.spark"), "this is imported <global name='\"world\"'/>" }
                },
                Path.Combine("importing", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>hello world</p>"));
            Assert.That(contents, Does.Not.Contain("import"));
        }

        [Test]
        public void IncludeFile()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    { Path.Combine("including", "index.spark"), "<p><include href='stuff.spark'/></p>" },
                    { Path.Combine("including", "stuff.spark"), "hello world" }
                },
                Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>hello world</p>"));
        }

        [Test]
        public void MissingFileThrowsException()
        {
            Assert.That(
                () =>
                {
                    var view = CreateView(
                        new InMemoryViewFolder
                        {
                            {
                                Path.Combine("including", "index.spark"),
                                "<p><include href='stuff.spark'/></p>"
                            }
                        },
                        Path.Combine("including", "index.spark"));
                    view.RenderView();
                },
                Throws.TypeOf<CompilerException>());
        }


        [Test]
        public void MissingFileWithEmptyFallbackIsBlank()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    {
                        Path.Combine("including", "index.spark"),
                        "<p><include href='stuff.spark'><fallback/></include></p>"
                    }
                },
                Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p></p>"));
        }

        [Test]
        public void MissingFileWithFallbackUsesContents()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    {
                        Path.Combine("including", "index.spark"),
                        "<p><include href='stuff.spark'><fallback>hello world</fallback></include></p>"
                    }
                },
                Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>hello world</p>"));
        }

        [Test]
        public void ValidIncludeFallbackDisappears()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    {
                        Path.Combine("including", "index.spark"),
                        "<p><include href='stuff.spark'><fallback>hello world</fallback></include></p>"
                    },
                    { Path.Combine("including", "stuff.spark"), "another file" }
                },
                Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>another file</p>"));
        }

        [Test]
        public void FallbackContainsAnotherInclude()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    {
                        Path.Combine("including", "index.spark"),
                        "<p><include href='stuff.spark'><fallback><include href='other.spark'/></fallback></include></p>"
                    },
                    { Path.Combine("including", "other.spark"), "other file" }
                },
                Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>other file</p>"));
        }

        [Test]
        public void IncludeRelativePath()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    { Path.Combine("including", "index.spark"), "<p><include href='../lib/other.spark'/></p>" },
                    { Path.Combine("lib", "other.spark"), "other file" }
                },
                Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>other file</p>"));
        }
        [Test]
        public void IncludeInsideAnInclude()
        {
            var view = CreateView(
                new InMemoryViewFolder
                {
                    { Path.Combine("including", "index.spark"), "<p><include href='../lib/other.spark'/></p>" },
                    { Path.Combine("lib", "other.spark"), "other <include href='third.spark'/> file" },
                    { Path.Combine("lib", "third.spark"), "third file" }
                },
                Path.Combine("including", "index.spark"));
            var contents = view.RenderView();
            Assert.That(contents, Is.EqualTo("<p>other third file file</p>"));
        }

        [Test]
        public void UsingXmlns()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {Path.Combine("including", "index.spark"), "<p xmlns:x='http://www.w3.org/2001/XInclude'><include/><x:include href='../lib/other.spark'/></p>"},
                                          {Path.Combine("lib", "other.spark"), "other file"}
                                      }, Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p xmlns:x='http://www.w3.org/2001/XInclude\'><include></include>other file</p>"));
        }

        [Test]
        public void IncludingAsText()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {Path.Combine("including", "index.spark"), "<p><include href='item.spark' parse='text'/></p>"},
                                          {Path.Combine("including", "item.spark"), "<li>at&t</li>"}
                                      }, Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p>&lt;li&gt;at&amp;t&lt;/li&gt;</p>"));
        }

        [Test]
        public void IncludingAsHtmlWithDollar()
        {
            var view = CreateView(new InMemoryViewFolder
                                      {
                                          {Path.Combine("including", "index.spark"), "<p><include href='jquery.templ.htm' parse='html'/></p>"},
                                          {Path.Combine("including", "jquery.templ.htm"), "<h4>${Title}</h4>"}
                                      }, Path.Combine("including", "index.spark"));

            var contents = view.RenderView();

            Assert.That(contents, Is.EqualTo("<p><h4>${Title}</h4></p>"));
        }
    }
}
