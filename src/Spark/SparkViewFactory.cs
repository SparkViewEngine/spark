using System.Collections.Generic;
using System.IO;
using MvcContrib.SparkViewEngine.Compiler;
using MvcContrib.SparkViewEngine.Compiler.NodeVisitors;
using MvcContrib.SparkViewEngine.Parser;
using Spark.FileSystem;

namespace MvcContrib.SparkViewEngine
{
	public interface ISparkViewFactory
	{
		ISparkView CreateInstance(string controllerName, string viewName, string masterName);
	}

	public class SparkViewFactory : ISparkViewFactory
	{
		//public SparkViewFactory()
		//    : this(new FileSystemViewSourceLoader(), new ParserFactory())
		//{

		//}

		public SparkViewFactory(string baseClass, IFileSystem fileSystem)
		{
			BaseClass = baseClass;
			FileSystem = fileSystem;
		}

		public string BaseClass { get; set; }
		public IFileSystem FileSystem { get; set; }


		public ISparkView CreateInstance(string controllerName, string viewName, string masterName)
		{
			var key = CreateKey(controllerName, viewName, masterName);
			var entry = CompiledViewHolder.Current.Lookup(key);
			if (entry == null)
			{
				entry = CreateEntry(key);
				CompiledViewHolder.Current.Store(entry);
			}

			return entry.Compiler.CreateInstance();
		}

		//public void RenderView(ViewContext viewContext)
		//{
		//    var key = CreateKey(viewContext);

		//    var entry = CompiledViewHolder.Current.Lookup(key);
		//    if (entry == null)
		//    {
		//        entry = CreateEntry(key);
		//        CompiledViewHolder.Current.Store(entry);
		//    }

		//    var view = entry.Compiler.CreateInstance();
		//    var content = view.RenderView(viewContext);

		//    viewContext.HttpContext.Response.Write(content);
		//}

		//public CompiledViewHolder.Key CreateKey(ViewContext viewContext)
		//{
		//    return CreateKey(
		//        viewContext.RouteData.GetRequiredString("controller"),
		//        viewContext.ViewName,
		//        viewContext.MasterName);
		//}

		public CompiledViewHolder.Key CreateKey(string controllerName, string viewName, string masterName)
		{
			var key = new CompiledViewHolder.Key
			{
				ControllerName = controllerName ?? string.Empty,
				ViewName = viewName ?? string.Empty,
				MasterName = masterName ?? string.Empty
			};

			if (key.MasterName == string.Empty)
			{
				if (FileSystem.HasView(string.Format("Shared\\{0}.xml", key.ControllerName)))
				{
					key.MasterName = key.ControllerName;
				}
				else if (FileSystem.HasView("Shared\\Application.xml"))
				{
					key.MasterName = "Application";
				}
			}
			return key;
		}

		public CompiledViewHolder.Entry CreateEntry(CompiledViewHolder.Key key)
		{
			var entry = new CompiledViewHolder.Entry
							  {
								  Key = key,
								  Loader = new ViewLoader { FileSystem = FileSystem },
								  Compiler = new ViewCompiler(BaseClass)
							  };

			var viewChunks = entry.Loader.Load(key.ControllerName, key.ViewName);

			IList<Chunk> masterChunks = new Chunk[0];
			if (!string.IsNullOrEmpty(key.MasterName))
				masterChunks = entry.Loader.Load("Shared", key.MasterName);

			entry.Compiler.CompileView(viewChunks, masterChunks);

			return entry;
		}

	}
}
