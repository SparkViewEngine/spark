using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine.Compiler.ChunkVisitors;

namespace MvcContrib.SparkViewEngine.Compiler.ChunkVisitors
{
	public class GlobalMembersVisitor : ChunkVisitor
	{
		private readonly StringBuilder _source;

		public GlobalMembersVisitor(StringBuilder output)
		{
			_source = output;
		}

		protected override void Visit(GlobalVariableChunk chunk)
		{
			_source.AppendLine(string.Format("{0} {1}={2};", chunk.Type ?? "object", chunk.Name, chunk.Value));
		}

		protected override void Visit(ViewDataChunk chunk)
		{
			_source.AppendLine(string.Format("{0} {1} {{get {{return ({0})ViewData[\"{1}\"];}}}}", chunk.Type ?? "object", chunk.Name));
		}

	}
}