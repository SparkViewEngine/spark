using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Spark.Compiler.Roslyn;

public class VisualBasicLink : IRoslynCompilationLink
{
    public bool ShouldVisit(string languageOrExtension) => languageOrExtension is "vb" or "vbs" or "visualbasic" or "vbscript";

    public Assembly Compile(bool debug, string assemblyName, List<MetadataReference> references, IEnumerable<string> sourceCode)
    {
        var syntaxTrees = sourceCode.Select(source => VisualBasicSyntaxTree.ParseText(source));

        var optimizationLevel = debug ? OptimizationLevel.Debug : OptimizationLevel.Release;

        var options = new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(optimizationLevel)
            .WithPlatform(Platform.AnyCpu);

        var compilation = VisualBasicCompilation.Create(
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