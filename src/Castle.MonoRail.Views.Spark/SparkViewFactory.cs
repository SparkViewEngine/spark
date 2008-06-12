using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Spark;
using Spark.FileSystem;
using IViewSource=Spark.FileSystem.IViewSource;

namespace Castle.MonoRail.Views.Spark
{
	public class SparkViewFactory : ViewEngineBase, IViewFolder
	{
		public SparkViewFactory()
		{			
		}

        public override void Service(IServiceProvider provider)
        {
            base.Service(provider);

            Engine = (ISparkViewEngine)provider.GetService(typeof (ISparkViewEngine));
            if (Engine == null)
                Engine = new SparkViewEngine(typeof(SparkView).FullName, this);
        }

		public ISparkViewEngine Engine { get; set; }

		public override string ViewFileExtension
		{
			get { return "xml"; }
		}

		public override bool HasTemplate(string templateName)
		{
			return base.HasTemplate(Path.ChangeExtension(templateName, ViewFileExtension));
		}

		public override void Process(string templateName, TextWriter output, IEngineContext context, IController controller,
		                             IControllerContext controllerContext)
		{
			var viewName = Path.GetFileName(templateName);
			var location = Path.GetDirectoryName(templateName);
			
			string masterName = null;
			if (controllerContext.LayoutNames != null)
				masterName = string.Join(" ", controllerContext.LayoutNames);

			var view = (SparkView)Engine.CreateInstance(location, viewName, masterName);
            view.Contextualize(context, controllerContext);
			output.Write(view.RenderView());
		}

		public override void Process(string templateName, string layoutName, TextWriter output,
		                             IDictionary<string, object> parameters)
		{
			throw new NotImplementedException();
		}

		public override void ProcessPartial(string partialName, TextWriter output, IEngineContext context,
		                                    IController controller, IControllerContext controllerContext)
		{
			throw new NotImplementedException();
		}

		public override void RenderStaticWithinLayout(string contents, IEngineContext context, IController controller,
		                                              IControllerContext controllerContext)
		{
			throw new NotImplementedException();
		}

		public override bool SupportsJSGeneration
		{
			get { return false; }
		}

		public override string JSGeneratorFileExtension
		{
			get { return null; }
		}

		public override object CreateJSGenerator(JSCodeGeneratorInfo generatorInfo, IEngineContext context,
												 IController controller, IControllerContext controllerContext)
		{
			throw new NotImplementedException();
		}

		public override void GenerateJS(string templateName, TextWriter output, JSCodeGeneratorInfo generatorInfo,
										IEngineContext context, IController controller, IControllerContext controllerContext)
		{
			throw new NotImplementedException();
		}

		IList<string> IViewFolder.ListViews(string path)
		{
			return ViewSourceLoader.ListViews(path);
		}

		bool IViewFolder.HasView(string path)
		{
			return ViewSourceLoader.HasSource(path);
		}

		IViewSource IViewFolder.GetViewSource(string path)
		{
			return new ViewSource(ViewSourceLoader.GetViewSource(path));
		}

		private class ViewSource : IViewSource
		{
			private readonly Framework.IViewSource _source;

			public ViewSource(Framework.IViewSource source)
			{
				_source = source;
			}

			public long LastModified
			{
				get { return _source.LastModified; }
			}

			public Stream OpenViewStream()
			{
				return _source.OpenViewStream();
			}
		}

	}
}
