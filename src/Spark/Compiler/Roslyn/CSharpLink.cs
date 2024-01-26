using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Spark.Compiler.Roslyn;

public class CSharpLink : IRoslynCompilationLink
{
    public bool ShouldVisit(string languageOrExtension) => languageOrExtension is "c#" or "cs" or "csharp";

    public static void ThrowIfCompilationNotSuccessful(EmitResult result)
    {
        if (result.Success)
        {
            return;
        }

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

    public Assembly Compile(bool debug, string assemblyName, string outputAssembly, IEnumerable<MetadataReference> references, IEnumerable<string> sourceCode)
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

        EmitResult result;
        Assembly assembly = null;

        if (string.IsNullOrEmpty(outputAssembly))
        {
            using var ms = new MemoryStream();
            
            result = compilation.Emit(ms);

            ThrowIfCompilationNotSuccessful(result);

            ms.Seek(0, SeekOrigin.Begin);

            assembly = Assembly.Load(ms.ToArray());
        }
        else
        {
            using (var fs = new FileStream(outputAssembly, FileMode.Create))
            {
                result = compilation.Emit(fs);
            }

            ThrowIfCompilationNotSuccessful(result);

            assembly = Assembly.Load(outputAssembly);
        }

        return assembly;
    }
}