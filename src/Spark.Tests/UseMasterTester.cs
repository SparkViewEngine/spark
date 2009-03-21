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
using NUnit.Framework;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
    [TestFixture]
    public class UseMasterTester
    {
        private ISparkViewEngine _engine;
        private InMemoryViewFolder _viewFolder;

        [SetUp]
        public void Init()
        {
            CompiledViewHolder.Current = new CompiledViewHolder();

            var settings = new SparkSettings()
                .SetPageBaseType(typeof(StubSparkView));
            var container = new SparkServiceContainer(settings);

            _viewFolder = new InMemoryViewFolder();

            container.SetServiceBuilder<IViewFolder>(c => _viewFolder);

            _engine = container.GetService<ISparkViewEngine>();
        }

        private string RenderView(SparkViewDescriptor descriptor)
        {
            var view = _engine.CreateInstance(descriptor);
            var contents = view.RenderView();
            _engine.ReleaseInstance(view);
            return contents;
        }

        [Test]
        public void NormalSituationUsesNoLayout()
        {
            _viewFolder.Add("Home\\Index.spark", "<p>Hello world</p>");

            var contents = RenderView(new SparkViewDescriptor()
                .AddTemplate("Home\\Index.spark"));

            Assert.AreEqual("<p>Hello world</p>", contents);
        }


        [Test]
        public void UseMasterLooksInLayoutFolder()
        {
            _viewFolder.Add("Home\\Index.spark", "<use master=\"foo\"/><p>Hello world</p>");
            _viewFolder.Add("Layouts\\foo.spark", "<h1>alpha</h1><use:view/><p>beta</p>");

            var descriptor = new SparkViewDescriptor()
                .AddTemplate("Home\\Index.spark");

            var contents = RenderView(descriptor);

            Assert.AreEqual("<h1>alpha</h1><p>Hello world</p><p>beta</p>", contents);
        }

        [Test, ExpectedException(typeof(CompilerException))]
        public void TemplateWontLoadRecursively()
        {
            _viewFolder.Add("Home\\Index.spark", "<use master=\"foo\"/><p>Hello world</p>");
            _viewFolder.Add("Layout\\foo.spark", "<h1>alpha</h1><use:view/><p>beta<use master=\"foo\"/></p>");

            var contents = RenderView(new SparkViewDescriptor()
                                          .AddTemplate("Home\\Index.spark"));

            Assert.AreEqual("<h1>alpha</h1><p>Hello world</p><p>beta</p>", contents);
        }

        [Test]
        public void DefaultLayoutsOverriddenByElement()
        {
            _viewFolder.Add("Home\\Normal.spark", "<p>Hello world</p>");
            _viewFolder.Add("Home\\Override.spark", "<use master=\"foo\"/><p>Hello world</p>");
            _viewFolder.Add("Layouts\\foo.spark", "<h1>alpha</h1><use:view/><p>beta</p>");
            _viewFolder.Add("Layouts\\Application.spark", "<h1>gamma</h1><use:view/><p>delta</p>");

            var contents1 = RenderView(new SparkViewDescriptor()
                .AddTemplate("Home\\Normal.spark")
                .AddTemplate("Layouts\\Application.spark"));

            Assert.AreEqual("<h1>gamma</h1><p>Hello world</p><p>delta</p>", contents1);

            var contents2 = RenderView(new SparkViewDescriptor()
                .AddTemplate("Home\\Override.spark")
                .AddTemplate("Layouts\\Application.spark"));

            Assert.AreEqual("<h1>alpha</h1><p>Hello world</p><p>beta</p>", contents2);
        }

        [Test]
        public void DaisyChainingMasterRendersMultipleLayers()
        {
            _viewFolder.Add("Home\\Index.spark", "<use master=\"foo\"/><p>Hello world</p><content:title>bar</content:title>");
            _viewFolder.Add("Layouts\\foo.spark", "<use master=\"html\"/><h1>alpha</h1><use:view/><p>beta</p>");
            _viewFolder.Add("Layouts\\html.spark", "<html><head><title><use:title/></title></head><body><use:view/></body></html>");

            var contents = RenderView(new SparkViewDescriptor()
                .AddTemplate("Home\\Index.spark")
                .AddTemplate("Layouts\\Application.spark"));

            Assert.AreEqual("<html><head><title>bar</title></head><body><h1>alpha</h1><p>Hello world</p><p>beta</p></body></html>", contents);
        }

        [Test]
        public void EmptyMasterPreventsDefaultLayout()
        {
            var settings = new SparkSettings()
                .SetPageBaseType(typeof(StubSparkView));
            var container = new SparkServiceContainer(settings);

            var viewFolder = new InMemoryViewFolder
                             {
                                 {"Home\\Index.spark", "<use master=\"\"/><p>Hello world</p><content:title>bar</content:title>"},
                                 {"Layouts\\Application.spark", "<h1>alpha</h1><use:view/><p>beta</p>"}
                             };

            container.SetServiceBuilder<IViewFolder>(c => viewFolder);

            var engine = container.GetService<ISparkViewEngine>();

            var descriptor = new SparkViewDescriptor()
                .AddTemplate("Home\\Index.spark")
                .AddTemplate("Layouts\\Application.spark");

            var view = engine.CreateInstance(descriptor);
            var contents = view.RenderView();
            engine.ReleaseInstance(view);

            Assert.AreEqual("<p>Hello world</p>", contents);
        }
    }
}
