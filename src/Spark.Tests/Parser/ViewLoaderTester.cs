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
using NUnit.Framework.SyntaxHelpers;
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

        private ViewLoader loader;

        private IViewFolder viewFolder;
        private ISparkSyntaxProvider syntaxProvider;

        [SetUp]
        public void Init()
        {

            viewFolder = MockRepository.GenerateMock<IViewFolder>();
            viewFolder.Stub(x => x.ListViews("home")).Return(new[] { "file.spark", "other.spark", "_comment.spark" });
            viewFolder.Stub(x => x.ListViews("Home")).Return(new[] { "file.spark", "other.spark", "_comment.spark" });
            viewFolder.Stub(x => x.ListViews("Account")).Return(new[] { "index.spark" });
            viewFolder.Stub(x => x.ListViews("Shared")).Return(new[] { "layout.spark", "_header.spark", "default.spark", "_footer.spark" });
            viewFolder.Stub(x => x.ListViews("")).IgnoreArguments().Return(new string[0]);

            syntaxProvider = MockRepository.GenerateMock<ISparkSyntaxProvider>();

            loader = new ViewLoader { ViewFolder = viewFolder, SyntaxProvider = syntaxProvider };
        }

        IViewFile ExpectGetChunks(string path, params Chunk[] chunks)
        {
            var source = MockRepository.GenerateMock<IViewFile>();

            viewFolder.Expect(x => x.GetViewSource(path)).Return(source);
            source.Expect(x => x.LastModified).Return(0);
            syntaxProvider.Expect(x => x.GetChunks(null, null)).IgnoreArguments().Return(chunks);

            return source;
        }

        [Test]
        public void LoadSimpleFile()
        {
            ExpectGetChunks("home\\simple.spark".AsPath(), new SendLiteralChunk());
            viewFolder.Stub(x => x.HasView("home\\_global.spark".AsPath())).Return(false);
            viewFolder.Stub(x => x.HasView("Shared\\_global.spark".AsPath())).Return(false);

            var chunks = loader.Load("home\\simple.spark".AsPath());
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();

            Assert.AreEqual(1, chunks.Count());
            Assert.AreEqual(1, loader.GetEverythingLoaded().Count());
        }

        [Test]
        public void LoadUsedFile()
        {
            ExpectGetChunks("Home\\usefile.spark".AsPath(), new RenderPartialChunk { Name = "mypartial" });
            viewFolder.Expect(x => x.HasView("Home\\mypartial.spark".AsPath())).Return(true);
            ExpectGetChunks("Home\\mypartial.spark".AsPath(), new SendLiteralChunk { Text = "Hello world" });
            viewFolder.Stub(x => x.HasView("Home\\_global.spark".AsPath())).Return(false);
            viewFolder.Stub(x => x.HasView("Shared\\_global.spark".AsPath())).Return(false);

            loader.Load("Home\\usefile.spark".AsPath());
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();

            Assert.AreEqual(2, loader.GetEverythingLoaded().Count());
        }


        [Test]
        public void LoadSharedFile()
        {
            ExpectGetChunks("Home\\usefile.spark".AsPath(), new RenderPartialChunk { Name = "mypartial" });
            viewFolder.Expect(x => x.HasView("Home\\mypartial.spark".AsPath())).Return(false);
            viewFolder.Expect(x => x.HasView("Shared\\mypartial.spark".AsPath())).Return(true);
            ExpectGetChunks("Shared\\mypartial.spark".AsPath(), new SendLiteralChunk { Text = "Hello world" });

            viewFolder.Stub(x => x.HasView("Home\\_global.spark".AsPath())).Return(false);
            viewFolder.Stub(x => x.HasView("Shared\\_global.spark".AsPath())).Return(false);

            loader.Load("Home\\usefile.spark".AsPath());
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();
        }

        [Test, Ignore("This test is invalidated. Mocks are hard to keep 'current'.")]
        public void FindPartialFiles()
        {
            var partials3 = loader.FindPartialFiles("Home\\other.spark".AsPath());
            var partials2 = loader.FindPartialFiles("Account\\index.spark".AsPath());
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();

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
            viewFolder.Expect(x => x.GetViewSource("Home\\nosuchfile.spark".AsPath())).Throw(new FileNotFoundException());

            loader.Load("Home\\nosuchfile.spark".AsPath());
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();
        }



        [Test]
        public void ExpiresWhenFilesChange()
        {
            var viewFolder = new StubViewFolder { Path = "home\\changing.spark".AsPath(), LastModified = 4 };

            var viewLoader = new ViewLoader
                             {
                                 ViewFolder = viewFolder,
                                 SyntaxProvider = MockRepository.GenerateStub<ISparkSyntaxProvider>()
                             };
            viewLoader.SyntaxProvider
                .Expect(x => x.GetChunks(null, null))
                .IgnoreArguments()
                .Return(new Chunk[0]);

            viewLoader.Load("home\\changing.spark".AsPath());

            Assert.That(viewLoader.IsCurrent());

            viewFolder.LastModified = 7;
            Assert.That(!viewLoader.IsCurrent());
        }

        public class StubViewFolder : IViewFolder, IViewFile
        {
            public string Path { get; set; }
            public long LastModified { get; set; }

            public IViewFile GetViewSource(string path)
            {
                if (string.Equals(path, Path, StringComparison.InvariantCultureIgnoreCase))
                    return this;

                throw new System.NotImplementedException();
            }

            public IList<string> ListViews(string path)
            {
                return new string[0];
            }

            public bool HasView(string path)
            {
                if (string.Equals(path, Path, StringComparison.InvariantCultureIgnoreCase))
                    return true;

                return false;
            }

            public Stream OpenViewStream()
            {
                throw new System.NotImplementedException();
            }
        }

        [Test]
        public void LoadingPartialInsideNamedSection()
        {
            var viewFolder = new InMemoryViewFolder
                                 {
                                     {"home\\index.spark".AsPath(), "<for each='var x in new[]{1,2,3}'><Guts><section:foo><Another/></section:foo></Guts></for>"},
                                     {"home\\_Guts.spark".AsPath(), "<div><render:foo/></div>"},
                                     {"home\\_Another.spark".AsPath(), "<p>hello world</p>"}
                                 };
            var viewLoader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(ParserSettings.DefaultBehavior), ViewFolder = viewFolder };
            var chunks = viewLoader.Load("home\\index.spark".AsPath());
            var everything = viewLoader.GetEverythingLoaded();
            Assert.AreEqual(3, everything.Count());
        }

        [Test]
        public void PartialsInSameFolderAreDiscovered()
        {
            var viewFolder = new InMemoryViewFolder
                             {
                                 {"home\\zero.spark".AsPath(), ""},
                                 {"home\\_one.spark".AsPath(), ""},
                                 {"product\\two.spark".AsPath(), ""},
                                 {"product\\_three.spark".AsPath(), ""},
                                 {"product\\_four.spark".AsPath(), ""},
                                 {"invoice\\five.spark".AsPath(), ""},
                             };

            var viewLoader = new ViewLoader { ViewFolder = viewFolder };
            var zero = viewLoader.FindPartialFiles("home\\zero.spark".AsPath());
            var product = viewLoader.FindPartialFiles("product\\two.spark".AsPath());
            var invoice = viewLoader.FindPartialFiles("invoice\\five.spark".AsPath());

            Assert.That(zero.Count(), Is.EqualTo(1));
            Assert.That(zero, Has.Some.EqualTo("one"));

            Assert.That(product.Count(), Is.EqualTo(2));
            Assert.That(product, Has.Some.EqualTo("three"));
            Assert.That(product, Has.Some.EqualTo("four"));

            Assert.That(invoice.Count(), Is.EqualTo(0));

            zero = viewLoader.FindPartialFiles("home/zero.spark");
            product = viewLoader.FindPartialFiles("product/two.spark");
            invoice = viewLoader.FindPartialFiles("invoice/five.spark");

            Assert.That(zero.Count(), Is.EqualTo(1));
            Assert.That(zero, Has.Some.EqualTo("one"));

            Assert.That(product.Count(), Is.EqualTo(2));
            Assert.That(product, Has.Some.EqualTo("three"));
            Assert.That(product, Has.Some.EqualTo("four"));

            Assert.That(invoice.Count(), Is.EqualTo(0));
        }

        [Test]
        public void PartialsInCascadingBaseFoldersAndSharedFoldersAreDiscovered()
        {
            var viewFolder = new InMemoryViewFolder
                             {
                                 {"area1\\controller2\\view3.spark".AsPath(), ""},
                                 {"area1\\controller2\\Shared\\_alpha.spark".AsPath(), ""},
                                 {"area1\\Shared\\_beta.spark".AsPath(), ""},
                                 {"Shared\\_gamma.spark".AsPath(), ""},
                                 {"area1\\controller2\\_epsilon.spark".AsPath(), ""},
                                 {"area1\\_zeta.spark".AsPath(), ""},
                                 {"_eta.spark", ""},
                                 {"area1\\controller4\\_dontfind1.spark".AsPath(), ""},
                                 {"area1\\controller4\\Shared\\_dontfind2.spark".AsPath(), ""},
                                 {"area2\\Shared\\_dontfind3.spark".AsPath(), ""},
                             };

            var viewLoader = new ViewLoader { ViewFolder = viewFolder };

            var partials = viewLoader.FindPartialFiles("area1\\controller2\\view3.spark".AsPath());
            Assert.That(partials, Has.Some.EqualTo("alpha"));
            Assert.That(partials, Has.Some.EqualTo("beta"));
            Assert.That(partials, Has.Some.EqualTo("gamma"));
            Assert.That(partials, Has.Some.EqualTo("epsilon"));
            Assert.That(partials, Has.Some.EqualTo("zeta"));
            Assert.That(partials, Has.Some.EqualTo("eta"));
            Assert.That(partials, Has.None.EqualTo("dontfind1"));
            Assert.That(partials, Has.None.EqualTo("dontfind2"));
            Assert.That(partials, Has.None.EqualTo("dontfind3"));
        }
    }
}