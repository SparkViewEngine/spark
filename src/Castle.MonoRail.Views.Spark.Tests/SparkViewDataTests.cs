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
namespace Castle.MonoRail.Views.Spark.Tests
{
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Resources;
    using Castle.MonoRail.Framework.Test;
    using NUnit.Framework;
    using Rhino.Mocks;

    [TestFixture]
    public class SparkViewDataTests
    {
        private MockRepository mocks;
        private SparkView view;

        private IEngineContext engineContext;
        IControllerContext controllerContext;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();

            view = mocks.PartialMock<SparkView>();

            engineContext = new StubEngineContext(new UrlInfo("", "Home", "Index", "/", "castle"));
            controllerContext = new ControllerContext();
        }

        [Test]
        public void PropertyBagAvailable()
        {
            controllerContext.PropertyBag.Add("foo", "bar");

            mocks.ReplayAll();
            view.Contextualize(engineContext, controllerContext, null, null, null);

            Assert.That(view.ViewData["foo"], Is.EqualTo("bar"));
        }

        [Test]
        public void MergingCollectionsLikeVelocity()
        {
            //Additionally - the contents of the following collections are merged into the engineContext.
            //* controller.Resources
            //* engineContext.Params
            //* controller.Helpers
            //* engineContext.Flash
            //* controller.PropertyBag

            var resource = MockRepository.GenerateMock<IResource>();

            mocks.ReplayAll();

            controllerContext.PropertyBag.Add("controllerPropertyBagKey", "controllerPropertyBagValue");
            engineContext.Flash.Add("contextFlashKey", "contextFlashValue");
            controllerContext.Helpers.Add("controllerHelpersKey", "controllerHelpersValue");
            engineContext.Request.Params.Add("contextParamsKey", "contextParamsValue");
            controllerContext.Resources.Add("controllerResourcesKey", resource);

            view.Contextualize(engineContext, controllerContext, null, null, null);

            Assert.Multiple(() =>
            {
                Assert.That(view.ViewData["controllerPropertyBagKey"], Is.EqualTo("controllerPropertyBagValue"));
                Assert.That(view.ViewData["contextFlashKey"], Is.EqualTo("contextFlashValue"));
                Assert.That(view.ViewData["controllerHelpersKey"], Is.EqualTo("controllerHelpersValue"));
                Assert.That(view.ViewData["contextParamsKey"], Is.EqualTo("contextParamsValue"));

                Assert.That(view.ViewData["controllerResourcesKey"], Is.SameAs(resource));
            });
        }
    }
}
