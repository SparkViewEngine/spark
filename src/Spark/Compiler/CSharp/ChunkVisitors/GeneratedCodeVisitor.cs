// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.CSharp.ChunkVisitors
{
    public class GeneratedCodeVisitor : GeneratedCodeVisitorBase
    {
        private readonly SourceBuilder _source;
        private readonly NullBehaviour _nullBehaviour;

        public GeneratedCodeVisitor(SourceBuilder output, Dictionary<string, object> globalSymbols, NullBehaviour nullBehaviour)
        {
            _nullBehaviour = nullBehaviour;
            _source = output;

            _scope = new Scope(new Scope(null) { Variables = globalSymbols });
        }

        public int Indent { get; set; }

        private SourceBuilder AppendIndent()
        {
            return _source.Append(' ', Indent);
        }

        private SourceBuilder CodeIndent(Chunk chunk)
        {
            if (_source.AdjustDebugSymbols)
            {
                if (chunk != null && chunk.Position != null)
                {
                    return _source
                        .AppendFormat("#line {0} \"{1}\"", chunk.Position.Line, chunk.Position.SourceContext.FileName)
                        .AppendLine()
                        .Append(' ', chunk.Position.Column - 1);
                }

                return _source.AppendLine("#line default").Append(' ', Indent);
            }

            return _source.Append(' ', Indent);
        }

        private void CodeHidden()
        {
            if (_source.AdjustDebugSymbols)
                _source.AppendLine("#line hidden");
        }

        private void CodeDefault()
        {
            if (_source.AdjustDebugSymbols)
                _source.AppendLine("#line default");
        }

        private void AppendOpenBrace()
        {
            AppendIndent().AppendLine("{");
            Indent += 4;
        }
        private void AppendCloseBrace()
        {
            Indent -= 4;
            AppendIndent().AppendLine("}");
        }

        class Scope
        {
            public Scope(Scope prior)
            {
                Variables = new Dictionary<string, object>();
                Prior = prior;
            }
            public Dictionary<string, object> Variables { get; set; }
            public Scope Prior { get; set; }
        }

        private Scope _scope;

        void PushScope()
        {
            _scope = new Scope(_scope);
        }
        void PopScope()
        {
            _scope = _scope.Prior;
        }
        void DeclareVariable(string name)
        {
            _scope.Variables.Add(name, null);
        }
        bool IsVariableDeclared(string name)
        {
            var scan = _scope;
            while (scan != null)
            {
                if (scan.Variables.ContainsKey(name))
                    return true;
                scan = scan.Prior;
            }
            return false;
        }


        protected override void Visit(SendLiteralChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Text))
                return;

            CodeHidden();
            AppendIndent().Append("Output.Write(\"").Append(EscapeStringContents(chunk.Text)).AppendLine("\");");
            CodeDefault();
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            var automaticallyEncode = chunk.AutomaticallyEncode;
            if (chunk.Code.ToString().StartsWith("H("))
                automaticallyEncode = false;

            AppendIndent().AppendLine("try");
            AppendIndent().AppendLine("{");
            CodeIndent(chunk)
                .Append("Output.Write(")
                .Append(automaticallyEncode ? "H(" : "")
                .AppendCode(chunk.Code)
                .Append(automaticallyEncode ? ")" : "")
                .AppendLine(");");
            CodeDefault();
            AppendIndent().AppendLine("}");

            if (_nullBehaviour == NullBehaviour.Lenient)
            {
                AppendIndent().AppendLine("catch(System.NullReferenceException)");
                AppendOpenBrace();
                if (!chunk.SilentNulls)
                {
                    AppendIndent().Append("Output.Write(\"${")
                        .Append(EscapeStringContents(chunk.Code))
                        .AppendLine("}\");");
                }
                AppendCloseBrace();
            }
            else
            {
                AppendIndent().AppendLine("catch(System.NullReferenceException ex)");
                AppendOpenBrace();
                AppendIndent().Append("throw new System.ArgumentNullException(\"${")
                    .Append(EscapeStringContents(chunk.Code))
                    .AppendLine("}\", ex);");
                AppendCloseBrace();
            }
        }

        static string EscapeStringContents(string text)
        {
            return text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");
        }

        protected override void Visit(MacroChunk chunk)
        {

        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            CodeIndent(chunk).AppendCode(chunk.Code).AppendLine();
            CodeDefault();
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            DeclareVariable(chunk.Name);

            CodeIndent(chunk).Append(chunk.Type).Append(' ').Append(chunk.Name);
            if (!Snippets.IsNullOrEmpty(chunk.Value))
            {
                _source.Append(" = ").Append(chunk.Value);
            }
            _source.AppendLine(";");
            CodeDefault();
        }

        protected override void Visit(UseMasterChunk chunk)
        {
            //no-op
        }

        protected override void Visit(DefaultVariableChunk chunk)
        {
            if (IsVariableDeclared(chunk.Name))
                return;

            DeclareVariable(chunk.Name);
            CodeIndent(chunk).Append(chunk.Type).Append(' ').Append(chunk.Name);
            if (!Snippets.IsNullOrEmpty(chunk.Value))
            {
                _source.Append(" = ").Append(chunk.Value);
            }
            _source.AppendLine(";");
            CodeDefault();
        }

        protected override void Visit(ForEachChunk chunk)
        {
            var terms = chunk.Code.ToString().Split(' ', '\r', '\n', '\t').ToList();
            var inIndex = terms.IndexOf("in");
            var variableName = (inIndex < 2 ? null : terms[inIndex - 1]);

            if (variableName == null)
            {
                CodeIndent(chunk)
                    .Append("foreach(")
                    .Append(chunk.Code)
                    .AppendLine(")");
                CodeDefault();
                PushScope();
                AppendIndent().AppendLine("{");
                Indent += 4;
                Accept(chunk.Body);
                Indent -= 4;
                AppendIndent().AppendLine(string.Format("}} //foreach {0}", chunk.Code.ToString().Replace("\r", "").Replace("\n", " ")));
                PopScope();
            }
            else
            {
                var detect = new DetectCodeExpressionVisitor(OuterPartial);
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

                PushScope();
                AppendIndent().AppendLine("{");
                if (autoIndex.Detected)
                {
                    DeclareVariable(variableName + "Index");
                    _source.Append(' ', Indent + 4).AppendFormat("int {0}Index = 0;\r\n", variableName);
                }
                if (autoIsFirst.Detected)
                {
                    DeclareVariable(variableName + "IsFirst");
                    _source.Append(' ', Indent + 4).AppendFormat("bool {0}IsFirst = true;\r\n", variableName);
                }
                if (autoCount.Detected)
                {
                    DeclareVariable(variableName + "Count");
                    string collectionCode = string.Join(" ", terms.ToArray(), inIndex + 1, terms.Count - inIndex - 1);
                    _source.Append(' ', Indent + 4).AppendFormat("int {0}Count = global::Spark.Compiler.CollectionUtility.Count({1});\r\n", variableName, collectionCode);
                }

                Indent += 4;
                CodeIndent(chunk)
                    .Append("foreach(")
                    .Append(chunk.Code)
                    .AppendLine(")");
                CodeDefault();

                PushScope();
                DeclareVariable(variableName);
                _source.Append(' ', Indent).AppendLine("{");

                CodeHidden();
                if (autoIsLast.Detected)
                {
                    DeclareVariable(variableName + "IsLast");
                    _source.Append(' ', Indent + 4).AppendFormat("bool {0}IsLast = ({0}Index == {0}Count - 1);\r\n",
                                                                 variableName);
                }
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
                PopScope();

                AppendIndent().AppendFormat("}} //foreach {0}\r\n", chunk.Code.ToString().Replace("\r", "").Replace("\n", " "));
                PopScope();
            }
        }

        protected override void Visit(ScopeChunk chunk)
        {
            PushScope();
            CodeIndent(chunk).AppendLine("{");
            CodeDefault();
            Indent += 4;
            Accept(chunk.Body);
            Indent -= 4;
            AppendIndent().AppendLine("}");
            PopScope();
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            CodeIndent(chunk)
                .Append(chunk.Name)
                .Append(" = ")
                .Append(chunk.Value)
                .AppendLine(";");
            CodeDefault();
        }


        protected override void Visit(ContentChunk chunk)
        {
            CodeIndent(chunk).AppendLine(string.Format("using(OutputScope(\"{0}\"))", chunk.Name));
            PushScope();
            AppendIndent().AppendLine("{");
            Indent += 4;
            Accept(chunk.Body);
            Indent -= 4;
            AppendIndent().AppendLine("}");
            PopScope();
        }

        protected override void Visit(UseImportChunk chunk)
        {

        }

        protected override void Visit(ContentSetChunk chunk)
        {
            CodeIndent(chunk).AppendLine("using(OutputScope(new System.IO.StringWriter()))");
            CodeDefault();

            PushScope();
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
            PopScope();

            CodeDefault();
        }

        protected override void Visit(UseContentChunk chunk)
        {
            CodeIndent(chunk).AppendLine(string.Format("if (Content.ContainsKey(\"{0}\"))", chunk.Name));
            CodeHidden();
            PushScope();
            AppendIndent().AppendLine("{");
            _source.Append(' ', Indent + 4).AppendLine(string.Format("global::Spark.Spool.TextWriterExtensions.WriteTo(Content[\"{0}\"], Output);", chunk.Name));
            AppendIndent().AppendLine("}");
            PopScope();

            if (chunk.Default.Count != 0)
            {
                AppendIndent().AppendLine("else");
                PushScope();
                AppendIndent().AppendLine("{");
                Indent += 4;
                Accept(chunk.Default);
                Indent -= 4;
                AppendIndent().AppendLine("}");
                PopScope();
            }
            CodeDefault();
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
            chunk.Extension.VisitChunk(this, OutputLocation.RenderMethod, chunk.Body, _source.Source);
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            switch (chunk.Type)
            {
                case ConditionalType.If:
                    {
                        //CodeIndent(chunk).AppendLine(string.Format("if ({0})", chunk.Condition));
                        CodeIndent(chunk)
                            .Append("if (")
                            .AppendCode(chunk.Condition)
                            .AppendLine(")");
                        CodeDefault();
                        PushScope();
                        AppendIndent().AppendLine("{");
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        AppendIndent().AppendLine(string.Format("}} // if ({0})",
                                                                chunk.Condition.ToString().Replace("\r", "").Replace("\n", " ")));
                        PopScope();
                    }
                    break;
                case ConditionalType.ElseIf:
                    {
                        //CodeIndent(chunk).AppendLine(string.Format("else if ({0})", chunk.Condition));
                        CodeIndent(chunk)
                            .Append("else if (")
                            .AppendCode(chunk.Condition)
                            .AppendLine(")");
                        CodeDefault();
                        PushScope();
                        AppendIndent().AppendLine("{");
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        AppendIndent().AppendLine(string.Format("}} // else if ({0})",
                                                                chunk.Condition.ToString().Replace("\r", "").Replace("\n", " ")));
                        PopScope();
                    }
                    break;
                case ConditionalType.Else:
                    {
                        AppendIndent().AppendLine("else");
                        PushScope();
                        CodeIndent(chunk).AppendLine("{");
                        CodeDefault();
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        AppendIndent().AppendLine("}");
                        PopScope();
                    }
                    break;
                case ConditionalType.Once:
                    {
                        CodeIndent(chunk).Append("if (Once(").Append(chunk.Condition).AppendLine("))");
                        PushScope();
                        AppendIndent().AppendLine("{");
                        CodeDefault();
                        Indent += 4;
                        Accept(chunk.Body);
                        Indent -= 4;
                        AppendIndent().AppendLine("}");
                        PopScope();
                    }
                    break;
                default:
                    throw new CompilerException("Unexpected conditional type " + chunk.Type);
            }
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {

        }

        protected override void Visit(PageBaseTypeChunk chunk)
        {
        }
    }

}
