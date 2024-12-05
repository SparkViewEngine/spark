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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    public class JavascriptPreRenderVisitor : ChunkVisitor
    {
        private readonly StringBuilder _source;

        public JavascriptPreRenderVisitor(StringBuilder source)
        {
            _source = source;
        }
        protected override void Visit(GlobalVariableChunk chunk)
        {
            _source
                .Append("var ")
                .Append(chunk.Name)
                .Append(" = this.")
                .Append(chunk.Name)
                .AppendLine(";");
        }

        protected override void Visit(DefaultVariableChunk chunk)
        {
            _source.Append("if (typeof(")
                .Append(chunk.Name)
                .Append(") === 'undefined') ")
                .Append(chunk.Name)
                .Append(" = ")
                .Append(chunk.Value)
                .AppendLine(";");
        }  

        protected override void Visit(ViewDataChunk chunk)
        {
            _source
                .Append("var ")
                .Append(chunk.Name)
                .Append(" = viewData[\"")
                .Append(chunk.Key)
                .AppendLine("\"];");
        }

        protected override void Visit(MacroChunk chunk)
        {
            _source.Append("function ").Append(chunk.Name).Append("(");
            string delimiter = "";
            foreach (var parameter in chunk.Parameters)
            {
                _source.Append(delimiter).Append(parameter.Name);
                delimiter = ", ";
            }
            _source.AppendLine(") {var __output__ = new StringWriter(); OutputScope(__output__);");
            var codeVisitor = new JavascriptGeneratedCodeVisitor(_source);
            codeVisitor.Accept(chunk.Body);
            _source.AppendLine("DisposeOutputScope(); return __output__.toString();}");            
        }         
    }
}
