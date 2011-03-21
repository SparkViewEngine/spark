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
using System.IO;

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Services;
    using NUnit.Framework;
    using global::Spark;

    [TestFixture]
    public class SparkViewFactoryStrictNullBehaviourTests : SparkViewFactoryTestsBase
    {
        protected override void Configure()
        {
            var settings = new SparkSettings();
            settings.SetNullBehaviour(NullBehaviour.Strict);
            var sparkViewEngine = new SparkViewEngine(settings);
            serviceProvider.AddService(typeof(ISparkViewEngine), sparkViewEngine);

            factory = new SparkViewFactory();
            factory.Service(serviceProvider);

            manager = new DefaultViewEngineManager();
            manager.Service(serviceProvider);
            serviceProvider.ViewEngineManager = manager;
            serviceProvider.AddService(typeof(IViewEngineManager), manager);

            manager.RegisterEngineForExtesionLookup(factory);
            manager.RegisterEngineForView(factory);
            factory.Service(serviceProvider);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBehaviourConfiguredToStrict_RegularConstruct()
        {
            mocks.ReplayAll();
            manager.Process(
                string.Format("Home{0}NullBehaviourConfiguredToStrict_RegularConstruct", Path.DirectorySeparatorChar),
                output, engineContext, controller, controllerContext);
            Console.WriteLine(output.ToString());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullBehaviourConfiguredToStrict_SuppressNullsConstruct()
        {
            mocks.ReplayAll();
            manager.Process(
                string.Format("Home{0}NullBehaviourConfiguredToStrict_SuppressNullsConstruct",
                              Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            Console.WriteLine(output.ToString());
        }
    }
}