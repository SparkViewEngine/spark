using System.Collections.Generic;
using System.Reflection;

namespace Spark.Compiler
{
    public interface IBatchCompiler
    {
        /// <summary>
        /// Compiles the <see cref="sourceCode"/> in the specified <see cref="languageOrExtension"/>.
        /// </summary>
        /// <param name="debug">When true the source is compiled in debug mode.</param>
        /// <param name="languageOrExtension">E.g. "csharp" or "visualbasic"</param>
        /// <param name="outputAssembly">E.g. "File.Name.dll" (optional)</param>
        /// <param name="sourceCode">The source code to compile.</param>
        /// <returns></returns>
        Assembly Compile(bool debug, string languageOrExtension, string outputAssembly, IEnumerable<string> sourceCode);
    }
}
