using Microsoft.CodeAnalysis.Emit;

namespace Spark.Compiler.Roslyn
{
    public class RoslynCompilerException(string message, EmitResult emitResult) : CompilerException(message)
    {
        public EmitResult EmitResult { get; set; } = emitResult;
    }
}