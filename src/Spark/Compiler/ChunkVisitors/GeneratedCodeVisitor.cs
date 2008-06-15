/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.ChunkVisitors
{
    public class GeneratedCodeVisitor : AbstractChunkVisitor
    {
        private readonly StringBuilder _source;
        private int _outputlevel = 0;

        public GeneratedCodeVisitor(StringBuilder output)
        {
            _source = output;
        }

        protected override void Visit(SendLiteralChunk chunk)
        {
            _source.AppendLine("output.Append(\"" + chunk.Text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\");");
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            _source.AppendLine(string.Format("output.Append({0});", chunk.Code));
        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            _source.AppendLine(chunk.Code.Replace("\r", "").Replace("\n", "\r\n"));
        }


        protected override void Visit(LocalVariableChunk chunk)
        {
            _source.AppendLine(string.Format("{0} {1} = {2};", chunk.Type, chunk.Name, chunk.Value));
        }

        protected override void Visit(ForEachChunk chunk)
        {
            _source.AppendLine(string.Format("foreach({0}) {{", chunk.Code));
            Accept(chunk.Body);
            _source.AppendLine(string.Format("}} //foreach {0}", chunk.Code.Replace("\r", "").Replace("\n", " ")));
        }

        protected override void Visit(ScopeChunk chunk)
        {
            _source.AppendLine("{");
            Accept(chunk.Body);
            _source.AppendLine("}");
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            _source.AppendLine(string.Format("{0} = {1};", chunk.Name, chunk.Value));
        }


        protected override void Visit(ContentChunk chunk)
        {
            _source.AppendLine(string.Format("{{ var __pushOutput{0}__ = output;", _outputlevel));
            _source.AppendLine(string.Format("output = BindContent(\"{0}\");", chunk.Name));
            _outputlevel += 1;
            Accept(chunk.Body);
            _outputlevel -= 1;
            _source.AppendLine(string.Format("output = __pushOutput{0}__; }}", _outputlevel));
        }

        protected override void Visit(UseContentChunk chunk)
        {
            _source.AppendLine(string.Format("if (Content.ContainsKey(\"{0}\")) {{", chunk.Name));
            _source.AppendLine(string.Format("output.Append(Content[\"{0}\"]);", chunk.Name));
            _source.AppendLine("} else {");
            Accept(chunk.Default);
            _source.AppendLine("}");
        }

        protected override void Visit(RenderPartialChunk chunk)
        {
            Accept(chunk.FileContext.Contents);
        }

        protected override void Visit(ViewDataChunk chunk)
        {

        }

        protected override void Visit(UseNamespaceChunk chunk)
        {

        }

        protected override void Visit(ExtensionChunk chunk)
        {
            chunk.Extension.VisitChunk(this, OutputLocation.RenderMethod, chunk.Body, _source);
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            switch (chunk.Type)
            {
                case ConditionalType.If:
                    {
                        _source.AppendLine(string.Format("if ({0}) {{", chunk.Condition));
                        Accept(chunk.Body);
                        _source.AppendLine(string.Format("}} // if ({0})",
                                                         chunk.Condition.Replace("\r", "").Replace("\n", " ")));
                    }
                    break;
                case ConditionalType.ElseIf:
                    {
                        _source.AppendLine(string.Format("else if ({0}) {{", chunk.Condition));
                        Accept(chunk.Body);
                        _source.AppendLine(string.Format("}} // else if ({0})",
                                                         chunk.Condition.Replace("\r", "").Replace("\n", " ")));
                    }
                    break;
                case ConditionalType.Else:
                    {
                        _source.AppendLine(string.Format("else {{", chunk.Condition));
                        Accept(chunk.Body);
                        _source.AppendLine("}");
                    }
                    break;
            }
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {

        }

    }
}