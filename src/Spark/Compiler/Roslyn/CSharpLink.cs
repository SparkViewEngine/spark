using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Spark.Compiler.Roslyn;

public class CSharpLink : IRoslynCompilationLink
{
    public bool ShouldVisit(string languageOrExtension) => languageOrExtension is "c#" or "cs" or "csharp";

    public Assembly Compile(bool debug, string assemblyName, List<MetadataReference> references, IEnumerable<string> sourceCode)
    {
        var syntaxTrees = sourceCode.Select(source => CSharpSyntaxTree.ParseText(source));

        var optimizationLevel = debug ? OptimizationLevel.Debug : OptimizationLevel.Release;

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(optimizationLevel)
            .WithPlatform(Platform.AnyCpu);

        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: syntaxTrees,
            references: references,
            options: options);

        using var ms = new MemoryStream();

        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            var sb = new StringBuilder();

            foreach (var diagnostic in failures)
            {
                sb.Append(diagnostic.Id).Append(":").AppendLine(diagnostic.GetMessage());
            }

            throw new RoslynCompilerException(sb.ToString(), result);
        }

        ms.Seek(0, SeekOrigin.Begin);

        var assembly = Assembly.Load(ms.ToArray());

        return assembly;
    }
}