using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    public class JavascriptGlobalMembersVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;

        public JavascriptGlobalMembersVisitor(StringBuilder source)
        {
            _source = source;
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
            _source
                .Append(chunk.Name)
                .Append(":")
                .Append(chunk.Value)
                .AppendLine(",");
        }
    }
}
