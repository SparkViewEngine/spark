using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    /// <summary>
    /// Abstract visitor which passes the properties that may contain code
    /// through a processing method. Used for things like converting anonymous
    /// type allocation from csharp syntax to javascript syntax.
    /// </summary>
    public abstract class CodeProcessingChunkVisitor : ChunkVisitor
    {
        public abstract Snippets Process(Chunk chunk, Snippets code);

        
        protected override void Visit(GlobalVariableChunk chunk)
        {
            chunk.Value = Process(chunk, chunk.Value);
            base.Visit(chunk);
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            chunk.Value = Process(chunk, chunk.Value);
            base.Visit(chunk);
        }

        protected override void Visit(DefaultVariableChunk chunk)
        {
            chunk.Value = Process(chunk, chunk.Value);
            base.Visit(chunk);
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            chunk.Value = Process(chunk, chunk.Value);
            base.Visit(chunk);
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            chunk.Code = Process(chunk, chunk.Code);
            base.Visit(chunk);
        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            chunk.Code = Process(chunk, chunk.Code);
            base.Visit(chunk);
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            chunk.Condition = Process(chunk, chunk.Condition);
            base.Visit(chunk);
        }

        protected override void Visit(ForEachChunk chunk)
        {
            chunk.Code = Process(chunk, chunk.Code);
            base.Visit(chunk);
        }
    }
}