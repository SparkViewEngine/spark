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

namespace Spark.Compiler.CSharp.ChunkVisitors
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
                        .WriteDirective("#line {0} \"{1}\"", chunk.Position.Line, chunk.Position.SourceContext.FileName)
                        .Indent(chunk.Position.Column - 1);
                }

                _source.StartOfLine = false;
                return _source.WriteLine("#line default");
            }

            return _source;
        }

        private void CodeHidden()
        {
            if (_source.AdjustDebugSymbols)
                _source.WriteDirective("#line hidden");
        }

        private void CodeDefault()
        {
            if (_source.AdjustDebugSymbols)
                _source.WriteDirective("#line default");
        }

        private void AppendOpenBrace()
        {
            PushScope();
            _source.WriteLine("{").AddIndent();
        }

        private void AppendCloseBrace()
        {
            _source.RemoveIndent().WriteLine("}");
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
            _source.Write("Output.Write(\"").Write(EscapeStringContents(chunk.Text)).WriteLine("\");");
            CodeDefault();
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            var automaticallyEncode = chunk.AutomaticallyEncode;
            if (chunk.Code.ToString().StartsWith("H("))
                automaticallyEncode = false;

            _source
                .WriteLine("try")
                .WriteLine("{");
            CodeIndent(chunk)
                .Write("Output.Write(")
                .Write(automaticallyEncode ? "H(" : "")
                .WriteCode(chunk.Code)
                .Write(automaticallyEncode ? ")" : "")
                .WriteLine(");");
            CodeDefault();
            _source
                .WriteLine("}");

            if (_nullBehaviour == NullBehaviour.Lenient)
            {
                _source.WriteLine("catch(System.NullReferenceException)");
                AppendOpenBrace();
                if (!chunk.SilentNulls)
                {
                    _source.Write("Output.Write(\"${")
                        .Write(EscapeStringContents(chunk.Code))
                        .WriteLine("}\");");
                }
                AppendCloseBrace();
            }
            else
            {
                _source.WriteLine("catch(System.NullReferenceException ex)");
                AppendOpenBrace();
                _source.Write("throw new System.ArgumentNullException(\"${")
                    .Write(EscapeStringContents(chunk.Code))
                    .WriteLine("}\", ex);");
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
            CodeIndent(chunk).WriteCode(chunk.Code).WriteLine();
            CodeDefault();
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            DeclareVariable(chunk.Name);

            CodeIndent(chunk).WriteCode(chunk.Type).Write(" ").WriteCode(chunk.Name);
            if (!Snippets.IsNullOrEmpty(chunk.Value))
            {
                _source.Write(" = ").WriteCode(chunk.Value);
            }
            _source.WriteLine(";");
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
            CodeIndent(chunk).WriteCode(chunk.Type).Write(" ").Write(chunk.Name);
            if (!Snippets.IsNullOrEmpty(chunk.Value))
            {
                _source.Write(" = ").WriteCode(chunk.Value);
            }
            _source.WriteLine(";");
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
                    .Write("foreach(")
                    .WriteCode(chunk.Code)
                    .WriteLine(")");
                CodeDefault();
                AppendOpenBrace();
                Accept(chunk.Body);
                AppendCloseBrace();
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

                AppendOpenBrace();
                if (autoIndex.Detected)
                {
                    DeclareVariable(variableName + "Index");
                    _source.WriteLine("int {0}Index = 0;", variableName);
                }
                if (autoIsFirst.Detected)
                {
                    DeclareVariable(variableName + "IsFirst");
                    _source.WriteLine("bool {0}IsFirst = true;", variableName);
                }
                if (autoCount.Detected)
                {
                    DeclareVariable(variableName + "Count");
                    var collectionCode = string.Join(" ", terms.ToArray(), inIndex + 1, terms.Count - inIndex - 1);
                    _source.WriteLine("int {0}Count = global::Spark.Compiler.CollectionUtility.Count({1});", variableName, collectionCode);
                }


                CodeIndent(chunk)
                    .Write("foreach(")
                    .WriteCode(chunk.Code)
                    .WriteLine(")");
                CodeDefault();

                AppendOpenBrace();
                DeclareVariable(variableName);

                CodeHidden();
                if (autoIsLast.Detected)
                {
                    DeclareVariable(variableName + "IsLast");
                    _source.WriteLine("bool {0}IsLast = ({0}Index == {0}Count - 1);", variableName);
                }
                CodeDefault();

                Accept(chunk.Body);

                CodeHidden();
                if (autoIndex.Detected)
                    _source.WriteLine("++{0}Index;", variableName);
                if (autoIsFirst.Detected)
                    _source.WriteLine("{0}IsFirst = false;", variableName);
                CodeDefault();

                AppendCloseBrace();

                AppendCloseBrace();
            }
        }

        protected override void Visit(ScopeChunk chunk)
        {
            AppendOpenBrace();
            CodeDefault();
            Accept(chunk.Body);
            AppendCloseBrace();
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            CodeIndent(chunk)
                .Write(chunk.Name)
                .Write(" = ")
                .WriteCode(chunk.Value)
                .WriteLine(";");
            CodeDefault();
        }


        protected override void Visit(ContentChunk chunk)
        {
            CodeIndent(chunk).WriteLine("using(OutputScope(\"{0}\"))", chunk.Name);
            CodeDefault();
            AppendOpenBrace();
            Accept(chunk.Body);
            AppendCloseBrace();
        }

        protected override void Visit(UseImportChunk chunk)
        {

        }

        protected override void Visit(ContentSetChunk chunk)
        {
            CodeIndent(chunk).WriteLine("using(OutputScope(new System.IO.StringWriter()))");
            CodeDefault();

            AppendOpenBrace();

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
            _source.WriteLine(format, chunk.Variable);

            AppendCloseBrace();

            CodeDefault();
        }

        protected override void Visit(UseContentChunk chunk)
        {
            CodeIndent(chunk).WriteLine(string.Format("if (Content.ContainsKey(\"{0}\"))", chunk.Name));
            CodeHidden();
            AppendOpenBrace();
            _source.WriteFormat("global::Spark.Spool.TextWriterExtensions.WriteTo(Content[\"{0}\"], Output);", chunk.Name);
            AppendCloseBrace();

            if (chunk.Default.Count != 0)
            {
                _source.WriteLine("else");
                AppendOpenBrace();
                Accept(chunk.Default);
                AppendCloseBrace();
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
            chunk.Extension.VisitChunk(this, OutputLocation.RenderMethod, chunk.Body, _source.GetStringBuilder());
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            switch (chunk.Type)
            {
                case ConditionalType.If:
                    {
                        CodeIndent(chunk)
                            .Write("if (")
                            .WriteCode(chunk.Condition)
                            .WriteLine(")");
                    }
                    break;
                case ConditionalType.ElseIf:
                    {
                        CodeIndent(chunk)
                            .Write("else if (")
                            .WriteCode(chunk.Condition)
                            .WriteLine(")");
                    }
                    break;
                case ConditionalType.Else:
                    {
                        _source.WriteLine("else");
                    }
                    break;
                case ConditionalType.Once:
                    {
                        CodeIndent(chunk)
                            .Write("if (Once(")
                            .WriteCode(chunk.Condition)
                            .WriteLine("))");
                    }
                    break;
                default:
                    throw new CompilerException("Unexpected conditional type " + chunk.Type);
            }
            CodeDefault();
            AppendOpenBrace();
            Accept(chunk.Body);
            AppendCloseBrace();
        }

        protected override void Visit(CacheChunk chunk)
        {
            var siteGuid = Guid.NewGuid();

            CodeIndent(chunk)
                .Write("if (BeginCachedContent(\"")
                .Write(siteGuid.ToString("n"))
                .Write("\", new global::Spark.CacheExpires(")
                .WriteCode(chunk.Expires)
                .Write("), ")
                .WriteCode(chunk.Key)
                .WriteLine("))")
                .WriteLine("{").AddIndent();

            _source
                .WriteLine("try");
            
            AppendOpenBrace();
            Accept(chunk.Body);
            AppendCloseBrace();

            _source
                .WriteLine("finally")
                .WriteLine("{").AddIndent()
                .Write("EndCachedContent(")
                .WriteCode(chunk.Signal)
                .Write(");")
                .RemoveIndent().WriteLine("}")
                .RemoveIndent().WriteLine("}");
        }
        protected override void Visit(MarkdownChunk chunk)
        {
            CodeIndent(chunk).WriteLine("using(MarkdownOutputScope())");
            CodeDefault();
            AppendOpenBrace();
            Accept(chunk.Body);
            AppendCloseBrace();
        }

        
    }

}
