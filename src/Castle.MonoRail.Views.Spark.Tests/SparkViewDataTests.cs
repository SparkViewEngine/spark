using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Resources;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Views.Spark.Tests
{
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
            
            engineContext = new MockEngineContext(new UrlInfo("", "Home", "Index", "/", "castle"));
            controllerContext = new ControllerContext();            


            SetupResult.For(view.RenderView()).Return("result-not-needed");
        }

        [Test]
        public void PropertyBagAvailable()
        {
            controllerContext.PropertyBag.Add("foo", "bar");

            mocks.ReplayAll();
            view.Contextualize(engineContext, controllerContext);

            Assert.AreEqual("bar", view.ViewData["foo"]);
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

            //SetupResult.For(controller.Resources).Return(new Dictionary<string, IResource>());
            //SetupResult.For(controller.Params).Return(new NameValueCollection());
            //SetupResult.For(controller.Helpers).Return(new HelperDictionary());
            //SetupResult.For(engineContext.Flash).Return(new Flash());

            var resource = mocks.CreateMock<IResource>();
            
//            var controller = mocks.PartialMock<Controller>();
            mocks.ReplayAll();
  //          controller.Contextualize(engineContext, controllerContext);

            controllerContext.PropertyBag.Add("controllerPropertyBagKey", "controllerPropertyBagValue");
            engineContext.Flash.Add("contextFlashKey", "contextFlashValue");
            controllerContext.Helpers.Add("controllerHelpersKey", "controllerHelpersValue");
            engineContext.Request.Params.Add("contextParamsKey", "contextParamsValue");
            controllerContext.Resources.Add("controllerResourcesKey", resource);

            view.Contextualize(engineContext, controllerContext);

            Assert.AreEqual("controllerPropertyBagValue", view.ViewData["controllerPropertyBagKey"]);
            Assert.AreEqual("contextFlashValue", view.ViewData["contextFlashKey"]);
            Assert.AreEqual("controllerHelpersValue", view.ViewData["controllerHelpersKey"]);
            Assert.AreEqual("contextParamsValue", view.ViewData["contextParamsKey"]);
            Assert.AreSame(resource, view.ViewData["controllerResourcesKey"]);
        }
    }
}
