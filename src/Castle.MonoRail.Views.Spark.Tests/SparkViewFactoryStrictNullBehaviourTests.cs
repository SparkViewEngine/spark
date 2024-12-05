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
            var settings = 
                new SparkSettings()
                    .SetBaseClassTypeName(typeof(SparkView));

            settings.SetNullBehaviour(NullBehaviour.Strict);

            serviceProvider.AddService(typeof(ISparkSettings), settings);

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
        public void NullBehaviourConfiguredToStrict_RegularConstruct()
        {
            mocks.ReplayAll();
            Assert.That(() =>
            manager.Process(
                string.Format("Home{0}NullBehaviourConfiguredToStrict_RegularConstruct", Path.DirectorySeparatorChar),
                output, engineContext, controller, controllerContext),
                Throws.TypeOf<ArgumentNullException>());
            Console.WriteLine(output.ToString());
        }

        [Test]
        public void NullBehaviourConfiguredToStrict_SuppressNullsConstruct()
        {
            mocks.ReplayAll();
            Assert.That(() =>
                        manager.Process(
                            string.Format("Home{0}NullBehaviourConfiguredToStrict_SuppressNullsConstruct",
                                          Path.DirectorySeparatorChar), output, engineContext, controller,
                            controllerContext),
                        Throws.TypeOf<ArgumentNullException>());
            Console.WriteLine(output.ToString());
        }
    }
}