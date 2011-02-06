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
using System.Globalization;
using System.Threading;

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System.IO;


    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Helpers;
    using Castle.MonoRail.Framework.Services;
    using NUnit.Framework;
    using global::Spark;
    using NUnit.Framework.SyntaxHelpers;


    [TestFixture]
    public class SparkViewFactoryTests : SparkViewFactoryTestsBase
	{
		protected override void Configure()
		{
			factory = new SparkViewFactory();
			factory.Service(serviceProvider);

			manager = new DefaultViewEngineManager();
			manager.Service(serviceProvider);
			serviceProvider.ViewEngineManager = manager;
			serviceProvider.AddService(typeof(IViewEngineManager), manager);

			manager.RegisterEngineForExtesionLookup(factory);
			manager.RegisterEngineForView(factory);
		}

		[Test]
        public void ExtensionIsSpark()
        {
            mocks.ReplayAll();
            Assert.AreEqual("spark", factory.ViewFileExtension);
        }

        [Test]
        public void ProcessBasicTemplate()
        {
            mocks.ReplayAll();
            manager.Process(string.Format("Home{0}Index", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            Assert.That(output.ToString().Contains("<h1>Simple test</h1>"));
        }

        [Test]
        public void ContextAndControllerContextAvailable()
        {
            mocks.ReplayAll();
            manager.Process(string.Format("Home{0}Index", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(string.Format("Home{0}Index.spark", Path.DirectorySeparatorChar));
            descriptor.Templates.Add(string.Format("Shared{0}default.spark", Path.DirectorySeparatorChar));
            var entry = factory.Engine.GetEntry(descriptor);
            var view = (SparkView)entry.CreateInstance();
            view.Contextualize(engineContext, controllerContext, factory, null);
            
            var result = new StringWriter();
            view.RenderView(result);
            Assert.AreEqual(result.ToString(), output.ToString());
            Assert.AreSame(engineContext, view.Context);
            Assert.AreSame(controllerContext, view.ControllerContext);
        }

        [Test, Ignore("Need to get the helpers to function again using the stub objects")]
        public void HelperModelDictionaries()
        {
            InitUrlInfo("", "Home", "Index");

            mocks.ReplayAll();
            controllerContext.Helpers.Add("FormHelper", new FormHelper(engineContext));
            var urlHelper = new UrlHelper(engineContext);
            controllerContext.Helpers.Add("UrlHelper", urlHelper);
            manager.Process(string.Format("Home{0}HelperModelDictionaries", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                            "Home/foo.castle",
                            "<form", "action='/Home/save.castle'", "method='get'",
                            "<input", "type=\"text\"", "id=\"hello\"", "class=\"world\"", "/>",
                            "</form>");
        }


        [Test]
        public void PropertyBagViewdata()
        {
            mocks.ReplayAll();
            propertyBag["foo"] = "baaz";
            propertyBag["bar"] = 7;
            manager.Process(string.Format("Home{0}PropertyBagViewdata", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                            "<p>foo:baaz</p>",
                            "<p>bar:7</p>",
                            "<p>bar+4:11</p>");
        }

		[Test]
		public void NullBehaviourConfiguredToLenient()
		{
			mocks.ReplayAll();
            manager.Process(string.Format("Home{0}NullBehaviourConfiguredToLenient", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
			var content = output.ToString();
			Assert.IsFalse(content.Contains("default"));

			ContainsInOrder(content,
				"<p>name kaboom *${user.Name}*</p>",
				"<p>name silently **</p>",
				"<p>name fixed *fred*</p>");
		}

        [Test]
        public void TerseHtmlEncode()
        {
            mocks.ReplayAll();
            manager.Process(string.Format("Home{0}TerseHtmlEncode", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                "<p>This &lt;contains/&gt; html</p>");
        }

        [Test]
        public void IncludingStatementsDirectly()
        {
            mocks.ReplayAll();
            manager.Process(string.Format("Home{0}CodeStatements", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                "<p>was true</p>");


            Assert.IsFalse(output.ToString().Contains("<p>was false</p>"));
            
        }

        [Test]
        public void Rescue404Rendering()
        {
            //SetupResult.For(response.StatusCode).PropertyBehavior();
            //SetupResult.For(response.StatusDescription).PropertyBehavior();
            mocks.ReplayAll();
            var handler = new MonoRailHttpHandlerFactory.NotFoundHandler("", "nosuchcontroller", engineContext);
            handler.ProcessRequest(null);
            Assert.AreEqual(404, response.StatusCode);
            Assert.AreEqual("<p>404 message rendered</p>\r\n", output.ToString());
        }

        [Test, Ignore("Controller Type effects are no longer supported in 1.1")]
        public void ControllerHelperAttributeCanBeUsed()
        {
            controller = new Helpers.HomeController();
            controllerContext.ControllerDescriptor = serviceProvider.ControllerDescriptorProvider.BuildDescriptor(controller);
            controllerContext.Helpers.Add("bar", new Helpers.TestingHelper());
            mocks.ReplayAll();
            manager.Process(string.Format("Home{0}ControllerHelperAttributeCanBeUsed", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
            Assert.That(output.ToString().Contains("<p>Hello</p>"));            
        }

		[Test, Ignore("No way to mock HttpContext.Current")]
		public void ControllerHelpersCanBeUsedWhenRenderingMailView()
		{
			controller = new Helpers.HomeController();
			controllerContext.ControllerDescriptor = serviceProvider.ControllerDescriptorProvider.BuildDescriptor(controller);
			controllerContext.Helpers.Add("bar", new Helpers.TestingHelper());
			mocks.ReplayAll();
            manager.Process(string.Format("Home{0}ControllerHelperAttributeCanBeUsed", Path.DirectorySeparatorChar), null, output, null);
			Assert.That(output.ToString().Contains("<p>Hello</p>"));
		}

        [Test]
        public void LateBoundExpressionShouldCallEval()
        {
            mocks.ReplayAll();
            propertyBag["hello"] = "world";
            propertyBag["foo"] = 1005.3;
			using (new CurrentCultureScope(""))
			{
                manager.Process(string.Format("Home{0}LateBoundExpressionShouldCallEval", Path.DirectorySeparatorChar), output, engineContext, controller, controllerContext);
				Assert.That(output.ToString(), Text.Contains(string.Format("<p>world {0:#,##0.00}</p>", 1005.3)));
			}
        }
    }

	public class CurrentCultureScope : IDisposable
	{
		private readonly CultureInfo _culture;
		public CurrentCultureScope(string name)
		{
			_culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo(name);
		}
		public void Dispose()
		{
			Thread.CurrentThread.CurrentCulture = _culture;
		}
	}
}
    
