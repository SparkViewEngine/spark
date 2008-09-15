using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    public class JavascriptGeneratedCodeVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;

        public JavascriptGeneratedCodeVisitor(StringBuilder source)
        {
            _source = source;
        }

        protected override void Visit(SendLiteralChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Text))
                return;

            var text =
                chunk.Text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace(
                    "\"", "\\\"");
            _source.Append("Output.Write(\"").Append(text).AppendLine("\");");
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            _source.Append("Output.Write(").Append(chunk.Code).AppendLine(");");
        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            throw new NotImplementedException();
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Value))
            {
                _source.Append("var ").Append(chunk.Name).AppendLine(" = null;");
            }
            else
            {
                _source.Append("var ").Append(chunk.Name).Append(" = ").Append(chunk.Value).AppendLine(";");
            }
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            _source.Append(chunk.Name).Append(" = ").Append(chunk.Value).AppendLine(";");
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            switch (chunk.Type)
            {
                case ConditionalType.If:
                    {
                        _source.Append("if (").Append(chunk.Condition).AppendLine(") {");
                        Accept(chunk.Body);
                        _source.AppendLine("}");
                    }
                    break;
                case ConditionalType.ElseIf:
                    {
                        _source.Append("else if (").Append(chunk.Condition).AppendLine(") {");
                        Accept(chunk.Body);
                        _source.AppendLine("}");
                    }
                    break;
                case ConditionalType.Else:
                    {
                        _source.AppendLine("else {");
                        Accept(chunk.Body);
                        _source.AppendLine("}");
                    }
                    break;
            }
        }

        protected override void Visit(ForEachChunk chunk)
        {
            var inspector = new ForEachInspector(chunk.Code);
            if (inspector.Recognized)
            {
                _source
                    .Append("for (var __iter__")
                    .Append(inspector.VariableName)
                    .Append(" in ")
                    .Append(inspector.CollectionCode)
                    .AppendLine(") {");
                _source
                    .Append("var ")
                    .Append(inspector.VariableName)
                    .Append(" = ")
                    .Append(inspector.CollectionCode)
                    .Append("[__iter__")
                    .Append(inspector.VariableName)
                    .AppendLine("];");
                Accept(chunk.Body);
                _source.Append("}");
            }
            else
            {
                _source.Append("for (").Append(chunk.Code).AppendLine(") {");
                Accept(chunk.Body);
                _source.Append("}");
            }
        }

        public RenderPartialChunk OuterPartial { get; set; }
        protected override void Visit(RenderPartialChunk chunk)
        {
            var priorOuterPartial = OuterPartial;
            OuterPartial = chunk;
            Accept(chunk.FileContext.Contents);
            OuterPartial = priorOuterPartial;
        }

        protected override void Visit(RenderSectionChunk chunk)
        {
            throw new NotImplementedException();
        }

        protected override void Visit(ScopeChunk chunk)
        {
            _source.AppendLine("{");
            base.Visit(chunk);
            _source.AppendLine("}");
        }

        protected override void Visit(ContentChunk chunk)
        {
            _source.Append("OutputScope('").Append(chunk.Name).AppendLine("'); {");
            Accept(chunk.Body);
            _source.AppendLine("} DisposeOutputScope();");
        }

        protected override void Visit(UseContentChunk chunk)
        {
            _source.Append("if (Content['").Append(chunk.Name).AppendLine("']) {");
            _source.Append("Output.Write(Content['").Append(chunk.Name).AppendLine("']);}");
            if (chunk.Default.Count != 0)
            {
                _source.AppendLine("else {");
                Accept(chunk.Default);
                _source.AppendLine("}");
            }
        }

        protected override void Visit(ContentSetChunk chunk)
        { 
            _source.AppendLine("OutputScope(new StringWriter()); {");
            Accept(chunk.Body);
            switch (chunk.AddType)
            {
                case ContentAddType.AppendAfter:
                    //format = "{0} = {0} + Output.ToString();";
                    _source
                        .Append(chunk.Variable)
                        .Append(" = ")
                        .Append(chunk.Variable)
                        .AppendLine(" + Output.toString();");
                    break;
                case ContentAddType.InsertBefore:
                    //format = "{0} = Output.ToString() + {0};";
                    _source
                        .Append(chunk.Variable)
                        .Append(" = Output.toString() + ")
                        .Append(chunk.Variable)
                        .AppendLine(";");
                    break;
                default:
                    //format = "{0} = Output.ToString();";
                     _source
                        .Append(chunk.Variable)
                        .Append(" = Output.toString();");
                    break;
            }

            _source.AppendLine("DisposeOutputScope();}");
        }

        protected override void Visit(MacroChunk chunk)
        {
            throw new NotImplementedException();
        }         
    }
}
