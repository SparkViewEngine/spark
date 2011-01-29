using System.CodeDom.Compiler;

namespace Spark.Compiler
{
	public class BatchCompilerException : CompilerException
	{
		public BatchCompilerException(string message, CompilerResults results) : base(message)
		{
			Results = results;
		}

		public CompilerResults Results { get; set; }
	}
}