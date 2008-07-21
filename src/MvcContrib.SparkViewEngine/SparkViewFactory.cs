using System.Diagnostics;
using System.Web.Mvc;
using MvcContrib.ViewFactories;
using Spark;

namespace MvcContrib.SparkViewEngine
{
	public class SparkViewFactory : IViewEngine
	{
		public SparkViewFactory()
			: this(new FileSystemViewSourceLoader())
		{

		}

		public SparkViewFactory(IViewSourceLoader viewSourceLoader)
		{
			_viewSourceLoaderWrapper = new ViewSourceLoaderWrapper(viewSourceLoader);
			
			Engine = new Spark.SparkViewEngine(
				"MvcContrib.SparkViewEngine.SparkView", 
				_viewSourceLoaderWrapper);
		}

		private readonly ViewSourceLoaderWrapper _viewSourceLoaderWrapper;

		public ISparkViewEngine Engine { get; set; }

		public IViewSourceLoader ViewSourceLoader
		{
			get { return _viewSourceLoaderWrapper.ViewSourceLoader; }
			set { _viewSourceLoaderWrapper.ViewSourceLoader = value; }
		}

        [DebuggerNonUserCode]
		public void RenderView(ViewContext viewContext)
		{
            var descriptor = new SparkViewDescriptor
                                 {
                                     ControllerName = viewContext.RouteData.GetRequiredString("controller"),
                                     ViewName = viewContext.ViewName,
                                     MasterName = viewContext.MasterName
                                 };

            var view = (SparkView) Engine.CreateInstance(descriptor);

			view.RenderView(viewContext);
		}
	}
}
