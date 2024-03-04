using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Spark.Compiler.Roslyn;

public interface IRoslynCompilationLink
{
    /// <summary>
    /// When true, only the <see cref="Compile"/> method for this link should be executed.
    /// </summary>
    /// <param name="languageOrExtension"></param>
    /// <returns></returns>
    bool ShouldVisit(string languageOrExtension);

    /// <summary>
    /// Compiles the specified <see cref="sourceCode"/>.
    /// </summary>
    /// <param name="debug"></param>
    /// <param name="assemblyName"></param>
    /// <param name="outputAssembly">When set the source code is compiled to a file.</param>
    /// <param name="references"></param>
    /// <param name="sourceCode"></param>
    /// <returns></returns>
    Assembly Compile(bool debug, string assemblyName, string outputAssembly, IEnumerable<MetadataReference> references, IEnumerable<string> sourceCode);
}