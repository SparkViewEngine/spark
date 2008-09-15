using System.Collections.Generic;

namespace Spark.Compiler.ChunkVisitors
{
    public interface IChunkVisitor
    {
        void Accept(IList<Chunk> chunks);
        void Accept(Chunk chunk);
    }
}