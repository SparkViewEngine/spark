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