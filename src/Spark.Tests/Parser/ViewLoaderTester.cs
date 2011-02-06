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
            ExpectGetChunks(string.Format("home{0}simple.spark", Path.DirectorySeparatorChar), new SendLiteralChunk());
            viewFolder.Stub(x => x.HasView(string.Format("home{0}_global.spark", Path.DirectorySeparatorChar))).Return(false);
            viewFolder.Stub(x => x.HasView(string.Format("Shared{0}_global.spark", Path.DirectorySeparatorChar))).Return(false);

            var chunks = loader.Load(string.Format("home{0}simple.spark", Path.DirectorySeparatorChar));
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();

            Assert.AreEqual(1, chunks.Count());
            Assert.AreEqual(1, loader.GetEverythingLoaded().Count());
        }

        [Test]
        public void LoadUsedFile()
        {
            ExpectGetChunks(string.Format("Home{0}usefile.spark", Path.DirectorySeparatorChar), new RenderPartialChunk { Name = "mypartial" });
            viewFolder.Expect(x => x.HasView(string.Format("Home{0}mypartial.spark", Path.DirectorySeparatorChar))).Return(true);
            ExpectGetChunks(string.Format("Home{0}mypartial.spark", Path.DirectorySeparatorChar), new SendLiteralChunk { Text = "Hello world" });
            viewFolder.Stub(x => x.HasView(string.Format("Home{0}_global.spark", Path.DirectorySeparatorChar))).Return(false);
            viewFolder.Stub(x => x.HasView(string.Format("Shared{0}_global.spark", Path.DirectorySeparatorChar))).Return(false);

            loader.Load(string.Format("Home{0}usefile.spark", Path.DirectorySeparatorChar));
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();

            Assert.AreEqual(2, loader.GetEverythingLoaded().Count());
        }


        [Test]
        public void LoadSharedFile()
        {
            ExpectGetChunks(string.Format("Home{0}usefile.spark", Path.DirectorySeparatorChar), new RenderPartialChunk { Name = "mypartial" });
            viewFolder.Expect(x => x.HasView(string.Format("Home{0}mypartial.spark", Path.DirectorySeparatorChar))).Return(false);
            viewFolder.Expect(x => x.HasView(string.Format("Shared{0}mypartial.spark", Path.DirectorySeparatorChar))).Return(true);
            ExpectGetChunks(string.Format("Shared{0}mypartial.spark", Path.DirectorySeparatorChar), new SendLiteralChunk { Text = "Hello world" });

            viewFolder.Stub(x => x.HasView(string.Format("Home{0}_global.spark", Path.DirectorySeparatorChar))).Return(false);
            viewFolder.Stub(x => x.HasView(string.Format("Shared{0}_global.spark", Path.DirectorySeparatorChar))).Return(false);

            loader.Load(string.Format("Home{0}usefile.spark", Path.DirectorySeparatorChar));
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();
        }

        [Test, Ignore("This test is invalidated. Mocks are hard to keep 'current'.")]
        public void FindPartialFiles()
        {
            var partials3 = loader.FindPartialFiles(string.Format("Home{0}other.spark", Path.DirectorySeparatorChar));
            var partials2 = loader.FindPartialFiles(string.Format("Account{0}index.spark", Path.DirectorySeparatorChar));
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
            viewFolder.Expect(x => x.GetViewSource(string.Format("Home{0}nosuchfile.spark", Path.DirectorySeparatorChar))).Throw(new FileNotFoundException());

            loader.Load(string.Format("Home{0}nosuchfile.spark", Path.DirectorySeparatorChar));
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();
        }



        [Test]
        public void ExpiresWhenFilesChange()
        {
            var viewFolder = new StubViewFolder { Path = string.Format("home{0}changing.spark", Path.DirectorySeparatorChar), LastModified = 4 };

            var viewLoader = new ViewLoader
                             {
                                 ViewFolder = viewFolder,
                                 SyntaxProvider = MockRepository.GenerateStub<ISparkSyntaxProvider>()
                             };
            viewLoader.SyntaxProvider
                .Expect(x => x.GetChunks(null, null))
                .IgnoreArguments()
                .Return(new Chunk[0]);

            viewLoader.Load(string.Format("home{0}changing.spark", Path.DirectorySeparatorChar));

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
                                     {string.Format("home{0}index.spark", Path.DirectorySeparatorChar), "<for each='var x in new[]{1,2,3}'><Guts><section:foo><Another/></section:foo></Guts></for>"},
                                     {string.Format("home{0}_Guts.spark", Path.DirectorySeparatorChar), "<div><render:foo/></div>"},
                                     {string.Format("home{0}_Another.spark", Path.DirectorySeparatorChar), "<p>hello world</p>"}
                                 };
            var viewLoader = new ViewLoader { SyntaxProvider = new DefaultSyntaxProvider(ParserSettings.DefaultBehavior), ViewFolder = viewFolder };
            var chunks = viewLoader.Load(string.Format("home{0}index.spark", Path.DirectorySeparatorChar));
            var everything = viewLoader.GetEverythingLoaded();
            Assert.AreEqual(3, everything.Count());
        }

        [Test]
        public void PartialsInSameFolderAreDiscovered()
        {
            var viewFolder = new InMemoryViewFolder
                             {
                                 {string.Format("home{0}zero.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("home{0}_one.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("product{0}two.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("product{0}_three.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("product{0}_four.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("invoice{0}five.spark", Path.DirectorySeparatorChar), ""},
                             };

            var viewLoader = new ViewLoader { ViewFolder = viewFolder };
            var zero = viewLoader.FindPartialFiles(string.Format("home{0}zero.spark", Path.DirectorySeparatorChar));
            var product = viewLoader.FindPartialFiles(string.Format("product{0}two.spark", Path.DirectorySeparatorChar));
            var invoice = viewLoader.FindPartialFiles(string.Format("invoice{0}five.spark", Path.DirectorySeparatorChar));

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
                                 {string.Format("area1{0}controller2{0}view3.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("area1{0}controller2{0}Shared{0}_alpha.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("area1{0}Shared{0}_beta.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("Shared{0}_gamma.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("area1{0}controller2{0}_epsilon.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("area1{0}_zeta.spark", Path.DirectorySeparatorChar), ""},
                                 {"_eta.spark", ""},
                                 {string.Format("area1{0}controller4{0}_dontfind1.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("area1{0}controller4{0}Shared{0}_dontfind2.spark", Path.DirectorySeparatorChar), ""},
                                 {string.Format("area2{0}Shared{0}_dontfind3.spark", Path.DirectorySeparatorChar), ""},
                             };

            var viewLoader = new ViewLoader { ViewFolder = viewFolder };

            var partials = viewLoader.FindPartialFiles(string.Format("area1{0}controller2{0}view3.spark", Path.DirectorySeparatorChar));
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