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

using System.Linq;
using System.Collections.Generic;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.ChunkVisitors
{
    public class GlobalMembersVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;
        readonly Dictionary<string, string> _viewDataAdded = new Dictionary<string, string>();
        readonly Dictionary<string, GlobalVariableChunk> _globalAdded = new Dictionary<string, GlobalVariableChunk>();

        public GlobalMembersVisitor(StringBuilder output)
        {
            _source = output;
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
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
        
        protected override void Visit(ViewDataChunk chunk)
        {
            var name = chunk.Name;
            var type = chunk.Type ?? "object";

            if (_viewDataAdded.ContainsKey(name))
            {
                if (_viewDataAdded[name] != type)
                {
                    throw new CompilerException(string.Format("The view data named {0} cannot be declared with different types '{1}' and '{2}'",
                        name, type, _viewDataAdded[name]));
                }
                return;
            }

            _viewDataAdded.Add(name, type);
            _source.AppendLine(string.Format("\r\n    {0} {1}\r\n    {{get {{return ({0})ViewData.Eval(\"{1}\");}}}}", type, name));
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
            _source.AppendLine("    {");
            _source.AppendLine("        using(OutputScope(new System.IO.StringWriter()))");
            _source.AppendLine("        {");
            
            var generator = new GeneratedCodeVisitor(_source) {Indent = 12};
            generator.Accept(chunk.Body);

            _source.AppendLine("            return Output.ToString();");
            _source.AppendLine("        }");
            _source.AppendLine("    }");
        }
    }
}