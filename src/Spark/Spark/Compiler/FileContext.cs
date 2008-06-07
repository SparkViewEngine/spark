using System.Collections.Generic;

namespace MvcContrib.SparkViewEngine.Compiler
{
	public class FileContext
	{
		public string ViewSourcePath { get; set; }

		public IList<Chunk> Contents;
	}
}
