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
using System.IO;
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

        private StringBuilder AppendIndent()
        {
            return _source.Append(' ', Indent);
        }

        private StringBuilder CodeIndent(Chunk chunk)
        {
            if (chunk != null && chunk.Position != null)
                return _source.AppendFormat("#line {0} \"{1}\"", chunk.Position.Line, chunk.Position.SourceContext.FileName).AppendLine().Append(' ', chunk.Position.Column - 1);

            return _source.AppendLine("#line default").Append(' ', Indent);
        }

        private void CodeHidden()
        {
            _source.AppendLine("#line hidden");
        }

        private void CodeDefault()
        {
            _source.AppendLine("#line default");
        }

        protected override void Visit(SendLiteralChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Text))
                return;

            CodeHidden();
            AppendIndent().AppendLine("Output.Write(\"" + chunk.Text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\");");
            CodeDefault();
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            CodeIndent(chunk).Append("Output.Write(").Append(chunk.Code).AppendLine(");");
            CodeDefault();
        }

        protected override void Visit(MacroChunk chunk)
        {

        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            CodeIndent(chunk).AppendLine(chunk.Code.Replace("\r", "").Replace("\n", "\r\n"));
            CodeDefault();
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            CodeIndent(chunk).Append(chunk.Type).Append(' ').Append(chunk.Name);
            if (!string.IsNullOrEmpty(chunk.Value))
            {
                _source.Append(" = ").Append(chunk.Value);
            }
            _source.AppendLine(";");
            CodeDefault();
        }

        protected override void Visit(ForEachChunk chunk)
        {
            var terms = chunk.Code.Split(' ', '\r', '\n', '\t').ToList();
            var inIndex = terms.IndexOf("in");
            string variableName = (inIndex < 2 ? null : terms[inIndex - 1]);

            if (variableName == null)
            {
                CodeIndent(chunk).AppendLine(string.Format("foreach({0})", chunk.Code));
                CodeDefault();
                AppendIndent().AppendLine("{");
                Indent += 4;
                Accept(chunk.Body);
                Indent -= 4;
                AppendIndent().AppendLine(string.Format("}} //foreach {0}", chunk.Code.Replace("\r", "").Replace("\n", " ")));
            }
            else
            {
                var detect = new DetectCodeExpressionVisitor(this.OuterPartial);
                var autoIndex = detect.Add(variableName + "Index");
                var autoCount = detect.Add(variableName + "Count");
                var autoIsFirst = detect.Add(variableName + "IsFirst");
                var autoIsLast = detect.Add(variableName + "IsLast");
                detect.Accept(chunk.Body);

                if (autoIsLast.Detected)
                {
                    autoIndex.Detected = true;
                    autoCount.Detected = true;
                }

                AppendIndent().AppendLine("{");
                if (autoIndex.Detected)
                    _source.Append(' ', Indent + 4).AppendFormat("int {0}Index = 0;\r\n", variableName);
                if (autoIsFirst.Detected)
                    _source.Append(' ', Indent + 4).AppendFormat("bool {0}IsFirst = true;\r\n", variableName);
                if (autoCount.Detected)
                {
                    string collectionCode = string.Join(" ", terms.ToArray(), inIndex + 1, terms.Count - inIndex - 1);
                    _source.Append(' ', Indent + 4).AppendFormat("int {0}Count = global::Spark.Compiler.CollectionUtility.Count({1});\r\n", variableName, collectionCode);
                }

                Indent += 4;
                CodeIndent(chunk).AppendFormat("foreach({0})\r\n", chunk.Code);
                CodeDefault();
                _source.Append(' ', Indent).AppendLine("{");

                CodeHidden();
                if (autoIsLast.Detected)
                    _source.Append(' ', Indent + 4).AppendFormat("bool {0}IsLast = ({0}Index == {0}Count - 1);\r\n", variableName);
                CodeDefault();

                Indent += 8;
                Accept(chunk.Body);
                Indent -= 8;

                CodeHidden();
                if (autoIndex.Detected)
                    _source.Append(' ', Indent + 4).AppendFormat("++{0}Index;\r\n", variableName);
                if (autoIsFirst.Detected)
                    _source.Append(' ', Indent + 4).AppendFormat("{0}IsFirst = false;\r\n", variableName);
                CodeDefault();

                _source.Append(' ').AppendLine("}");
                AppendIndent().AppendFormat("}} //foreach {0}\r\n", chunk.Code.Replace("\r", "").Replace("\n", " "));
            }
        }

        protected override void Visit(ScopeChunk chunk)
        {
            CodeIndent(chunk).AppendLine("{");
            CodeDefault();
            Indent += 4;
            Accept(chunk.Body);
            Indent -= 4;
            AppendIndent().AppendLine("}");
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            CodeIndent(chunk).AppendLine(string.Format("{0} = {1};", chunk.Name, chunk.Value));
            CodeDefault();
        }


        protected override void Visit(ContentChunk chunk)
        {
            CodeIndent(chunk).AppendLine(string.Format("using(OutputScope(\"{0}\"))", chunk.Name));
            AppendIndent().AppendLine("{");
            Indent += 4;
            Accept(chunk.Body);
            Indent -= 4;
            AppendIndent().AppendLine("}");
        }

        protected override void Visit(ContentSetChunk chunk)
        {
            CodeIndent(chunk).AppendLine("using(OutputScope(new System.IO.StringWriter()))");
            CodeDefault();

            AppendIndent().AppendLine("{");
            Indent += 4;
            Accept(chunk.Body);

            CodeHidden();
            string format;
            switch (chunk.AddType)
            {
                case ContentAddType.AppendAfter:
                    format = "{0} = {0} + Output.ToString();";
                    break;
                case ContentAddType.InsertBefore:
                    format = "{0} = Output.ToString() + {0};";
                    break;
                default:
                    format = "{0} = Output.ToString();";
                    break;
            }
            AppendIndent().AppendFormat(format, chunk.Variable).AppendLine();

            Indent -= 4;
            AppendIndent().AppendLine("}");

            CodeDefault();
        }

        protected override void Visit(UseContentChunk chunk)
        {
            CodeIndent(chunk).AppendLine(string.Format("if (Content.ContainsKey(\"{0}\"))", chunk.Name));
            CodeHidden();
            AppendIndent().AppendLine("{");
            _source.Append(' ', Indent + 4).AppendLine(string.Format("Output.Write(Content[\"{0}\"]);", chunk.Name));
            AppendIndent().AppendLine("}");
            if (chunk.Default.Count != 0)
            {
                AppendIndent().AppendLine("else");
                AppendIndent().AppendLine("{");
                Indent += 4;
                Accept(chunk.Default);
                Indent -= 4;
                AppendIndent().AppendLine("}");
            }
            CodeDefault();
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
            if (string.IsNullOrEmpty(chunk.Name))
            {
                Accept(OuterPartial.Body);
            }
            else if (OuterPartial.Sections.ContainsKey(chunk.Name))
            {
                Accept(OuterPartial.Sections[chunk.Name]);
            }
            else
            {
                Accept(chunk.Default);
            }
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
                        CodeIndent(chunk).AppendLine(string.Format("if ({0})", chunk.Condition));
                        CodeDefault();
                        AppendIndent().AppendLine("{");
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        AppendIndent().AppendLine(string.Format("}} // if ({0})",
                                                         chunk.Condition.Replace("\r", "").Replace("\n", " ")));
                    }
                    break;
                case ConditionalType.ElseIf:
                    {
                        CodeIndent(chunk).AppendLine(string.Format("else if ({0})", chunk.Condition));
                        CodeDefault();
                        AppendIndent().AppendLine("{");
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        AppendIndent().AppendLine(string.Format("}} // else if ({0})",
                                                         chunk.Condition.Replace("\r", "").Replace("\n", " ")));
                    }
                    break;
                case ConditionalType.Else:
                    {
                        AppendIndent().AppendLine("else");
                        CodeIndent(chunk).AppendLine("{");
                        CodeDefault();
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        AppendIndent().AppendLine("}");
                    }
                    break;
            }
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {

        }

    }
}