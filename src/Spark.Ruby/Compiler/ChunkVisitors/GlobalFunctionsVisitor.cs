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
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using Spark.Ruby.Compiler;

namespace Spark.Ruby.Compiler.ChunkVisitors
{
    public class GlobalFunctionsVisitor : ChunkVisitor
    {
        private readonly SourceWriter _source;
        private readonly IDictionary<string, object> _globals;

        public GlobalFunctionsVisitor(SourceWriter source, IDictionary<string, object> globals)
        {
            _source = source;
            _globals = globals;
        }

        protected override void Visit(MacroChunk chunk)
        {
            _source.Write("def ").Write(chunk.Name).Write("(");
            string delimiter = "";
            foreach (var parameter in chunk.Parameters)
            {
                _source.Write(delimiter).Write(parameter.Name);
                delimiter = ",";
            }
            _source.WriteLine(")");
            _source.Indent++;

            var generator = new GeneratedCodeVisitor(_source, _globals);
            _source.WriteLine("__output__scope__=output_scope");

            _source.WriteLine("begin");
            _source.Indent++;
            generator.Accept(chunk.Body);
            _source.WriteLine("return output.to_string");
            _source.Indent--;
            _source.WriteLine("ensure");
            _source.Indent++;
            _source.WriteLine("__output__scope__.dispose");
            _source.Indent--;
            _source.WriteLine("end");

            _source.Indent--;
            _source.WriteLine("end");
        }
    }
}