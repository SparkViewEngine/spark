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