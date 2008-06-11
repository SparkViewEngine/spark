using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Services;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Views.Spark.Tests
{
	[TestFixture]
	public class SparkViewFactoryTests 
	{
		private MockRepository mocks;

		[SetUp]
		public void Init()
		{
			mocks = new MockRepository();
		}

		[Test]
		public void ExtensionIsXml()
		{
			var factory = new SparkViewFactory();
			Assert.AreEqual("xml", factory.ViewFileExtension);
		}

		[Test]
		public void ProcessBasicTemplate()
		{
			var engineContext = mocks.CreateMock<IEngineContext>();
			var controller = mocks.CreateMock<IController>();
			var controllerContext = mocks.CreateMock<IControllerContext>();
			var output = new StringWriter();
			var manager = new DefaultViewEngineManager();

			var factory = new SparkViewFactory();
			manager.RegisterEngineForExtesionLookup(factory);
			manager.RegisterEngineForView(factory);
			
			manager.Process("Home\\Index", output, engineContext, controller, controllerContext);
		}
	}

}
