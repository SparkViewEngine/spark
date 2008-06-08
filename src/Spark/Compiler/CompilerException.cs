using System;

namespace Spark.Compiler
{
	public class CompilerException : SystemException
	{
		public CompilerException(string message)
			: base(message)
		{

		}
	}
}