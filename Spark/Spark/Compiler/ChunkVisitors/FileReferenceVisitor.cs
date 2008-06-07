using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcContrib.SparkViewEngine.Compiler.ChunkVisitors
{
	public class FileReferenceVisitor : ChunkVisitor
	{
		private readonly IList<RenderPartialChunk> _references = new List<RenderPartialChunk>();

		public IList<RenderPartialChunk> References
		{
			get { return _references; }
		}

		protected override void Visit(RenderPartialChunk chunk)
		{
			References.Add(chunk);
		}
	}
}
