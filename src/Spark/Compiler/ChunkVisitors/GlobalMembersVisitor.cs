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

using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.ChunkVisitors
{
    public class GlobalMembersVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;

        public GlobalMembersVisitor(StringBuilder output)
        {
            _source = output;
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
            _source.AppendLine(string.Format("{0} {1}={2};", chunk.Type ?? "object", chunk.Name, chunk.Value));
        }

        protected override void Visit(ViewDataChunk chunk)
        {
            _source.AppendLine(string.Format("{0} {1} {{get {{return ({0})ViewData.Eval(\"{1}\");}}}}", chunk.Type ?? "object", chunk.Name));
        }

        protected override void Visit(ExtensionChunk chunk)
        {
            chunk.Extension.VisitChunk(this, OutputLocation.ClassMembers, chunk.Body, _source);
        }

        protected override void Visit(MacroChunk chunk)
        {
            _source.Append(string.Format("string {0}(", chunk.Name));
            string delimiter = "";
            foreach (var parameter in chunk.Parameters)
            {
                _source.Append(delimiter).Append(parameter.Type).Append(" ").Append(parameter.Name);
                delimiter = ", ";            
            }
            _source.AppendLine(")");
            _source.AppendLine("{ using(OutputScope(new System.IO.StringWriter())) { ");
            
            var generator = new GeneratedCodeVisitor(_source);
            generator.Accept(chunk.Body);

            _source.AppendLine("return Output.ToString(); }}");
        }
    }
}