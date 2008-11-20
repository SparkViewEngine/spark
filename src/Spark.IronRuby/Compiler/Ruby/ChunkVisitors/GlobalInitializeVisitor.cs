using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;

namespace Spark.IronRuby.Compiler.Ruby.ChunkVisitors
{
    public class GlobalInitializeVisitor : ChunkVisitor
    {
        private readonly SourceWriter _source;

        public GlobalInitializeVisitor(SourceWriter sourceWriter)
        {
            _source = sourceWriter;
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
            _source.Write("@").Write(chunk.Name).Write("=").WriteLine(chunk.Value);
        }
    }
}