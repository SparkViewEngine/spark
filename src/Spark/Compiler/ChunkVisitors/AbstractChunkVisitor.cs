using System;
using System.Collections.Generic;

namespace Spark.Compiler.ChunkVisitors
{
	public abstract class AbstractChunkVisitor
	{
		public void Accept(IList<Chunk> chunks)
		{
			if (chunks == null) throw new ArgumentNullException("chunks");

			foreach (var chunk in chunks)
				Accept(chunk);
		}

		public void Accept(Chunk chunk)
		{
			if (chunk == null) throw new ArgumentNullException("chunk");

			if (chunk is SendLiteralChunk)
			{
				Visit((SendLiteralChunk)chunk);
			}
			else if (chunk is LocalVariableChunk)
			{
				Visit((LocalVariableChunk)chunk);
			}
			else if (chunk is SendExpressionChunk)
			{
				Visit((SendExpressionChunk)chunk);
			}
			else if (chunk is ForEachChunk)
			{
				Visit((ForEachChunk)chunk);
			}
			else if (chunk is ScopeChunk)
			{
				Visit((ScopeChunk)chunk);
			}
			else if (chunk is GlobalVariableChunk)
			{
				Visit((GlobalVariableChunk)chunk);
			}
			else if (chunk is AssignVariableChunk)
			{
				Visit((AssignVariableChunk)chunk);
			}
			else if (chunk is ContentChunk)
			{
				Visit((ContentChunk)chunk);
			}
			else if (chunk is UseContentChunk)
			{
				Visit((UseContentChunk)chunk);
			}
			else if (chunk is RenderPartialChunk)
			{
				Visit((RenderPartialChunk)chunk);
			}
			else if (chunk is ViewDataChunk)
			{
				Visit((ViewDataChunk)chunk);
			}
			else if (chunk is ViewDataModelChunk)
			{
				Visit((ViewDataModelChunk)chunk);
			}
			else if (chunk is UseNamespaceChunk)
			{
				Visit((UseNamespaceChunk)chunk);
			}
			else
			{
				throw new CompilerException(string.Format("Unknown chunk type {0}", chunk.GetType().Name));
			}
		}

		protected abstract void Visit(ViewDataModelChunk chunk);

		protected abstract void Visit(ViewDataChunk chunk);
		protected abstract void Visit(RenderPartialChunk chunk);
		protected abstract void Visit(AssignVariableChunk chunk);
		protected abstract void Visit(UseContentChunk chunk);
		protected abstract void Visit(GlobalVariableChunk chunk);
		protected abstract void Visit(ScopeChunk chunk);
		protected abstract void Visit(ForEachChunk chunk);
		protected abstract void Visit(SendLiteralChunk chunk);
		protected abstract void Visit(LocalVariableChunk chunk);
		protected abstract void Visit(SendExpressionChunk chunk);
		protected abstract void Visit(ContentChunk chunk);
		protected abstract void Visit(UseNamespaceChunk chunk);
	}
}