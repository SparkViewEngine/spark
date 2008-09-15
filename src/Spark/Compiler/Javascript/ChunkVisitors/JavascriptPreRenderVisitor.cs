using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    public class JavascriptPreRenderVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;

        public JavascriptPreRenderVisitor(StringBuilder source)
        {
            _source = source;
        }
        protected override void Visit(GlobalVariableChunk chunk)
        {
            _source
                .Append("var ")
                .Append(chunk.Name)
                .Append(" = this.")
                .Append(chunk.Name)
                .AppendLine(";");
        }

        protected override void Visit(ViewDataChunk chunk)
        {
            _source
                .Append("var ")
                .Append(chunk.Name)
                .Append(" = viewData[\"")
                .Append(chunk.Key)
                .AppendLine("\"];");
        }
    }
}
