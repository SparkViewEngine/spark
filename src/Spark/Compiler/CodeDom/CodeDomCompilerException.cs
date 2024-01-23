using System.CodeDom.Compiler;

namespace Spark.Compiler.CodeDom
{
    public class CodeDomCompilerException(string message, CompilerResults results) : CompilerException(message)
    {
        public CompilerResults Results { get; set; } = results;
    }
}