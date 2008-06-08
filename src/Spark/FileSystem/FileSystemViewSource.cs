using System;
using System.IO;

namespace Spark.FileSystem
{
	public class FileSystemViewSource : IViewSource
	{
		private string _fullPath;

		public FileSystemViewSource(string fullPath)
		{
			_fullPath = fullPath;
		}

		public long LastModified
		{
			get { return File.GetLastWriteTimeUtc(_fullPath).Ticks; }
		}

		public Stream OpenViewStream()
		{
			return new FileStream(_fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}
	}
}