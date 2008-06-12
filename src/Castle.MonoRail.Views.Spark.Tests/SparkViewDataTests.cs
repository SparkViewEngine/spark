using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Views.Spark.Tests
{
    [TestFixture]
    public class SparkViewDataTests
    {
        private MockRepository mocks;
        private SparkView view;

        IControllerContext controllerContext;
        Dictionary<string, object> propertyBag;
        private IEngineContext context;

        [SetUp]
        public void Init()
        {
            mocks = new MockRepository();

            view = mocks.PartialMock<SparkView>();
            context = mocks.DynamicMock<IEngineContext>();
            controllerContext = mocks.DynamicMock<IControllerContext>();

            propertyBag = new Dictionary<string, object>();

            SetupResult.For(controllerContext.PropertyBag).Return(propertyBag);
            SetupResult.For(view.RenderView()).Return("result-not-needed");
        }

        [Test]
        public void PropertyBagAvailable()
        {
            propertyBag.Add("foo", "bar");

            mocks.ReplayAll();
            view.RenderView(context, controllerContext);
            Assert.AreEqual("bar", view.ViewData["foo"]);
        }
    }
}
