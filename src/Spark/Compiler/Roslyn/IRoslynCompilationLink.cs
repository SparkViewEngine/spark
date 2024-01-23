using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Spark.Compiler.Roslyn;

public interface IRoslynCompilationLink
{
    bool ShouldVisit(string languageOrExtension);

    Assembly Compile(bool debug, string assemblyName, List<MetadataReference> references, IEnumerable<string> sourceCode);
}