using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.FileSystem;
using Spark.Tests.Models;
using Spark.Tests.Stubs;

namespace Spark.Tests
{
	[TestFixture, Category("SparkViewEngine")]
	public class SparkViewFactoryTester
	{
		private MockRepository mocks;
		//private HttpContextBase context;
		//private HttpRequestBase request;
		//private HttpResponseBase response;
		//private IController controller;
		//private RouteData routeData;

		//		private 
		//		private SparkViewEngine factory = new SparkViewEngine(new FileSystemViewSourceLoader("SparkViewEngine\\Views"), new ParserFactory());

		private StubViewFactory factory;
		private SparkViewEngine engine;
		private StringBuilder sb;

		[SetUp]
		public void Init()
		{
			// clears cache
			CompiledViewHolder.Current = null;


			engine = new SparkViewEngine("Spark.Tests.Stubs.StubSparkView", new FileSystemViewFolder("Views"));
			factory = new StubViewFactory { Engine = engine };

			sb = new StringBuilder();

			// reset routes
			//RouteTable.Routes.Clear();
			//RouteTable.Routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
			//                          {
			//                              Defaults = new RouteValueDictionary(new { action = "Index", id = "" })
			//                          });

			mocks = new MockRepository();
			//context = mocks.DynamicHttpContextBase();
			//response = context.Response;
			//request = context.Request;
			//SetupResult.For(request.ApplicationPath).Return("/");
			//SetupResult.For(response.ApplyAppPathModifier("")).IgnoreArguments().Do(new Func<string, string>(path => path));

			//Expect.Call(() => response.Write(""))
			//    .IgnoreArguments()
			//    .Do(new writedelegate(onwrite));

			////            SetupResult.For(delegate() { response.Write(null); }).IgnoreArguments().Callback(onwrite);

			//controller = mocks.DynamicMock<IController>();
			//sb = new StringBuilder();

			//routeData = new RouteData();
			//routeData.Values.Add("controller", "Home");
			//routeData.Values.Add("action", "Index");

			//factory = new SparkViewEngine(new FileSystemViewSourceLoader("SparkViewEngine\\Views"), new ParserFactory());

		}
		//delegate void writedelegate(string data);
		

		//void onwrite(string data)
		//{
		//    sb.Append(data);
		//}

		StubViewContext MakeViewContext(string viewName, string masterName)
		{
			return MakeViewContext(viewName, masterName, null);
		}

		StubViewContext MakeViewContext(string viewName, string masterName, object viewData)
		{
			return new StubViewContext { ControllerName = "Home", ViewName = viewName, MasterName = masterName, Output = sb };
			//return new StubViewContext(context, routeData, controller, viewName, masterName, new ViewDataDictionary(viewData), null);
		}


		[Test]
		public void RenderPlainView()
		{
			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("index", null));

			mocks.VerifyAll();
		}


		[Test]
		public void ForEachTest()
		{
			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("foreach", null));

			mocks.VerifyAll();

