using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Spark.FileSystem
{
	public class FileSystemViewFolder : IViewFolder
	{
		private readonly string _basePath;

		public FileSystemViewFolder (string basePath)
		{
			_basePath = basePath;
		}

		
		public IViewSource GetViewSource(string path)
		{
			string fullPath = Path.Combine(_basePath, path);
			if (!File.Exists(fullPath))
				throw new FileNotFoundException("View source file not found.", fullPath);

			return new FileSystemViewSource(fullPath);
		}

		public IList<string> ListViews(string path)
		{
			var files = Directory.GetFiles(Path.Combine(_basePath, path));
			return files.ToList().ConvertAll(viewPath => Path.GetFileName(viewPath));
		}

		public bool HasView(string path)
		{
			return File.Exists(Path.Combine(_basePath, path));
		}
	}
}
