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
        private readonly SourceWriter _source;
        private readonly NullBehaviour _nullBehaviour;

        public GeneratedCodeVisitor(SourceWriter source, Dictionary<string, object> globalSymbols, NullBehaviour nullBehaviour)
        {
            _nullBehaviour = nullBehaviour;
            _source = source;

            _scope = new Scope(new Scope(null) { Variables = globalSymbols });
        }

        
        private SourceWriter CodeIndent(Chunk chunk)
        {
            if (_source.AdjustDebugSymbols)
            {
                if (chunk != null && chunk.Position != null)
                {
                    _source.StartOfLine = false;
                    return _source
                        .WriteDirective("#ExternalSource(\"{1}\",  {0})", chunk.Position.Line, chunk.Position.SourceContext.FileName)
                        .Indent(chunk.Position.Column - 1);
                }

                return _source.WriteDirective("#ExternalSource(\"\", 16707566)");
            }

            return _source;
        }

        private void CodeHidden()
        {
            if (_source.AdjustDebugSymbols)
                _source.WriteLine("#ExternalSource(\"\", 16707566)");
        }

        private void CodeDefault()
        {
            if (_source.AdjustDebugSymbols)
                _source.WriteLine("#End ExternalSource");
        }

        private void AppendOpenScope()
        {
            PushScope();
            _source.WriteLine("If True Then").AddIndent();
        }

        private void AppendCloseScope()
        {
            _source.RemoveIndent().WriteLine("End If");
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
            public Scope Prior { get; private set; }
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
            _source.Write("Output.Write(\"").Write(EscapeStringContents(chunk.Text)).WriteLine("\")");
            CodeDefault();
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            var automaticallyEncode = chunk.AutomaticallyEncode;
            if (chunk.Code.ToString().StartsWith("H("))
                automaticallyEncode = false;

            _source.WriteLine("Try").AddIndent();
            CodeIndent(chunk)
                .Write("Output.Write(")
                .Write(automaticallyEncode ? "H(" : "")
                .WriteCode(chunk.Code)
                .Write(automaticallyEncode ? ")" : "")
                .WriteLine(")")
                .RemoveIndent();
            CodeDefault();

            if (_nullBehaviour == NullBehaviour.Lenient)
            {
                _source.WriteLine("Catch ex As Global.System.NullReferenceException");
                if (!chunk.SilentNulls)
                {
                    _source
                        .AddIndent()
                        .Write("Output.Write(\"${")
                        .Write(EscapeStringContents(chunk.Code))
                        .WriteLine("}\")")
                        .RemoveIndent();
                }
                _source.WriteLine("End Try");
            }
            else
            {
                _source.WriteLine("Catch ex As Global.System.NullReferenceException");
                _source
                    .AddIndent()
                    .Write("Throw New Global.System.ArgumentNullException(\"${")
                    .Write(EscapeStringContents(chunk.Code))
                    .WriteLine("}\", ex)")
                    .RemoveIndent();
                _source.WriteLine("End Try");
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
            CodeIndent(chunk).WriteCode(chunk.Code).WriteLine();
            CodeDefault();
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            LocalVariableImpl(chunk, chunk.Name, chunk.Type, chunk.Value);
        }

        protected override void Visit(DefaultVariableChunk chunk)
        {
            if (IsVariableDeclared(chunk.Name))
                return;

            LocalVariableImpl(chunk, chunk.Name, chunk.Type, chunk.Value);
        }

        private void LocalVariableImpl(Chunk chunk, Snippets name, Snippets type, Snippets value)
        {
            DeclareVariable(name);

            if (Snippets.IsNullOrEmpty(type) || String.Equals(type, "var"))
            {
                CodeIndent(chunk)
                    .Write("Dim ")
                    .WriteCode(name);
            }
            else
            {
                CodeIndent(chunk)
                    .Write("Dim ")
                    .WriteCode(name)
                    .Write(" As ")
                    .WriteCode(type);
            }
            if (!Snippets.IsNullOrEmpty(value))
            {
                _source.Write(" = ").WriteCode(value);
            }
            _source.WriteLine();
            CodeDefault();
        }

        protected override void Visit(UseMasterChunk chunk)
        {
            //no-op
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
                    .Write("For Each ")
                    .WriteLine(chunk.Code)
                    .AddIndent();
                CodeDefault();
                PushScope();
                Accept(chunk.Body);
                _source
                    .RemoveIndent()
                    .WriteLine("Next");
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
                    _source.WriteLine("Dim {0}Index As Integer = 0", variableName);
                }
                if (autoIsFirst.Detected)
                {
                    DeclareVariable(variableName + "IsFirst");
                    _source.WriteLine("Dim {0}IsFirst As Boolean = True", variableName);
                }
                if (autoCount.Detected)
                {
                    DeclareVariable(variableName + "Count");
                    var collectionCode = string.Join(" ", terms.ToArray(), inIndex + 1, terms.Count - inIndex - 1);
                    _source.WriteLine("Dim {0}Count As Integer = Global.Spark.Compiler.CollectionUtility.Count({1})", variableName, collectionCode);
                }

                CodeIndent(chunk)
                    .Write("For Each ")
                    .WriteLine(chunk.Code)
                    .AddIndent();
                CodeDefault();
                
                PushScope();

                DeclareVariable(variableName);

                CodeHidden();
                if (autoIsLast.Detected)
                {
                    DeclareVariable(variableName + "IsLast");
                    _source.WriteLine("Dim {0}IsLast As Boolean = ({0}Index = {0}Count - 1)", variableName);
                }
                CodeDefault();

                Accept(chunk.Body);

                CodeHidden();
                if (autoIndex.Detected)
                    _source.WriteLine("{0}Index = {0}Index + 1", variableName);
                if (autoIsFirst.Detected)
                    _source.WriteLine("{0}IsFirst = False", variableName);
                CodeDefault();

                PopScope();
                _source.RemoveIndent().WriteLine("Next");

                AppendCloseScope();
            }
        }

        protected override void Visit(ScopeChunk chunk)
        {
            AppendOpenScope();
            Accept(chunk.Body);
            AppendCloseScope();
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            CodeIndent(chunk)
                .Write(chunk.Name)
                .Write(" = ")
                .WriteLine(chunk.Value);
            CodeDefault();
        }


        protected override void Visit(ContentChunk chunk)
        {
            CodeIndent(chunk)
                .Write("Using OutputScope(\"")
                .Write(chunk.Name)
                .WriteLine("\")")
                .AddIndent();
            PushScope();
            Accept(chunk.Body);
            PopScope();
            _source
                .RemoveIndent()
                .WriteLine("End Using");
        }

        protected override void Visit(UseImportChunk chunk)
        {

        }

        protected override void Visit(ContentSetChunk chunk)
        {
            CodeIndent(chunk)
                .WriteLine("Using OutputScope(new System.IO.StringWriter())")
                .AddIndent();
            CodeDefault();

            PushScope();
            Accept(chunk.Body);

            CodeHidden();
            string format;
            switch (chunk.AddType)
            {
                case ContentAddType.AppendAfter:
                    format = "{0} = {0} + Output.ToString()";
                    break;
                case ContentAddType.InsertBefore:
                    format = "{0} = Output.ToString() + {0}";
                    break;
                default:
                    format = "{0} = Output.ToString()";
                    break;
            }
            _source.WriteFormat(format, chunk.Variable).WriteLine();

            PopScope();
            _source.RemoveIndent().WriteLine("End Using");

            CodeDefault();
        }

        protected override void Visit(UseContentChunk chunk)
        {
            CodeIndent(chunk)
                .Write("If Content.ContainsKey(\"")
                .Write(chunk.Name)
                .WriteLine("\") Then").AddIndent()
                .Write("Global.Spark.Spool.TextWriterExtensions.WriteTo(Content(\"")
                .Write(chunk.Name)
                .WriteLine("\"), Output)").RemoveIndent();

            if (chunk.Default.Any())
            {
                _source
                    .WriteLine("Else").AddIndent();
                PushScope();
                Accept(chunk.Default);
                PopScope();
                _source.RemoveIndent();
            }

            _source.WriteLine("End If");
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
            chunk.Extension.VisitChunk(this, OutputLocation.RenderMethod, chunk.Body, _source.GetStringBuilder());
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            switch (chunk.Type)
            {
                case ConditionalType.If:
                    CodeIndent(chunk).Write("If ").WriteCode(chunk.Condition).WriteLine(" Then");
                    break;
                case ConditionalType.ElseIf:
                    _source.ClearEscrowLine();
                    CodeIndent(chunk).Write("ElseIf ").WriteCode(chunk.Condition).WriteLine(" Then");
                    break;
                case ConditionalType.Else:
                    _source.ClearEscrowLine();
                    _source.WriteLine("Else");
                    break;
                case ConditionalType.Once:
                    _source.Write("If Once(").WriteCode(chunk.Condition).WriteLine(") Then");
                    break;
                case ConditionalType.Unless:
                    CodeIndent(chunk).Write("If Not ").WriteCode(chunk.Condition).WriteLine(" Then");
                    break;
                default:
                    throw new CompilerException(string.Format("Unknown ConditionalChunk type {0}", chunk.Type));
            }

            _source
                .AddIndent();
            PushScope();
            Accept(chunk.Body);
            PopScope();
            _source
                .RemoveIndent()
                .EscrowLine("End If");

        }

        protected override void Visit(ViewDataModelChunk chunk)
        {

        }

        protected override void Visit(PageBaseTypeChunk chunk)
        {
        }
    }

}
