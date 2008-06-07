using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcContrib.SparkViewEngine.Compiler.ChunkVisitors
{
	public class ChunkVisitor : AbstractChunkVisitor
	{
		protected override void Visit(SendLiteralChunk chunk)
		{
		}

		protected override void Visit(ForEachChunk chunk)
		{
			Accept(chunk.Body);
		}

		protected override void Visit(LocalVariableChunk chunk)
		{
		}

		protected override void Visit(ContentChunk chunk)
		{
			Accept(chunk.Body);
		}

		protected override void Visit(SendExpressionChunk chunk)
		{
		}

		protected override void Visit(ScopeChunk chunk)
		{
			Accept(chunk.Body);
		}

		protected override void Visit(RenderPartialChunk chunk)
		{
		}

		protected override void Visit(ViewDataChunk chunk)
		{
		}

		protected override void Visit(AssignVariableChunk chunk)
		{
		}

		protected override void Visit(GlobalVariableChunk chunk)
		{
		}

		protected override void Visit(UseContentChunk chunk)
		{
			Accept(chunk.Default);
		}

		protected override void Visit(UseNamespaceChunk chunk)
		{
		}

		protected override void Visit(ViewDataModelChunk chunk)
		{

		}
	}
}
