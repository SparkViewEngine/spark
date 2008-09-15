using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    public class JavascriptPostRenderVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;

        public JavascriptPostRenderVisitor(StringBuilder source)
        {
            _source = source;
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
            _source
                .Append("this.")
                .Append(chunk.Name)
                .Append(" = ")
                .Append(chunk.Name)
                .AppendLine(";");
        }
    }
}
