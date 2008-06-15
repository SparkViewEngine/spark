using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;
using Rhino.Mocks;

namespace Castle.MonoRail.Views.Spark.Tests
{
	[TestFixture]
	public class ViewComponentExtensionTests
	{
		private MockRepository mocks;

		private IEngineContext engineContext;
		IControllerContext controllerContext;
		private SparkViewFactory factory;
		private DefaultViewEngineManager manager;
		private IController controller;
		private DefaultViewComponentFactory viewComponentFactory;

		[SetUp]
		public void Init()
		{
			mocks = new MockRepository();

			MockServices services = new MockServices();
			services.ViewSourceLoader = new FileAssemblyViewSourceLoader("Views");
			services.AddService(typeof(IViewSourceLoader), services.ViewSourceLoader);

			viewComponentFactory = new DefaultViewComponentFactory();
			viewComponentFactory.Initialize();
			services.AddService(typeof(IViewComponentFactory), viewComponentFactory);
			services.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);

			controller = mocks.DynamicMock<IController>();
			engineContext = new MockEngineContext(new UrlInfo("", "Home", "Index", "/", "castle"));
			controllerContext = new ControllerContext();

			factory = new SparkViewFactory();
			factory.Service(services);

			engineContext.AddService(typeof(IViewComponentFactory), viewComponentFactory);
			engineContext.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);

			manager = new DefaultViewEngineManager();
			manager.RegisterEngineForExtesionLookup(factory);
			manager.RegisterEngineForView(factory);

		}

		[Test, Ignore("This is very tricky to mock")]
		public void DiggPaginationComponent()
		{
			var writer = new StringWriter();
			IList<string> dataSource = new List<string>();
			for(int i = 100; i != 200; i++)
				dataSource.Add(i.ToString());

			controllerContext.PropertyBag["items"] = PaginationHelper.CreatePagination(dataSource, 10, 3);

			manager.Process("Home\\DiggPaginationComponent", writer, engineContext, controller, controllerContext);

			ContainsInOrder(writer.GetStringBuilder().ToString(),
				"<li>130</li>", "<li>139</li>");
		}

		static void ContainsInOrder(string content, params string[] values)
		{
			int index = 0;
			foreach (string value in values)
			{
				int nextIndex = content.IndexOf(value, index);
				Assert.GreaterOrEqual(nextIndex, 0, string.Format("Looking for {0}", value));
				index = nextIndex + value.Length;
			}
		}
	}
}
