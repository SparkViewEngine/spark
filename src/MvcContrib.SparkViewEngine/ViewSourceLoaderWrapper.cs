using System.Collections.Generic;
using System.IO;
using MvcContrib.ViewFactories;
using Spark.FileSystem;
using IViewSource=Spark.FileSystem.IViewFile;

namespace MvcContrib.SparkViewEngine
{
	public class ViewSourceLoaderWrapper : IViewFolder
	{
		public ViewSourceLoaderWrapper(IViewSourceLoader viewSourceLoader)
		{
			ViewSourceLoader = viewSourceLoader;
		}

		public IViewSourceLoader ViewSourceLoader { get; set; }

		public bool HasView(string path)
		{
			return ViewSourceLoader.HasView(path);
		}

		public IList<string> ListViews(string path)
		{
			return ViewSourceLoader.ListViews(path);
		}

		public IViewSource GetViewSource(string path)
		{
			return new ViewSourceWrapper(ViewSourceLoader.GetViewSource(path));
		}

		internal class ViewSourceWrapper : IViewSource
		{
			private readonly ViewFactories.IViewSource _source;

			public ViewSourceWrapper(ViewFactories.IViewSource source)
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