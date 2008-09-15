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

        protected override void Visit(MacroChunk chunk)
        {
            _source.Append("function ").Append(chunk.Name).Append("(");
            string delimiter = "";
            foreach (var parameter in chunk.Parameters)
            {
                _source.Append(delimiter).Append(parameter.Name);
                delimiter = ", ";
            }
            _source.AppendLine(") {var __output__ = new StringWriter(); OutputScope(__output__);");
            var codeVisitor = new JavascriptGeneratedCodeVisitor(_source);
            codeVisitor.Accept(chunk.Body);
            _source.AppendLine("DisposeOutputScope(); return __output__.toString();}");            
        }         
    }
}
