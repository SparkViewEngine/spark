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
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Ruby.Compiler.ChunkVisitors
{
    public class GeneratedCodeVisitor : ChunkVisitor
    {
        private readonly SourceWriter _source;
        private readonly VariableTracker _variables;

        public GeneratedCodeVisitor(SourceWriter source, IDictionary<string, object> globals)
        {
            _source = source;
            _variables = new VariableTracker(globals);
        }

        protected override void Visit(MacroChunk chunk)
        {
            //no-op
        }

        protected override void Visit(SendLiteralChunk chunk)
        {
            _source.Write("output_write_adapter \"").Write(EscapeStringContents(chunk.Text)).WriteLine("\"");
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            //_source.Write("output_write_adapter ").WriteLine(chunk.Code);
            //TODO: handle exception

            _source.WriteLine("begin");
            _source.Indent++;
            _source.Write("output_write_adapter(").Write(chunk.Code).WriteLine(")");
            _source.Indent--;
            _source.WriteLine("rescue");
            if (!chunk.SilentNulls)
            {
                _source.Indent++;
                _source.Write("output_write_adapter \"${").Write(EscapeStringContents(chunk.Code)).WriteLine(" => \"+$!+\"}\"");
                _source.Indent--;
            }
            _source.WriteLine("end");
        }

        static string EscapeStringContents(string text)
        {
            return text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            _variables.Declare(chunk.Name);

            var value = chunk.Value;
            if (Snippets.IsNullOrEmpty(value))
                value = "nil";
            _source.Write(chunk.Name).Write("=").WriteLine(value);
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            if (!_variables.IsDeclared(chunk.Name))
                _variables.Declare(chunk.Name);

            var value = chunk.Value;
            if (Snippets.IsNullOrEmpty(value))
                value = "nil";
            _source.Write(chunk.Name).Write("=").WriteLine(value);
        }

        protected override void Visit(DefaultVariableChunk chunk)
        {
            if (_variables.IsDeclared(chunk.Name))
                return;

            _variables.Declare(chunk.Name);
            var value = chunk.Value;
            if (Snippets.IsNullOrEmpty(value))
                value = "nil";
            _source.Write(chunk.Name).Write("=").WriteLine(value);
        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            _source.WriteLine(chunk.Code);
        }

        protected override void Visit(ForEachChunk chunk)
        {
            var forEach = new ForEachInspector(chunk.Code);

            if (!forEach.Recognized)
            {
                _source.Write("for ").WriteLine(chunk.Code);
                _source.Indent++;
                _variables.PushScope();
                Accept(chunk.Body);
                _source.WriteLine("pass");
                _variables.PopScope();
                _source.Indent--;
                return;
            }

            _variables.PushScope();

            var detect = new DetectCodeExpressionVisitor(OuterPartial);
            var autoIndex = detect.Add(forEach.VariableName + "Index");
            var autoCount = detect.Add(forEach.VariableName + "Count");
            var autoIsFirst = detect.Add(forEach.VariableName + "IsFirst");
            var autoIsLast = detect.Add(forEach.VariableName + "IsLast");
            detect.Accept(chunk.Body);

            if (autoIsLast.Detected)
            {
                autoIndex.Detected = true;
                autoCount.Detected = true;
            }

            if (autoIndex.Detected)
            {
                _variables.Declare(forEach.VariableName + "Index");
                _source.Write(forEach.VariableName).WriteLine("Index=0");
            }
            if (autoIsFirst.Detected)
            {
                _variables.Declare(forEach.VariableName + "IsFirst");
                _source.Write(forEach.VariableName).WriteLine("IsFirst=true");
            }
            if (autoCount.Detected)
            {
                _variables.Declare(forEach.VariableName + "Count");
                _source.Write(forEach.VariableName).WriteLine("Count=0");
                _source.Write(forEach.CollectionCode).Write(".each {")
                    .Write(forEach.VariableName).Write("Count=")
                    .Write(forEach.VariableName).Write("Count+1")
                    .WriteLine("}");
            }

            _variables.Declare(forEach.VariableName);
            _source.Write("for ").WriteLine(chunk.Code);
            _source.Indent++;
            if (autoIsLast.Detected)
            {
                _variables.Declare(forEach.VariableName + "IsLast");
                _source
                    .Write(forEach.VariableName).Write("IsLast=(")
                    .Write(forEach.VariableName).Write("Index==")
                    .Write(forEach.VariableName).WriteLine("Count - 1)");
            }
            Accept(chunk.Body);
            if (autoIndex.Detected)
            {
                _source
                    .Write(forEach.VariableName).Write("Index=")
                    .Write(forEach.VariableName).WriteLine("Index+1");
            }
            if (autoIsFirst.Detected)
            {
                _source.Write(forEach.VariableName).WriteLine("IsFirst=false");
            }
            _source.Indent--;
            _source.WriteLine("end");

            _variables.PopScope();
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            switch (chunk.Type)
            {
                case ConditionalType.If:
                    _source.Write("if ").WriteLine(chunk.Condition);
                    break;
                case ConditionalType.ElseIf:
                    _source.ClearEscrowLine();
                    _source.Write("elsif ").WriteLine(chunk.Condition);
                    break;
                case ConditionalType.Else:
                    _source.ClearEscrowLine();
                    _source.Write("else ").WriteLine(chunk.Condition);
                    break;
                case ConditionalType.Once:
                    _source.Write("if once(").Write(chunk.Condition).WriteLine(")");
                    break;
                default:
                    throw new CompilerException(string.Format("Unknown ConditionalChunk type {0}", chunk.Type));
            }
            _source.Indent++;
            _variables.PushScope();
            Accept(chunk.Body);
            _variables.PopScope();
            _source.Indent--;
            _source.EscrowLine("end");
        }

        protected override void Visit(UseContentChunk chunk)
        {
            _source.Write("if content.contains_key(\"").Write(chunk.Name).WriteLine("\")");
            _source.Indent++;
            _source.Write("output_write_adapter content.get_Item(\"").Write(chunk.Name).WriteLine("\")");
            _source.Indent--;
            if (chunk.Default.Count != 0)
            {
                _source.WriteLine("else");
                _source.Indent++;
                Accept(chunk.Default);
                _source.Indent--;
            }
            _source.WriteLine("end");
        }

        private int _usingDepth;
        protected override void Visit(ContentChunk chunk)
        {
            _source.Write("__using").Write(++_usingDepth).Write("=output_scope_adapter(\"").Write(chunk.Name).WriteLine("\")");
            Accept(chunk.Body);
            _source.Write("__using").Write(_usingDepth--).WriteLine(".dispose");
        }

        protected override void Visit(ContentSetChunk chunk)
        {
            _source.Write("__using").Write(++_usingDepth).WriteLine("=output_scope");
            Accept(chunk.Body);
            switch (chunk.AddType)
            {
                case ContentAddType.AppendAfter:
                    //"{0} = {0} + Output.ToString();";
                    _source.Write(chunk.Variable).Write("=").Write(chunk.Variable).WriteLine("+output.to_string.to_s");
                    break;
                case ContentAddType.InsertBefore:
                    //"{0} = Output.ToString() + {0};";
                    _source.Write(chunk.Variable).Write("=output.to_string.to_s+").WriteLine(chunk.Variable);
                    break;
                default:
                    //"{0} = Output.ToString();";
                    _source.Write(chunk.Variable).WriteLine("=output.to_string.to_s");
                    break;
            }
            _source.Write("__using").Write(_usingDepth--).WriteLine(".dispose");
        }

        protected override void Visit(ScopeChunk chunk)
        {
            _variables.PushScope();
            Accept(chunk.Body);
            _variables.PopScope();
        }


        private RenderPartialChunk OuterPartial { get; set; }
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
    }
}