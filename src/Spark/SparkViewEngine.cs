using System.Collections.Generic;
using Spark.Compiler;
using Spark.Parser;
using Spark;
using Spark.FileSystem;

namespace Spark
{
	public class SparkViewEngine : ISparkViewEngine
	{
		public SparkViewEngine(string baseClass, IViewFolder viewFolder)
		{
			BaseClass = baseClass;
			ViewFolder = viewFolder;
		}

		public string BaseClass { get; set; }
		public IViewFolder ViewFolder { get; set; }


		public ISparkViewEntry GetEntry(string controllerName, string viewName, string masterName)
		{
			var key = CreateKey(controllerName, viewName, masterName);
			return CompiledViewHolder.Current.Lookup(key);
		}

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
				if (ViewFolder.HasView(string.Format("Shared\\{0}.xml", key.ControllerName)))
				{
					key.MasterName = key.ControllerName;
				}
				else if (ViewFolder.HasView("Shared\\Application.xml"))
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
			            		Loader = new ViewLoader { ViewFolder = ViewFolder },
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