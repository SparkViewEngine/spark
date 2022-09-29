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
using System.Configuration;
using NUnit.Framework;
using Spark.Bindings;
using Spark.FileSystem;
using System.IO;
using Spark.Web;

namespace Spark
{
    [TestFixture]
    public class SparkServiceContainerTester
    {
        [Test]
        public void ContainerCreatesDefaultServices()
        {
            var container = new SparkServiceContainer();

            var langauageFactory = container.GetService<ISparkLanguageFactory>();
            Assert.IsInstanceOf(typeof(DefaultLanguageFactory), langauageFactory);

            var resourcePathManager = container.GetService<IResourcePathManager>();
            Assert.IsInstanceOf(typeof(DefaultResourcePathManager), resourcePathManager);

            var bindingProvider = container.GetService<IBindingProvider>();
            Assert.IsInstanceOf(typeof(DefaultBindingProvider), bindingProvider);

            var partialProvider = container.GetService<IPartialProvider>();
            Assert.IsInstanceOf(typeof(DefaultPartialProvider), partialProvider);

            var partialReferenceProvider = container.GetService<IPartialReferenceProvider>();
            Assert.IsInstanceOf(typeof(DefaultPartialReferenceProvider), partialReferenceProvider);
        }

        [Test]
        public void ConfigSettingsUsedByDefault()
        {
            var container = new SparkServiceContainer();

            var settings = container.GetService<ISparkViewEngine>().Settings;
            Assert.AreSame(ConfigurationManager.GetSection("spark"), settings);
        }

        [Test]
        public void CreatedSettingsUsedWhenProvided()
        {
            var settings = new ApplicationBaseSparkSettings().SetPrefix("foo");
            var container = new SparkServiceContainer(settings);

            var settings2 = container.GetService<ISparkViewEngine>().Settings;
            Assert.AreSame(settings, settings2);
        }

        [Test]
        public void SettingsServiceReplacesType()
        {
            var container = new SparkServiceContainer();
            container.SetService<ISparkExtensionFactory>(new StubExtensionFactory());
            Assert.IsInstanceOf(typeof(StubExtensionFactory), container.GetService<ISparkExtensionFactory>());
        }

        [Test]
        public void AddingServiceInstanceCallsInitialize()
        {
            var container = new SparkServiceContainer();
            var service = new TestService();
            Assert.IsFalse(service.Initialized);
            container.SetService<ITestService>(service);
            Assert.IsTrue(service.Initialized);
            Assert.AreSame(service, container.GetService<ITestService>());
        }

        [Test]
        public void AddingServiceBuilderCallsInitialize()
        {
            var container = new SparkServiceContainer();
            container.SetServiceBuilder(typeof(ITestService), c => new TestService());
            var service = container.GetService<ITestService>();
            Assert.IsInstanceOf(typeof(TestService), service);
            Assert.IsTrue(((TestService)service).Initialized);
        }

        [Test]
        public void EngineGetsCustomServiceAndViewFolderSettings()
        {
            var settings = new ApplicationBaseSparkSettings();
            settings.AddViewFolder(typeof(TestViewFolder),
                                   new Dictionary<string, string> { { "testpath", Path.Combine("hello", "world.spark") } });

            var container = new SparkServiceContainer(settings);
            container.SetServiceBuilder<IViewActivatorFactory>(c=>new TestActivatorFactory());

            var engine = container.GetService<ISparkViewEngine>();
            Assert.IsInstanceOf(typeof(TestActivatorFactory), engine.ViewActivatorFactory);

            Assert.IsTrue(engine.ViewFolder.HasView(Path.Combine("hello", "world.spark")));
        }
    }

    public interface ITestService
    {

    }
    public class TestService : ITestService, ISparkServiceInitialize
    {
        public bool Initialized { get; set; }
        public void Initialize(ISparkServiceContainer container)
        {
            Initialized = true;
        }
    }
    public class TestViewFolder : IViewFolder
    {
        private readonly string _testpath;

        public TestViewFolder(string testpath)
        {
            _testpath = testpath;
        }

        public IViewFile GetViewSource(string path)
        {
            throw new System.NotImplementedException();
        }

        public IList<string> ListViews(string path)
        {
            throw new System.NotImplementedException();
        }

        public bool HasView(string path)
        {
            return path == _testpath;
        }
    }
    public class TestActivatorFactory : IViewActivatorFactory
    {
        public IViewActivator Register(Type type)
        {
            throw new System.NotImplementedException();
        }

        public void Unregister(Type type, IViewActivator activator)
        {
            throw new System.NotImplementedException();
        }
    }
}
