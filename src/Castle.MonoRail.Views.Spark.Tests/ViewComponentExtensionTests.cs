// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Helpers;
    using Castle.MonoRail.Framework.Services;
    using Castle.MonoRail.Framework.Test;
    using NUnit.Framework;
    using Rhino.Mocks;

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

			var services = new StubMonoRailServices();
			services.ViewSourceLoader = new FileAssemblyViewSourceLoader("Views");
			services.AddService(typeof(IViewSourceLoader), services.ViewSourceLoader);

			viewComponentFactory = new DefaultViewComponentFactory();
			viewComponentFactory.Initialize();
			services.AddService(typeof(IViewComponentFactory), viewComponentFactory);
			services.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);

			controller = mocks.DynamicMock<IController>();
			engineContext = new StubEngineContext(new UrlInfo("", "Home", "Index", "/", "castle"));
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
