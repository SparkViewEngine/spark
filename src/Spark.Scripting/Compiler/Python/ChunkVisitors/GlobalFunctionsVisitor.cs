using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Scripting.Compiler.Python.ChunkVisitors
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
            _source.WriteLine("):");
            _source.Indent++;
            foreach (var global in _globals.Keys)
                _source.Write("global ").WriteLine(global);
            var generator = new GeneratedCodeVisitor(_source, _globals);
            _source.WriteLine("__output__scope__=OutputScopeAdapter(None)");
            
            _source.WriteLine("try:");
            _source.Indent++;
            generator.Accept(chunk.Body);
            _source.WriteLine("return Output.ToString()");
            _source.Indent--;

            _source.WriteLine("finally:");
            _source.Indent++;
            _source.WriteLine("__output__scope__.Dispose()");
            _source.Indent--;

            _source.Indent--;
        }
    }
}
