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
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using Spark.Ruby.Compiler;

namespace Spark.Ruby.Compiler.ChunkVisitors
{
    public class GlobalMembersVisitor : ChunkVisitor
    {
        private readonly SourceWriter _source;
        private readonly Dictionary<string, object> _globals;

        public GlobalMembersVisitor(SourceWriter sourceWriter, Dictionary<string, object> globals)
        {
            _source = sourceWriter;
            _globals = globals;
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
            if (!_globals.ContainsKey(chunk.Name))
                _globals.Add(chunk.Name, null);

            _source.Write("attr_accessor :").WriteLine(chunk.Name);
        }
    }
}