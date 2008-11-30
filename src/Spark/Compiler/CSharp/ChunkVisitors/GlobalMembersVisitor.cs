// Copyright 2008 Louis DeJardin - http://whereslou.com
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

namespace Spark.Compiler.CSharp.ChunkVisitors
{
    public class GlobalMembersVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;
        private readonly Dictionary<string, object> _globalSymbols;
    	private readonly NullBehaviour _nullBehaviour;
    	readonly Dictionary<string, string> _viewDataAdded = new Dictionary<string, string>();
        readonly Dictionary<string, GlobalVariableChunk> _globalAdded = new Dictionary<string, GlobalVariableChunk>();
        private int _indent = 4;

		public GlobalMembersVisitor(StringBuilder output, Dictionary<string, object> globalSymbols, NullBehaviour nullBehaviour)
        {
            _source = output;
            _globalSymbols = globalSymbols;
			_nullBehaviour = nullBehaviour;
        }

        private int Indent
        {
            get { return _indent; }
        }

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

        protected override void Visit(GlobalVariableChunk chunk)
        {
            if (!_globalSymbols.ContainsKey(chunk.Name))
                _globalSymbols.Add(chunk.Name, null);

            if (_globalAdded.ContainsKey(chunk.Name))
            {
                if (_globalAdded[chunk.Name].Type != chunk.Type ||
                    _globalAdded[chunk.Name].Value != chunk.Value)
                {
                    throw new CompilerException(string.Format("The global named {0} cannot be declared repeatedly with different types or values",
                                                              chunk.Name));
                }
                return;
            }

            var type = chunk.Type ?? "object";
            var typeParts = type.Split(' ', '\t');
            if (typeParts.Contains("const") || typeParts.Contains("readonly"))
            {
                _source.AppendFormat("\r\n    {0} {1} = {2};",
                                     type, chunk.Name, chunk.Value);
            }
            else
            {
                _source.AppendFormat(
                    "\r\n    {0} _{1} = {2};\r\n    public {0} {1} {{ get {{return _{1};}} set {{_{1} = value;}} }}",
                    type, chunk.Name, chunk.Value);
            }
            _source.AppendLine();
        }


        protected override void Visit(ViewDataModelChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.TModelAlias))
                return;

            AppendIndent().Append(chunk.TModel).Append(" ").AppendLine(chunk.TModelAlias);
            CodeIndent(chunk).AppendLine("{get {return ViewData.Model;}}");
            CodeDefault();
        }

        protected override void Visit(ViewDataChunk chunk)
        {
            var key = chunk.Key;
            var name = chunk.Name;
            var type = chunk.Type ?? "object";

            if (!_globalSymbols.ContainsKey(chunk.Name))
                _globalSymbols.Add(chunk.Name, null);

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
            AppendIndent().Append(type).Append(" ").AppendLine(name);
            if (string.IsNullOrEmpty(chunk.Default))
            {
                CodeIndent(chunk)
                    .Append("{get {return (")
                    .Append(type)
                    .Append(")ViewData.Eval(\"")
                    .Append(key)
                    .AppendLine("\");}}");
            }
            else
            {
                CodeIndent(chunk)
                    .Append("{get {return (")
                    .Append(type)
                    .Append(")(ViewData.Eval(\"")
                    .Append(key)
                    .Append("\")??")
                    .Append(chunk.Default)
                    .AppendLine(");}}");
            }
            CodeDefault();
        }

        protected override void Visit(ExtensionChunk chunk)
        {
            chunk.Extension.VisitChunk(this, OutputLocation.ClassMembers, chunk.Body, _source);
        }

        protected override void Visit(MacroChunk chunk)
        {
            _source.Append(string.Format("\r\n    string {0}(", chunk.Name));
            string delimiter = "";
            foreach (var parameter in chunk.Parameters)
            {
                _source.Append(delimiter).Append(parameter.Type).Append(" ").Append(parameter.Name);
                delimiter = ", ";
            }
            _source.AppendLine(")");
            CodeIndent(chunk).AppendLine("{");
            CodeHidden();
            _source.AppendLine("        using(OutputScope(new System.IO.StringWriter()))");
            _source.AppendLine("        {");

            CodeDefault();
            var generator = new GeneratedCodeVisitor(_source, null, _nullBehaviour) { Indent = 12 };
            generator.Accept(chunk.Body);

            CodeHidden();
            _source.AppendLine("            return Output.ToString();");
            _source.AppendLine("        }");
            _source.AppendLine("    }");
            CodeDefault();
        }
    }
}
