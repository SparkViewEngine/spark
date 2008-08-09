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

        public GeneratedCodeVisitor(StringBuilder output)
        {
            _source = output;
        }

        public int Indent { get; set; }

        protected override void Visit(SendLiteralChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Text))
                return;

            _source.Append(' ', Indent).AppendLine("Output.Write(\"" + chunk.Text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\");");
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            _source.Append(' ', Indent).AppendLine(string.Format("Output.Write({0});", chunk.Code));
        }

        protected override void Visit(MacroChunk chunk)
        {

        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            _source.Append(' ', Indent).AppendLine(chunk.Code.Replace("\r", "").Replace("\n", "\r\n"));
        }


        protected override void Visit(LocalVariableChunk chunk)
        {
            _source.Append(' ', Indent).AppendLine(string.Format("{0} {1} = {2};", chunk.Type, chunk.Name, chunk.Value));
        }

        protected override void Visit(ForEachChunk chunk)
        {
            var terms = chunk.Code.Split(' ', '\r', '\n', '\t').ToList();
            var inIndex = terms.IndexOf("in");
            string variableName = (inIndex < 2 ? null : terms[inIndex - 1]);

            if (variableName == null)
            {
                _source.Append(' ', Indent).AppendLine(string.Format("foreach({0})", chunk.Code));
                _source.Append(' ', Indent).AppendLine("{");
                Indent += 4;
                Accept(chunk.Body); 
                Indent -= 4;
                _source.Append(' ', Indent).AppendLine(string.Format("}} //foreach {0}", chunk.Code.Replace("\r", "").Replace("\n", " ")));
            }
            else
            {
                _source.Append(' ', Indent).AppendLine("{");
                _source.Append(' ', Indent + 4).AppendFormat("int {0}Index = 0;\r\n", variableName);
                _source.Append(' ', Indent + 4).AppendFormat("foreach({0})\r\n", chunk.Code);
                _source.Append(' ', Indent + 4).AppendLine("{");
                Indent += 8;
                Accept(chunk.Body);
                Indent -= 8;
                _source.Append(' ', Indent + 8).AppendFormat("++{0}Index;\r\n", variableName);
                _source.Append(' ', Indent + 4).AppendLine("}");
                _source.Append(' ', Indent).AppendFormat("}} //foreach {0}\r\n", chunk.Code.Replace("\r", "").Replace("\n", " "));
            }
        }

        protected override void Visit(ScopeChunk chunk)
        {
            _source.Append(' ', Indent).AppendLine("{");
            Indent += 4;
            Accept(chunk.Body);
            Indent -= 4;
            _source.Append(' ', Indent).AppendLine("}");
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            _source.Append(' ', Indent).AppendLine(string.Format("{0} = {1};", chunk.Name, chunk.Value));
        }


        protected override void Visit(ContentChunk chunk)
        {
            _source.Append(' ', Indent).AppendLine(string.Format("using(OutputScope(\"{0}\"))", chunk.Name));
            _source.Append(' ', Indent).AppendLine("{");
            Indent += 4;
            Accept(chunk.Body);
            Indent -= 4;
            _source.Append(' ', Indent).AppendLine("}");
        }

        protected override void Visit(UseContentChunk chunk)
        {
            _source.Append(' ', Indent).AppendLine(string.Format("if (Content.ContainsKey(\"{0}\"))", chunk.Name));
            _source.Append(' ', Indent).AppendLine("{");
            _source.Append(' ', Indent + 4).AppendLine(string.Format("Output.Write(Content[\"{0}\"]);", chunk.Name));
            _source.Append(' ', Indent).AppendLine("}");
            if (chunk.Default.Count != 0)
            {
                _source.Append(' ', Indent).AppendLine("else");
                _source.Append(' ', Indent).AppendLine("{");
                Indent += 4;
                Accept(chunk.Default);
                Indent -= 4;
                _source.Append(' ', Indent).AppendLine("}");
            }
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

        protected override void Visit(UseAssemblyChunk chunk)
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
                        _source.Append(' ', Indent).AppendLine(string.Format("if ({0})", chunk.Condition));
                        _source.Append(' ', Indent).AppendLine("{");
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        _source.Append(' ', Indent).AppendLine(string.Format("}} // if ({0})",
                                                         chunk.Condition.Replace("\r", "").Replace("\n", " ")));
                    }
                    break;
                case ConditionalType.ElseIf:
                    {
                        _source.Append(' ', Indent).AppendLine(string.Format("else if ({0})", chunk.Condition));
                        _source.Append(' ', Indent).AppendLine("{");
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        _source.Append(' ', Indent).AppendLine(string.Format("}} // else if ({0})",
                                                         chunk.Condition.Replace("\r", "").Replace("\n", " ")));
                    }
                    break;
                case ConditionalType.Else:
                    {
                        _source.Append(' ', Indent).AppendLine("else");
                        _source.Append(' ', Indent).AppendLine("{");
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        _source.Append(' ', Indent).AppendLine("}");
                    }
                    break;
            }
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {

        }

    }
}