			string content = sb.ToString();
			Assert.That(content.Contains(@"<li class=""odd"">1: foo</li>"));
			Assert.That(content.Contains(@"<li class=""even"">2: bar</li>"));
			Assert.That(content.Contains(@"<li class=""odd"">3: baaz</li>"));
		}


		[Test]
		public void GlobalSetTest()
		{

			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("globalset", null));

			mocks.VerifyAll();

			string content = sb.ToString();
			Assert.That(content.Contains("<p>default: Global set test</p>"));
			Assert.That(content.Contains("<p>7==7</p>"));
		}

		[Test]
		public void MasterTest()
		{
			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("childview", "layout"));

			mocks.VerifyAll();
			string content = sb.ToString();
			Assert.That(content.Contains("<title>Standalone Index View</title>"));
			Assert.That(content.Contains("<h1>Standalone Index View</h1>"));
			Assert.That(content.Contains("<p>no header by default</p>"));
			Assert.That(content.Contains("<p>no footer by default</p>"));
		}

		[Test]
		public void CaptureNamedContent()
		{

			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("namedcontent", "layout"));

			mocks.VerifyAll();
			string content = sb.ToString();
			Assert.That(content.Contains("<p>main content</p>"));
			Assert.That(content.Contains("<p>this is the header</p>"));
			Assert.That(content.Contains("<p>footer part one</p>"));
			Assert.That(content.Contains("<p>footer part two</p>"));
		}

		//[Test]
		//public void HtmlHelperWorksOnItsOwn()
		//{
		//    mocks.ReplayAll();

		//    var viewContext = MakeViewContext("helpers", null);
		//    var html = new HtmlHelper(viewContext, new ViewDataContainer { ViewData = viewContext.ViewData });
		//    var link = html.ActionLink("hello", "world");
		//    response.Write(link);

		//    mocks.VerifyAll();

		//    Assert.AreEqual("<a href=\"/Home/world\">hello</a>", link);
		//}



		[Test, Ignore("Library no longer references asp.net mvc directly")]
		public void UsingHtmlHelper()
		{

			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("helpers", null));

			mocks.VerifyAll();
			string content = sb.ToString();
			Assert.That(content.Contains("<p><a href=\"/Home/Sort\">Click me</a></p>"));
			Assert.That(content.Contains("<p>foo&gt;bar</p>"));
		}

		[Test]
		public void UsingPartialFile()
		{
			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("usingpartial", null));

			mocks.VerifyAll();
			string content = sb.ToString();
			Assert.That(content.Contains("<li>Partial where x=\"zero\"</li>"));
			Assert.That(content.Contains("<li>Partial where x=\"one\"</li>"));
			Assert.That(content.Contains("<li>Partial where x=\"two\"</li>"));
			Assert.That(content.Contains("<li>Partial where x=\"three\"</li>"));
			Assert.That(content.Contains("<li>Partial where x=\"four\"</li>"));
		}

		[Test]
		public void UsingPartialFileImplicit()
		{
			mocks.ReplayAll();

			factory.RenderView(MakeViewContext("usingpartialimplicit", null));

			mocks.VerifyAll();
			string content = sb.ToString();
			Assert.That(content.Contains("<li class=\"odd\">one</li>"));
			Assert.That(content.Contains("<li class=\"even\">two</li>"));
		}


		[Test, Ignore("Library no longer references asp.net mvc directly")]
		public void DeclaringViewDataAccessor()
		{
			mocks.ReplayAll();
			var comments = new[] { new Comment { Text = "foo" }, new Comment { Text = "bar" } };
			var viewContext = MakeViewContext("viewdata", null, new { Comments = comments, Caption = "Hello world" });

			factory.RenderView(viewContext);

			mocks.VerifyAll();
			string content = sb.ToString();
			Assert.That(content.Contains("<h1>Hello world</h1>"));
			Assert.That(content.Contains("<p>foo</p>"));
			Assert.That(content.Contains("<p>bar</p>"));
		}

		[Test]
		public void MasterEmptyByDefault()
		{
			var viewFolder = mocks.CreateMock<IViewFolder>();
			Expect.Call(viewFolder.HasView("Shared\\Application.xml")).Return(false);
			SetupResult.For(viewFolder.HasView("Shared\\Foo.xml")).Return(false);

			engine.ViewFolder = viewFolder;

			mocks.ReplayAll();

			var key = engine.CreateKey("Foo", "Baaz", null);

			Assert.AreEqual("Foo", key.ControllerName);
			Assert.AreEqual("Baaz", key.ViewName);
			Assert.IsEmpty(key.MasterName);
		}

		[Test]
		public void MasterApplicationIfPresent()
		{
			var viewFolder = mocks.CreateMock<IViewFolder>();
			Expect.Call(viewFolder.HasView("Shared\\Application.xml")).Return(true);
			SetupResult.For(viewFolder.HasView("Shared\\Foo.xml")).Return(false);

			engine.ViewFolder = viewFolder;


			mocks.ReplayAll();

			var key = engine.CreateKey("Foo", "Baaz", null);


			Assert.AreEqual("Foo", key.ControllerName);
			Assert.AreEqual("Baaz", key.ViewName);
			Assert.AreEqual("Application", key.MasterName);
		}

		[Test]
		public void MasterForControllerIfPresent()
		{
			var viewFolder = mocks.CreateMock<IViewFolder>();
			SetupResult.For(viewFolder.HasView("Shared\\Application.xml")).Return(true);
			SetupResult.For(viewFolder.HasView("Shared\\Foo.xml")).Return(true);

			engine.ViewFolder = viewFolder;

			mocks.ReplayAll();


			var key = engine.CreateKey("Foo", "Baaz", null);


			Assert.AreEqual("Foo", key.ControllerName);
			Assert.AreEqual("Baaz", key.ViewName);
			Assert.AreEqual("Foo", key.MasterName);
		}


		[Test]
		public void UsingNamespace()
		{
			mocks.ReplayAll();
			var viewContext = MakeViewContext("usingnamespace", null);

			factory.RenderView(viewContext);

			mocks.VerifyAll();
			string content = sb.ToString();
			Assert.That(content.Contains("<p>Foo</p>"));
			Assert.That(content.Contains("<p>Bar</p>"));
			Assert.That(content.Contains("<p>Hello</p>"));
		}
	}
}