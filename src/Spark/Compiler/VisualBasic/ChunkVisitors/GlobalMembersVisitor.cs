// Copyright 2008-2024 Louis DeJardin
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.VisualBasic.ChunkVisitors
{
    public class GlobalMembersVisitor : ChunkVisitor
    {
        private readonly SourceWriter _source;
        private readonly Dictionary<string, object> _globalSymbols;
    	private readonly NullBehaviour _nullBehaviour;
    	readonly Dictionary<string, string> _viewDataAdded = new Dictionary<string, string>();
        readonly Dictionary<string, GlobalVariableChunk> _globalAdded = new Dictionary<string, GlobalVariableChunk>();
        
        public GlobalMembersVisitor(SourceWriter output, Dictionary<string, object> globalSymbols, NullBehaviour nullBehaviour)
        {
            _source = output;
            _globalSymbols = globalSymbols;
			_nullBehaviour = nullBehaviour;
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

        protected override void Visit(GlobalVariableChunk chunk)
        {
            if (!_globalSymbols.ContainsKey(chunk.Name))
                _globalSymbols.Add(chunk.Name, null);

            if (_globalAdded.ContainsKey(chunk.Name))
            {
                if (_globalAdded[chunk.Name].Type != chunk.Type ||
                    _globalAdded[chunk.Name].Value != chunk.Value)
                {
                    throw new CompilerException(
                        string.Format(
                            "The global named {0} cannot be declared repeatedly with different types or values",
                            chunk.Name));
                }
                return;
            }

            var type = chunk.Type ?? "Object";
            _source.WriteFormat(
                "\r\n    Private _{1} As {0} = {2}" +
                "\r\n    Public Property {1}() As {0}" +
                "\r\n        Get" +
                "\r\n            Return _{1}" +
                "\r\n        End Get" +
                "\r\n        Set(ByVal value as {0})" +
                "\r\n            _{1} = value" +
                "\r\n        End Set" +
                "\r\n    End Property",
                type, chunk.Name, chunk.Value);
            _source.WriteLine();
        }


        protected override void Visit(ViewDataModelChunk chunk)
        {
            if (Snippets.IsNullOrEmpty(chunk.TModelAlias))
                return;

            _source
                .Write("Public ReadOnly Property ")
                .WriteCode(chunk.TModelAlias)
                .Write("() As ")
                .WriteCode(chunk.TModel)
                .WriteLine().AddIndent();
            _source.WriteLine("Get").AddIndent();
            _source.WriteLine("Return ViewData.Model");
            _source.RemoveIndent().WriteLine("End Get");
            _source.RemoveIndent().WriteLine("End Property");
            CodeDefault();                
        }

        protected override void Visit(ViewDataChunk chunk)
        {
            var key = chunk.Key;
            var name = chunk.Name;
            var type = chunk.Type ?? "object";

            if (!_globalSymbols.ContainsKey(name))
                _globalSymbols.Add(name, null);

            if (_viewDataAdded.ContainsKey(name))
            {
                if (_viewDataAdded[name] != key + ":" + type)
                {
                    throw new CompilerException(
                        string.Format("The view data named {0} cannot be declared with different types '{1}' and '{2}'",
                                      name, type, _viewDataAdded[name]));
                }
                return;
            }

            _viewDataAdded.Add(name, key + ":" + type);
            //_source.WriteCode(type).Write(" ").WriteLine(name);
            _source
                .Write("Public ReadOnly Property ")
                .WriteCode(name)
                .Write("() As ")
                .WriteCode(type)
                .WriteLine().AddIndent()
                .WriteLine("Get").AddIndent();

            if (Snippets.IsNullOrEmpty(chunk.Default))
            {
                CodeIndent(chunk)
                    .Write("Return CType(ViewData.Eval(\"")
                    .Write(key)
                    .Write("\"),")
                    .WriteCode(type)
                    .WriteLine(")");
            }
            else
            {
                CodeIndent(chunk)
                    .Write("Return CType(If(ViewData.Eval(\"")
                    .Write(key)
                    .Write("\"),")
                    .WriteCode(chunk.Default)
                    .Write("),")
                    .WriteCode(type)
                    .WriteLine(")");
            }
            _source
                .RemoveIndent().WriteLine("End Get")
                .RemoveIndent().WriteLine("End Property");

            CodeDefault();
        }

        protected override void Visit(ExtensionChunk chunk)
        {
            chunk.Extension.VisitChunk(this, OutputLocation.ClassMembers, chunk.Body, _source.GetStringBuilder());
        }

        protected override void Visit(MacroChunk chunk)
        {
            _source
                .Write("Public Function ")
                .Write(chunk.Name)
                .Write("(");
            var delimiter = "";
            foreach (var parameter in chunk.Parameters)
            {
                _source
                    .Write(delimiter)
                    .Write(parameter.Name)
                    .Write(" As ")
                    .WriteCode(parameter.Type);
                delimiter = ", ";
            }
            _source
                .WriteLine(") As String")
                .AddIndent();
            _source
                .WriteLine("Using OutputScope(new Global.System.IO.StringWriter())")
                .AddIndent();

            var variables = new Dictionary<string, object>();
            foreach (var param in chunk.Parameters)
            {
                variables.Add(param.Name, null);
            }

            var generator = new GeneratedCodeVisitor(_source, variables, _nullBehaviour);
            generator.Accept(chunk.Body);

            _source
                .WriteLine("Return Output.ToString()")
                .RemoveIndent().WriteLine("End Using")
                .RemoveIndent().WriteLine("End Function");
        }
    }
}
