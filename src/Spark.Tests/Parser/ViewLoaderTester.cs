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

using System.IO;
using System.Linq;
using Spark.Parser;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.FileSystem;
using Spark.Compiler;
using Spark.Parser.Syntax;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class ViewLoaderTester
    {
        private MockRepository mocks;
        private ViewLoader loader;

        private IViewFolder viewSourceLoader;
        private ISparkSyntaxProvider syntaxProvider;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();

            viewSourceLoader = mocks.CreateMock<IViewFolder>();
            SetupResult.For(viewSourceLoader.ListViews("home")).Return(new[] { "file.spark", "other.spark", "_comment.spark" });
            SetupResult.For(viewSourceLoader.ListViews("Home")).Return(new[] { "file.spark", "other.spark", "_comment.spark" });
            SetupResult.For(viewSourceLoader.ListViews("Account")).Return(new[] { "index.spark" });
            SetupResult.For(viewSourceLoader.ListViews("Shared")).Return(new[] { "layout.spark", "_header.spark", "default.spark", "_footer.spark" });

            syntaxProvider = mocks.CreateMock<ISparkSyntaxProvider>();

            loader = new ViewLoader { ViewFolder = viewSourceLoader, SyntaxProvider = syntaxProvider };
        }

        IViewFile ExpectGetChunks(string path, params Chunk[] chunks)
        {
            var source = mocks.CreateMock<IViewFile>();

            Expect.Call(viewSourceLoader.GetViewSource(path)).Return(source);
            Expect.Call(source.LastModified).Return(0);
            Expect.Call(syntaxProvider.GetChunks(path, viewSourceLoader, null, null)).Return(chunks);

            return source;
        }

        [Test]
        public void LoadSimpleFile()
        {
            ExpectGetChunks("home\\simple.spark", new SendLiteralChunk());

            mocks.ReplayAll();
            var chunks = loader.Load("home\\simple.spark");
            mocks.VerifyAll();

            Assert.AreEqual(1, chunks.Count());
            Assert.AreEqual(1, loader.GetEverythingLoaded().Count());
        }

        [Test]
        public void LoadUsedFile()
        {
            ExpectGetChunks("Home\\usefile.spark", new RenderPartialChunk { Name = "mypartial" });
            Expect.Call(viewSourceLoader.HasView("Home\\mypartial.spark")).Return(true);
            ExpectGetChunks("Home\\mypartial.spark", new SendLiteralChunk { Text = "Hello world" });

            mocks.ReplayAll();
            loader.Load("Home\\usefile.spark");
            mocks.VerifyAll();

            Assert.AreEqual(2, loader.GetEverythingLoaded().Count());
        }


        [Test]
        public void LoadSharedFile()
        {
            ExpectGetChunks("Home\\usefile.spark", new RenderPartialChunk { Name = "mypartial" });
            Expect.Call(viewSourceLoader.HasView("Home\\mypartial.spark")).Return(false);
            Expect.Call(viewSourceLoader.HasView("Shared\\mypartial.spark")).Return(true);
            ExpectGetChunks("Shared\\mypartial.spark", new SendLiteralChunk { Text = "Hello world" });

            mocks.ReplayAll();
            loader.Load("Home\\usefile.spark");
            mocks.VerifyAll();
        }

        [Test]
        public void FindPartialFiles()
        {
            mocks.ReplayAll();
            var partials3 = loader.FindPartialFiles("Home\\other.spark");
            var partials2 = loader.FindPartialFiles("Account\\index.spark");
            mocks.VerifyAll();

            Assert.AreEqual(3, partials3.Count);
            Assert.That(partials3.Contains("comment"));
            Assert.That(partials3.Contains("header"));
            Assert.That(partials3.Contains("footer"));


            Assert.AreEqual(2, partials2.Count);
            Assert.That(partials2.Contains("header"));
            Assert.That(partials2.Contains("footer"));
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        public void FileNotFoundException()
        {
            Expect.Call(viewSourceLoader.GetViewSource("Home\\nosuchfile.spark")).Throw(new FileNotFoundException());

            mocks.ReplayAll();
            loader.Load("Home\\nosuchfile.spark");
            mocks.VerifyAll();
        }

        [Test]
        public void ExpiresWhenFilesChange()
        {
            var source = ExpectGetChunks("home\\changing.spark", new SendLiteralChunk { Text = "Hello world" });
            Expect.Call(viewSourceLoader.GetViewSource("home\\changing.spark")).Return(source);
            Expect.Call(source.LastModified).Return(0);
            Expect.Call(viewSourceLoader.GetViewSource("home\\changing.spark")).Return(source);
            Expect.Call(source.LastModified).Return(42);

            mocks.ReplayAll();
            loader.Load("home\\changing.spark");
            Assert.That(loader.IsCurrent());
            Assert.That(!loader.IsCurrent());
            mocks.VerifyAll();
        }

        [Test]
        public void LoadingPartialInsideNamedSection()
        {
            var viewFolder = new InMemoryViewFolder
                                 {
                                     {"home\\index.spark", "<for each='var x in new[]{1,2,3}'><Guts><section:foo><Another/></section:foo></Guts></for>"},
                                     {"home\\_Guts.spark", "<div><render:foo/></div>"},
                                     {"home\\_Another.spark", "<p>hello world</p>"}
                                 };
            var loader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(), ViewFolder = viewFolder };
            var chunks = loader.Load("home\\index.spark");
            var everything = loader.GetEverythingLoaded();
            Assert.AreEqual(3, everything.Count());

        }
    }
}