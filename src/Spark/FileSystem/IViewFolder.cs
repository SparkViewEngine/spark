using System.Collections.Generic;
using System.IO;

namespace Spark.FileSystem
{
	public interface IViewFolder
	{
		IViewSource GetViewSource(string path);
		IList<string> ListViews(string path);
		bool HasView(string path);
	}

	public interface IViewSource
	{
		long LastModified { get; }
		Stream OpenViewStream();
	}
}
