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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spark.Bindings;
using Spark.Parser;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.FileSystem;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
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
            viewFolder.Stub(x => x.ListViews(Arg<string>.Is.Anything)).IgnoreArguments().Return(Array.Empty<string>());

            syntaxProvider = MockRepository.GenerateMock<ISparkSyntaxProvider>();

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            loader = new ViewLoader(settings, viewFolder, partialProvider, new DefaultPartialReferenceProvider(partialProvider), null, syntaxProvider, null);
        }

        private static int syntaxCallbacks = 0;

        IViewFile ExpectGetChunks(string path, params Chunk[] chunks)
        {
            var source = MockRepository.GenerateMock<IViewFile>();

            viewFolder
                .Expect(x => x.GetViewSource(path))
                .Return(source);

            source
                .Expect(x => x.LastModified)
                .Return(0);

            syntaxProvider
                .Expect(x => x.GetChunks(Arg<VisitorContext>.Is.Anything, Arg<string>.Is.Anything))
                .WhenCalled(
                    call =>
                    {
                        syntaxCallbacks++;
                    })
                .Return(chunks);

            return source;
        }

        [Test]
        public void LoadSimpleFile()
        {
            ExpectGetChunks(Path.Combine("home", "simple.spark"), new SendLiteralChunk());

            viewFolder.Stub(x => x.HasView(Path.Combine("home", "_global.spark"))).Return(false);
            viewFolder.Stub(x => x.HasView(Path.Combine("Shared", "_global.spark"))).Return(false);

            var chunks = loader.Load(Path.Combine("home", "simple.spark"));
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();

            Assert.Multiple(() =>
            {
                Assert.That(chunks.Count(), Is.EqualTo(1));
                Assert.That(loader.GetEverythingLoaded().Count(), Is.EqualTo(1));
            });
        }

        [Test]
        public void LoadUseFile()
        {
            ExpectGetChunks(Path.Combine("Home", "usefile.spark"), new RenderPartialChunk { Name = "mypartial" });
            viewFolder.Expect(x => x.HasView(Path.Combine("Home", "mypartial.spark"))).Return(true);
            ExpectGetChunks(Path.Combine("Home", "mypartial.spark"), new SendLiteralChunk { Text = "Hello world" });

            viewFolder.Stub(x => x.HasView(Path.Combine("Home", "_global.spark"))).Return(false);
            viewFolder.Stub(x => x.HasView(Path.Combine("Shared", "_global.spark"))).Return(false);

            loader.Load(Path.Combine("Home", "usefile.spark"));

            viewFolder.VerifyAllExpectations();
            syntaxProvider.AssertWasCalled(call => call.GetChunks(Arg<VisitorContext>.Is.Anything, Arg<string>.Is.Anything), options => options.Repeat.Times(2));

            Assert.That(loader.GetEverythingLoaded().Count(), Is.EqualTo(2));
        }

        [Test]
        public void LoadSharedFile()
        {
            ExpectGetChunks(Path.Combine("Home", "usefile.spark"), new RenderPartialChunk { Name = "mypartial" });
            viewFolder.Expect(x => x.HasView(Path.Combine("Home", "mypartial.spark"))).Return(false);
            viewFolder.Expect(x => x.HasView(Path.Combine("Shared", "mypartial.spark"))).Return(true);
            ExpectGetChunks(Path.Combine("Shared", "mypartial.spark"), new SendLiteralChunk { Text = "Hello world" });

            viewFolder.Stub(x => x.HasView(Path.Combine("Home", "_global.spark"))).Return(false);
            viewFolder.Stub(x => x.HasView(Path.Combine("Shared", "_global.spark"))).Return(false);

            loader.Load(Path.Combine("Home", "usefile.spark"));

            viewFolder.VerifyAllExpectations();
            syntaxProvider.AssertWasCalled(call => call.GetChunks(Arg<VisitorContext>.Is.Anything, Arg<string>.Is.Anything), options => options.Repeat.Times(2));
        }

        [Test, Ignore("This test is invalidated. Mocks are hard to keep 'current'.")]
        public void FindPartialFiles()
        {
            var partials3 = loader.FindPartialFiles(Path.Combine("Home", "other.spark"));
            var partials2 = loader.FindPartialFiles(Path.Combine("Account", "index.spark"));
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();

            Assert.That(partials3, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(partials3.Contains("comment"));
                Assert.That(partials3.Contains("header"));
                Assert.That(partials3.Contains("footer"));

                Assert.That(partials2, Has.Count.EqualTo(2));
            });
            Assert.Multiple(() =>
            {
                Assert.That(partials2.Contains("header"));
                Assert.That(partials2.Contains("footer"));
            });
        }

        [Test]
        public void FileNotFoundException()
        {
            viewFolder.Expect(x => x.GetViewSource(Path.Combine("Home", "nosuchfile.spark"))).Throw(new FileNotFoundException());
            Assert.That(() =>
                        loader.Load(Path.Combine("Home", "nosuchfile.spark")),
                        Throws.TypeOf<FileNotFoundException>());
            viewFolder.VerifyAllExpectations();
            syntaxProvider.VerifyAllExpectations();
        }

        [Test]
        public void ExpiresWhenFilesChange()
        {
            var settings = new SparkSettings();

            var viewFolder = new StubViewFolder
            {
                Path = Path.Combine("home", "changing.spark"),
                LastModified = 4
            };

            this.syntaxProvider = MockRepository.GenerateStub<ISparkSyntaxProvider>();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(settings, viewFolder, partialProvider, new DefaultPartialReferenceProvider(partialProvider), null, this.syntaxProvider, null);

            this.syntaxProvider
                .Expect(x => x.GetChunks(null, null))
                .IgnoreArguments()
                .Return(Array.Empty<Chunk>());

            viewLoader.Load(Path.Combine("home", "changing.spark"));

            Assert.That(viewLoader.IsCurrent());

            viewFolder.LastModified = 7;
            Assert.That(!viewLoader.IsCurrent());
        }

        [Test]
        public void BindingProviderIsCalledUsingTheCorrectBindingRequest()
        {
            var bindingProvider = MockRepository.GenerateMock<IBindingProvider>();
            var syntaxProvider = MockRepository.GenerateStub<ISparkSyntaxProvider>();
            var viewPath = Path.Combine("Account", "index.spark");
            var folder = new StubViewFolder { Path = viewPath, LastModified = 4 };

            bindingProvider.Expect(x => x.GetBindings(null))
                .IgnoreArguments()
                .Return(Array.Empty<Binding>())
                .Callback<BindingRequest>(x => x.ViewFolder == folder && x.ViewPath == viewPath);

            syntaxProvider.Stub(x => x.GetChunks(null, null))
                .IgnoreArguments()
                .Return(new List<Chunk>());

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(settings, folder, partialProvider, new DefaultPartialReferenceProvider(partialProvider), null, syntaxProvider, bindingProvider);

            viewLoader.Load(viewPath);

            bindingProvider.VerifyAllExpectations();
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
                return Array.Empty<string>();
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
                                     {Path.Combine("home", "index.spark"), "<for each='var x in new[]{1,2,3}'><Guts><section:foo><Another/></section:foo></Guts></for>"},
                                     {Path.Combine("home", "_Guts.spark"), "<div><render:foo/></div>"},
                                     {Path.Combine("home", "_Another.spark"), "<p>hello world</p>"}
                                 };

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(
                settings,
                viewFolder,
                new DefaultPartialProvider(),
                new DefaultPartialReferenceProvider(partialProvider),
                null,
                new DefaultSyntaxProvider(ParserSettings.DefaultBehavior),
                null);
            _ = viewLoader.Load(Path.Combine("home", "index.spark"));
            var everything = viewLoader.GetEverythingLoaded();

            Assert.That(everything.Count(), Is.EqualTo(3));
        }

        [Test]
        public void PartialsInSameFolderAreDiscovered()
        {
            var viewFolder = new InMemoryViewFolder
                             {
                                 {Path.Combine("home", "zero.spark"), ""},
                                 {Path.Combine("home", "_one.spark"), ""},
                                 {Path.Combine("product", "two.spark"), ""},
                                 {Path.Combine("product", "_three.spark"), ""},
                                 {Path.Combine("product", "_four.spark"), ""},
                                 {Path.Combine("invoice", "five.spark"), ""},
                             };

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(
                settings,
                viewFolder,
                new DefaultPartialProvider(),
                new DefaultPartialReferenceProvider(partialProvider),
                null,
                new DefaultSyntaxProvider(ParserSettings.DefaultBehavior),
                null);

            var zero = viewLoader.FindPartialFiles(Path.Combine("home", "zero.spark"));
            var product = viewLoader.FindPartialFiles(Path.Combine("product", "two.spark"));
            var invoice = viewLoader.FindPartialFiles(Path.Combine("invoice", "five.spark"));

            Assert.Multiple(() =>
            {
                Assert.That(zero.Count(), Is.EqualTo(1));
                Assert.That(zero, Has.Some.EqualTo("one"));

                Assert.That(product.Count(), Is.EqualTo(2));
                Assert.That(product, Has.Some.EqualTo("three"));
            });
            Assert.That(product, Has.Some.EqualTo("four"));

            Assert.That(invoice.Count(), Is.EqualTo(0));

            zero = viewLoader.FindPartialFiles("home/zero.spark");
            product = viewLoader.FindPartialFiles("product/two.spark");
            invoice = viewLoader.FindPartialFiles("invoice/five.spark");

            Assert.That(zero.Count(), Is.EqualTo(1));
            Assert.That(zero, Has.Some.EqualTo("one"));

            Assert.That(product.Count(), Is.EqualTo(2));
            Assert.That(product, Has.Some.EqualTo("three"));
            Assert.Multiple(() =>
            {
                Assert.That(product, Has.Some.EqualTo("four"));

                Assert.That(invoice.Count(), Is.EqualTo(0));
            });
        }

        [Test]
        public void PartialsInCascadingBaseFoldersAndSharedFoldersAreDiscovered()
        {
            var viewFolder = new InMemoryViewFolder
                             {
                                 {Path.Combine("area1","controller2","view3.spark"), ""},
                                 {Path.Combine("area1","controller2","Shared","_alpha.spark"), ""},
                                 {Path.Combine("area1","Shared","_beta.spark"), ""},
                                 {Path.Combine("Shared", "_gamma.spark"), ""},
                                 {Path.Combine("area1","controller2","_epsilon.spark"), ""},
                                 {Path.Combine("area1", "_zeta.spark"), ""},
                                 {"_eta.spark", ""},
                                 {Path.Combine("area1","controller4","_dontfind1.spark"), ""},
                                 {Path.Combine("area1","controller4","Shared","_dontfind2.spark"), ""},
                                 {Path.Combine("area2","Shared","_dontfind3.spark"), ""},
                             };

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(
                settings,
                viewFolder,
                new DefaultPartialProvider(),
                new DefaultPartialReferenceProvider(partialProvider),
                null,
                new DefaultSyntaxProvider(ParserSettings.DefaultBehavior),
                null);

            var partials = viewLoader.FindPartialFiles(Path.Combine("area1", "controller2", "view3.spark"));

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

        [Test]
        public void LoadingEmptyFile()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("home", "empty.spark"), "" },
            };

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(
                settings,
                viewFolder,
                new DefaultPartialProvider(),
                new DefaultPartialReferenceProvider(partialProvider),
                null,
                new DefaultSyntaxProvider(ParserSettings.DefaultBehavior),
                null);
            _ = viewLoader.Load(Path.Combine("home", "empty.spark"));
            var everything = viewLoader.GetEverythingLoaded();

            Assert.That(everything.Count(), Is.EqualTo(1));
        }

        [Test]
        public void LoadingEmptyShadeFile()
        {
            var viewFolder = new InMemoryViewFolder
            {
                { Path.Combine("home", "empty.shade"), "" },
            };

            var settings = new SparkSettings();

            var partialProvider = new DefaultPartialProvider();

            var viewLoader = new ViewLoader(
                settings,
                viewFolder,
                new DefaultPartialProvider(),
                new DefaultPartialReferenceProvider(partialProvider),
                null,
                new DefaultSyntaxProvider(ParserSettings.DefaultBehavior),
                null);
            _ = viewLoader.Load(Path.Combine("home", "empty.shade"));
            var everything = viewLoader.GetEverythingLoaded();

            Assert.That(everything.Count(), Is.EqualTo(1));
        }
    }
}