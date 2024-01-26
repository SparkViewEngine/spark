using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Spark.Compiler.Roslyn;

public class VisualBasicLink : IRoslynCompilationLink
{
    public bool ShouldVisit(string languageOrExtension) => languageOrExtension is "vb" or "vbs" or "visualbasic" or "vbscript";

    public Assembly Compile(bool debug, string assemblyName, string outputAssembly, IEnumerable<MetadataReference> references, IEnumerable<string> sourceCode)
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

        EmitResult result;
        Assembly assembly = null;

        if (string.IsNullOrEmpty(outputAssembly))
        {
            using var ms = new MemoryStream();
            
            result = compilation.Emit(ms);

            CSharpLink.ThrowIfCompilationNotSuccessful(result);

            ms.Seek(0, SeekOrigin.Begin);

            assembly = Assembly.Load(ms.ToArray());
        }
        else
        {
            using var fs = new FileStream(outputAssembly, FileMode.Create);

            result = compilation.Emit(fs);

            CSharpLink.ThrowIfCompilationNotSuccessful(result);

            assembly = Assembly.Load(outputAssembly);
        }

        return assembly;
    }
}