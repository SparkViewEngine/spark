using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Spark.Compiler.Roslyn
{
    public class RoslynBatchCompiler : IBatchCompiler
    {
        private readonly IEnumerable<IRoslynCompilationLink> links;

        public RoslynBatchCompiler()
        {
            this.links = new IRoslynCompilationLink[] { new CSharpLink(), new VisualBasicLink() };
        }

        public RoslynBatchCompiler(IEnumerable<IRoslynCompilationLink> links)
        {
            this.links = links;
        }

        /// <summary>
        /// Compiles the <see cref="sourceCode"/> in the specified <see cref="languageOrExtension"/>.
        /// </summary>
        /// <param name="debug">When true the source is compiled in debug mode.</param>
        /// <param name="languageOrExtension">E.g. "csharp" or "visualbasic"</param>
        /// <param name="outputAssembly">E.g. "File.Name.dll" (optional)</param>
        /// <param name="sourceCode">The source code to compile.</param>
        /// <returns></returns>
        public Assembly Compile(bool debug, string languageOrExtension, string outputAssembly, IEnumerable<string> sourceCode)
        {
            Assembly assembly = null;

            if (!this.links.Any())
            {
                throw new ConfigurationErrorsException("No IRoslynCompilationLink links");
            }

            string assemblyName;
            if (!string.IsNullOrEmpty(outputAssembly))
            {
                var basePath = Path.GetTempPath();
                assemblyName = Path.Combine(basePath, outputAssembly);
            }
            else
            {
                assemblyName = Path.GetRandomFileName();
            }

            var references = new List<MetadataReference>();
            
            foreach (var currentAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (currentAssembly.IsDynamic())
                {
                    continue;
                }

                var reference = MetadataReference.CreateFromFile(currentAssembly.Location);

                references.Add(reference);
            }

            var match = false;
            foreach (var visitor in this.links)
            {
                if (visitor.ShouldVisit(languageOrExtension))
                {
                    match = true;

                    assembly = visitor.Compile(debug, assemblyName, references, sourceCode);

                    // Chain of responsibility pattern
                    break;
                }
            }

            if (!match)
            {
                throw new ArgumentOutOfRangeException(nameof(languageOrExtension), languageOrExtension, "Un-handled value");
            }

            return assembly;
        }
    }
}