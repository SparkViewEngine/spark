using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;
using Rhino.Mocks;
using Spark;

namespace Castle.MonoRail.Views.Spark.Tests
{
	[TestFixture]
	public class SparkViewFactoryTests 
	{
		private MockRepository mocks;
        private IEngineContext engineContext;
        private IRequest request;
        private IResponse response;
        private IServerUtility server;

        private IController controller;
        private IControllerContext controllerContext;

        private IDictionary propertyBag;
        private Flash flash;
        private IDictionary session;

        private StringWriter output;
	    private DefaultViewEngineManager manager;
	    private HelperDictionary helpers;
	    private IRoutingEngine routingEngine;
        private SparkViewFactory factory;
	    private NameValueCollection requestParams;
	    private IDictionary contextItems;

	    [SetUp]
		public void Init()
		{
			mocks = new MockRepository();
            factory = new SparkViewFactory();
	        engineContext = mocks.CreateMock<IEngineContext>();
            server = new MockServerUtility();
            request = mocks.CreateMock<IRequest>();
            response = mocks.CreateMock<IResponse>();

            controller = mocks.CreateMock<IController>();
	        controllerContext = mocks.CreateMock<IControllerContext>();
	        //server = mocks.CreateMock<IServerUtility>();
	        routingEngine = mocks.CreateMock<IRoutingEngine>();
	        output = new StringWriter();
	        helpers = new HelperDictionary();

	        propertyBag = new Dictionary<string, object>();
            flash = new Flash();
            session = new Dictionary<string, object>();
            requestParams = new NameValueCollection();
            contextItems = new Dictionary<string, object>();


	        SetupResult.For(engineContext.Server).Return(server);
            SetupResult.For(engineContext.Request).Return(request);
            SetupResult.For(engineContext.Response).Return(response);
	        SetupResult.For(engineContext.CurrentController).Return(controller);
            SetupResult.For(engineContext.CurrentControllerContext).Return(controllerContext);
            SetupResult.For(engineContext.Flash).Return(flash);
            SetupResult.For(engineContext.Session).Return(session);
	        SetupResult.For(engineContext.Items).Return(contextItems);

            SetupResult.For(request.Params).Return(requestParams);
	        
	        SetupResult.For(controllerContext.LayoutNames).Return(new [] {"default"});
	        SetupResult.For(controllerContext.Helpers).Return(helpers);
	        SetupResult.For(controllerContext.PropertyBag).Return(propertyBag);

	        SetupResult.For(routingEngine.IsEmpty).Return(true);

            

            var urlBuilder = new TestUrlBuilder(server, routingEngine);

	        var serviceProvider = mocks.CreateMock<IServiceProvider>();
            var viewSourceLoader = new FileAssemblyViewSourceLoader("Views");
            SetupResult.For(serviceProvider.GetService(typeof(IViewSourceLoader))).Return(viewSourceLoader);
            SetupResult.For(serviceProvider.GetService(typeof(ILoggerFactory))).Return(new NullLogFactory());
            SetupResult.For(serviceProvider.GetService(typeof(ISparkViewEngine))).Return(null);
            SetupResult.For(serviceProvider.GetService(typeof(IUrlBuilder))).Return(urlBuilder);
            mocks.Replay(serviceProvider);

	        SetupResult.For(engineContext.GetService(null)).IgnoreArguments().Do(
	            new Func<Type, object>(serviceProvider.GetService));

            factory.Service(serviceProvider);


	        manager = new DefaultViewEngineManager();
            manager.RegisterEngineForExtesionLookup(factory);
            manager.RegisterEngineForView(factory);
        }

        class TestUrlBuilder : DefaultUrlBuilder
        {
            public TestUrlBuilder(IServerUtility server, IRoutingEngine engine) : base(server, engine)
            {
            }

            public override UrlParts CreateUrlPartsBuilder(UrlInfo current, UrlBuilderParameters parameters, System.Collections.IDictionary routeParameters)
            {
                return base.CreateUrlPartsBuilder(current, parameters, routeParameters);
            }
        }

        void InitUrlInfo(string area, string controller, string action)
        {
            UrlInfo urlInfo = new UrlInfo(area, controller, action, "/", "castle");
            SetupResult.For(engineContext.UrlInfo).Return(urlInfo);
            
            RouteMatch routeMatch = new RouteMatch();
            SetupResult.For(controllerContext.RouteMatch).Return(routeMatch);
        }

	    void ContainsInOrder(string content, params string[] values)
        {
            int index = 0;
            foreach (string value in values)
            {
                int nextIndex = content.IndexOf(value, index);
                Assert.GreaterOrEqual(nextIndex, 0, string.Format("Looking for {0}", value));
                index = nextIndex + value.Length;
            }
        }

		[Test]
		public void ExtensionIsXml()
		{
            mocks.ReplayAll();
            Assert.AreEqual("xml", this.factory.ViewFileExtension);
		}

		[Test]
		public void ProcessBasicTemplate()
		{
            mocks.ReplayAll();
            manager.Process("Home\\Index", output, engineContext, controller, controllerContext);
            Assert.That(output.ToString().Contains("<h1>Simple test</h1>"));
		}

        [Test]
        public void ContextAndControllerContextAvailable()
        {
            mocks.ReplayAll();
            manager.Process("Home\\Index", output, engineContext, controller, controllerContext);
            var entry = factory.Engine.GetEntry("Home", "Index", "default");
            var view = (SparkView)entry.CreateInstance();
            var result = view.RenderView(engineContext, controllerContext);
            Assert.AreEqual(result, output.ToString());
            Assert.AreSame(engineContext, view.Context);
            Assert.AreSame(controllerContext, view.ControllerContext);
        }

        [Test]
        public void HelperModelDictionaries()
        {
            InitUrlInfo("", "Home", "Index");

            mocks.ReplayAll();
            helpers.Add(new FormHelper(engineContext));
            helpers.Add(new UrlHelper(engineContext));
            manager.Process("Home\\HelperModelDictionaries", output, engineContext, controller, controllerContext);
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
            manager.Process("Home\\PropertyBagViewdata", output, engineContext, controller, controllerContext);
            ContainsInOrder(output.ToString(),
                            "<p>foo:baaz</p>",
                            "<p>bar:7</p>",
                            "<p>bar+4:11</p>");
        }
	}

}
