using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;
using Rhino.Mocks;
using Spark;

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    public class BaseViewComponentTests
    {
        protected DefaultViewComponentFactory viewComponentFactory;
        protected MockRepository mocks;
        protected ControllerContext controllerContext;
        protected StubEngineContext engineContext;
        protected SparkViewFactory factory;
        protected IController controller;

        [SetUp]
        public virtual void Init()
        {
            mocks = new MockRepository();

            var services = new StubMonoRailServices();
            services.ViewSourceLoader = new FileAssemblyViewSourceLoader("MonoRail.Tests.Views");
            services.AddService(typeof(IViewSourceLoader), services.ViewSourceLoader);

            viewComponentFactory = new DefaultViewComponentFactory();
            viewComponentFactory.Initialize();
            services.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            services.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);

            var settings = new SparkSettings().SetDebug(true);
            services.AddService(typeof(ISparkViewEngine), new SparkViewEngine(settings));

            factory = new SparkViewFactory();
            factory.Service(services);

            controller = mocks.CreateMock<IController>();
            controllerContext = new ControllerContext();
            var request = new StubRequest();
            request.FilePath = "";
            var response = new StubResponse();
            engineContext = new StubEngineContext(request, response, new UrlInfo("", "Home", "Index", "/", "castle"));
            engineContext.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            engineContext.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);
        }
    }
}