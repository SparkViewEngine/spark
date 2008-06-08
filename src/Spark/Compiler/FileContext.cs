using System.Collections.Generic;

namespace Spark.Compiler
{
	public class FileContext
	{
		public string ViewSourcePath { get; set; }

		public IList<Chunk> Contents;
	}
}
