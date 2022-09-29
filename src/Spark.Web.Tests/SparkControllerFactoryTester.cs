using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using MvcContrib.ControllerFactories;
using MvcContrib.SparkViewEngine;
using NUnit.Framework;
using Rhino.Mocks;

namespace MvcContrib.UnitTests.SparkViewEngine
{
    [TestFixture, Category("SparkViewEngine")]
    public class SparkControllerFactoryTester
    {
        [Test]
        public void ControllerSetsViewEngine()
        {
            MockRepository mocks = new MockRepository();
            RequestContext context = new RequestContext(mocks.DynamicHttpContextBase(), new RouteData());

            SparkControllerFactory controllerFactory = new SparkControllerFactory();
            MvcContrib.ConventionController controller =
                (MvcContrib.ConventionController)((IControllerFactory)controllerFactory).CreateController(context, "Convention");

            Assert.IsNotNull(controller.ViewEngine);
            Assert.IsAssignableFrom(typeof(SparkViewFactory), controller.ViewEngine);
        }
    }
}
