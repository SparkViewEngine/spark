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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.VisualBasic.ChunkVisitors
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
                        .AppendFormat("#ExternalSource(\"{1}\",  {0})", chunk.Position.Line, chunk.Position.SourceContext.FileName)
                        .AppendLine()
                        .Append(' ', chunk.Position.Column - 1);
                }

                return _source.AppendLine("#ExternalSource(\"\", 16707566)").Append(' ', Indent);
            }

            return _source.Append(' ', Indent);
        }

        private void CodeHidden()
        {
            if (_source.AdjustDebugSymbols)
                _source.AppendLine("#ExternalSource(\"\", 16707566)");
        }

        private void CodeDefault()
        {
            if (_source.AdjustDebugSymbols)
                _source.AppendLine("#End ExternalSource");
        }

        private void AppendOpenScope()
        {
            PushScope();
            AppendIndent().AppendLine("If True Then");
            Indent += 4;
        }
        private void AppendCloseScope()
        {
            Indent -= 4;
            AppendIndent().AppendLine("End If");
            PopScope();
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
            AppendIndent().Append("Output.Write(\"").Append(EscapeStringContents(chunk.Text)).AppendLine("\")");
            CodeDefault();
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            var automaticallyEncode = chunk.AutomaticallyEncode;
            if (chunk.Code.ToString().StartsWith("H("))
                automaticallyEncode = false;

            AppendIndent().AppendLine("Try");
            Indent += 4;
            CodeIndent(chunk)
                .Append("Output.Write(")
                .Append(automaticallyEncode ? "H(" : "")
                .AppendCode(chunk.Code)
                .Append(automaticallyEncode ? ")" : "")
                .AppendLine(")");
            Indent -= 4;
            CodeDefault();

            if (_nullBehaviour == NullBehaviour.Lenient)
            {
                AppendIndent().AppendLine("Catch ex As Global.System.NullReferenceException");
                if (!chunk.SilentNulls)
                {
                    Indent += 4;
                    AppendIndent().Append("Output.Write(\"${")
                        .Append(EscapeStringContents(chunk.Code))
                        .AppendLine("}\")");
                    Indent -= 4;
                }
                AppendIndent().AppendLine("End Try");
            }
            else
            {
                AppendIndent().AppendLine("Catch ex As Global.System.NullReferenceException");
                Indent += 4;
                AppendIndent().Append("Throw New Global.System.ArgumentNullException(\"${")
                    .Append(EscapeStringContents(chunk.Code))
                    .AppendLine("}\", ex)");
                Indent -= 4;
                AppendIndent().AppendLine("End Try");
            }
        }

        static string EscapeStringContents(string text)
        {
            return text
                .Replace("\"", "[\"]")
                .Replace("\t", "\" & vbTab & \"")
                .Replace("\r\n", "\" & vbCrLf & \"")
                .Replace("\r", "\" & vbCr & \"")
                .Replace("\n", "\" & vbLf & \"")
                .Replace(" & \"\"", "")
                .Replace("[\"]", "\"\"");
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

            if (Snippets.IsNullOrEmpty(chunk.Type) || String.Equals(chunk.Type, "var"))
            {
                CodeIndent(chunk)
                    .Append("Dim ")
                    .Append(chunk.Name);
            }
            else
            {
                CodeIndent(chunk)
                    .Append("Dim ")
                    .Append(chunk.Name)
                    .Append(" As ")
                    .Append(chunk.Type);
            }
            if (!Snippets.IsNullOrEmpty(chunk.Value))
            {
                _source.Append(" = ").Append(chunk.Value);
            }
            _source.AppendLine();
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

        static bool IsInOrAs(string part)
        {
            return string.Equals(part, "In", StringComparison.InvariantCultureIgnoreCase) ||
                   string.Equals(part, "As", StringComparison.InvariantCultureIgnoreCase);
        }
        static bool IsIn(string part)
        {
            return string.Equals(part, "In", StringComparison.InvariantCultureIgnoreCase);
        }

        protected override void Visit(ForEachChunk chunk)
        {
            var terms = chunk.Code.ToString().Split(' ', '\r', '\n', '\t').ToList();
            var inIndex = terms.FindIndex(IsIn);
            var inOrAsIndex = terms.FindIndex(IsInOrAs);
            var variableName = (inOrAsIndex < 1 ? null : terms[inOrAsIndex - 1]);

            if (variableName == null)
            {
                CodeIndent(chunk)
                    .Append("For Each ")
                    .AppendLine(chunk.Code);
                CodeDefault();
                PushScope();
                Indent += 4;
                Accept(chunk.Body);
                Indent -= 4;
                AppendIndent().AppendLine("Next");
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

                AppendOpenScope();

                if (autoIndex.Detected)
                {
                    DeclareVariable(variableName + "Index");
                    _source.Append(' ', Indent + 4).AppendFormat("Dim {0}Index As Integer = 0\r\n", variableName);
                }
                if (autoIsFirst.Detected)
                {
                    DeclareVariable(variableName + "IsFirst");
                    _source.Append(' ', Indent + 4).AppendFormat("Dim {0}IsFirst As Boolean = True\r\n", variableName);
                }
                if (autoCount.Detected)
                {
                    DeclareVariable(variableName + "Count");
                    var collectionCode = string.Join(" ", terms.ToArray(), inIndex + 1, terms.Count - inIndex - 1);
                    _source.Append(' ', Indent + 4).AppendFormat("Dim {0}Count As Integer = Global.Spark.Compiler.CollectionUtility.Count({1})\r\n", variableName, collectionCode);
                }

                CodeIndent(chunk)
                    .Append("For Each ")
                    .AppendLine(chunk.Code);
                CodeDefault();
                Indent += 4;
                PushScope();

                DeclareVariable(variableName);

                CodeHidden();
                if (autoIsLast.Detected)
                {
                    DeclareVariable(variableName + "IsLast");
                    _source.Append(' ', Indent + 4).AppendFormat("Dim {0}IsLast As Boolean = ({0}Index = {0}Count - 1)\r\n",
                                                                 variableName);
                }
                CodeDefault();

                Accept(chunk.Body);

                CodeHidden();
                if (autoIndex.Detected)
                    _source.Append(' ', Indent + 4).AppendFormat("{0}Index = {0}Index + 1\r\n", variableName);
                if (autoIsFirst.Detected)
                    _source.Append(' ', Indent + 4).AppendFormat("{0}IsFirst = False\r\n", variableName);
                CodeDefault();

                PopScope();
                Indent -= 4;
                _source.Append(' ', Indent).AppendLine("Next");

                AppendCloseScope();
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
