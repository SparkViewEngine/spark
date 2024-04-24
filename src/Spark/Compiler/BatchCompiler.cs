using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CSharp;
using Spark.Compiler.Roslyn;

namespace Spark.Compiler;

#if NETFRAMEWORK

public class CodeDomBatchCompiler : IBatchCompiler
{
    /// <summary>
    /// Compiles the <see cref="sourceCode"/> in the specified <see cref="languageOrExtension"/>.
    /// </summary>
    /// <param name="debug">When true the source is compiled in debug mode.</param>
    /// <param name="languageOrExtension">E.g. "csharp" or "visualbasic"</param>
    /// <param name="outputAssembly">E.g. "File.Name.dll" (optional)</param>
    /// <param name="sourceCode">The source code to compile.</param>
    /// <param name="excludeAssemblies">The full name of assemblies to exclude.</param>
    /// <returns></returns>
    /// <exception cref="CompilerException"></exception>
    /// <exception cref="CodeDomCompilerException"></exception>
    public Assembly Compile(bool debug, string languageOrExtension, string outputAssembly, IEnumerable<string> sourceCode, IEnumerable<string> excludeAssemblies)
    {
        var language = languageOrExtension;
        if (CodeDomProvider.IsDefinedLanguage(languageOrExtension) == false &&
            CodeDomProvider.IsDefinedExtension(languageOrExtension))
        {
            language = CodeDomProvider.GetLanguageFromExtension(languageOrExtension);
        }

        CodeDomProvider codeProvider;
        CompilerParameters compilerParameters;

        if (ConfigurationManager.GetSection("system.codedom") != null)
        {
            var compilerInfo = CodeDomProvider.GetCompilerInfo(language);
            codeProvider = compilerInfo.CreateProvider();
            compilerParameters = compilerInfo.CreateDefaultCompilerParameters();
        }
        else
        {
            if (!language.Equals("c#", StringComparison.OrdinalIgnoreCase) &&
                !language.Equals("cs", StringComparison.OrdinalIgnoreCase) &&
                !language.Equals("csharp", StringComparison.OrdinalIgnoreCase))
            {
                throw new CompilerException(
                    $"When running the {typeof(CodeDomBatchCompiler).FullName} in an AppDomain without a system.codedom config section only the csharp language is supported. This happens if you are precompiling your views.");
            }

            var compilerVersion = GetCompilerVersion();

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", compilerVersion } };
            codeProvider = new CSharpCodeProvider(providerOptions);
            compilerParameters = new CompilerParameters();
        }

        compilerParameters.TreatWarningsAsErrors = false;
        var extension = codeProvider.FileExtension;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic())
            {
                continue;
            }

            if (excludeAssemblies.Contains(assembly.FullName))
            {
                continue;
            }

            compilerParameters.ReferencedAssemblies.Add(assembly.Location);
        }

        CompilerResults compilerResults;

        // ReSharper disable once RedundantAssignment
        var dynamicBase = string.Empty;

        dynamicBase = AppDomain.CurrentDomain.SetupInformation.DynamicBase;

        var basePath = !string.IsNullOrEmpty(dynamicBase) ? dynamicBase : Path.GetTempPath();
        compilerParameters.TempFiles = new TempFileCollection(basePath); //Without this, the generated code throws Access Denied exception with Impersonate mode on platforms like SharePoint
        if (debug)
        {
            compilerParameters.IncludeDebugInformation = true;

            var baseFile = Path.Combine(basePath, Guid.NewGuid().ToString("n"));

            var codeFiles = new List<string>();
            int fileCount = 0;
            foreach (string sourceCodeItem in sourceCode)
            {
                ++fileCount;
                var codeFile = baseFile + "-" + fileCount + "." + extension;
                using (var stream = new FileStream(codeFile, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(sourceCodeItem);
                    }
                }
                codeFiles.Add(codeFile);
            }

            if (!string.IsNullOrEmpty(outputAssembly))
            {
                compilerParameters.OutputAssembly = Path.Combine(basePath, outputAssembly);
            }
            else
            {
                compilerParameters.OutputAssembly = baseFile + ".dll";
            }
            compilerResults = codeProvider.CompileAssemblyFromFile(compilerParameters, codeFiles.ToArray());
        }
        else
        {
            if (!string.IsNullOrEmpty(outputAssembly))
            {
                compilerParameters.OutputAssembly = Path.Combine(basePath, outputAssembly);
            }
            else
            {
                // This should result in the assembly being loaded without keeping the file on disk
                compilerParameters.GenerateInMemory = true;
            }

            compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, sourceCode.ToArray());
        }

        if (compilerResults.Errors.HasErrors)
        {
            var sb = new StringBuilder().AppendLine("Dynamic view compilation failed.");

            foreach (CompilerError err in compilerResults.Errors)
            {
                sb.AppendFormat("{4}({0},{1}): {2} {3}: ", err.Line, err.Column, err.IsWarning ? "warning" : "error", err.ErrorNumber, err.FileName);
                sb.AppendLine(err.ErrorText);
            }

            sb.AppendLine();
            foreach (var sourceCodeItem in sourceCode)
            {
                using var reader = new StringReader(sourceCodeItem);

                for (int lineNumber = 1; ; ++lineNumber)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    sb.Append(lineNumber).Append(' ').AppendLine(line);
                }
            }
            throw new CodeDomCompilerException(sb.ToString(), compilerResults);
        }

        return compilerResults.CompiledAssembly;
    }

    private static string GetCompilerVersion()
    {
        return "v4.0";
    }
}

public class CodeDomCompilerException(string message, CompilerResults results) : CompilerException(message)
{
    public CompilerResults Results { get; set; } = results;
}

#endif

public class RoslynBatchCompiler : IBatchCompiler
{
    private readonly IEnumerable<IRoslynCompilationLink> links;
    private readonly ISparkSettings settings;

    private readonly IList<PortableExecutableReference> References;

    public RoslynBatchCompiler(ISparkSettings settings) : this(new IRoslynCompilationLink[] { new CSharpLink(), new VisualBasicLink() }, settings)
    {
    }

    public RoslynBatchCompiler(IEnumerable<IRoslynCompilationLink> links, ISparkSettings settings)
    {
        this.links = links;
        this.settings = settings;
        this.References = new List<PortableExecutableReference>();
    }

    public void AddAssemblies(params string[] dlls)
    {
        foreach (var dll in dlls)
        {
            this.AddAssembly(dll);
        }
    }

    public bool AddAssembly(string assemblyDll)
    {
        if (string.IsNullOrEmpty(assemblyDll))
        {
            return false;
        }

        var file = Path.GetFullPath(assemblyDll);

        if (!File.Exists(file))
        {
            // Check framework or dedicated runtime app folder
            var path = Path.GetDirectoryName(typeof(object).Assembly.Location);
            file = Path.Combine(path, assemblyDll);
            if (!File.Exists(file))
            {
                return false;
            }
        }

        if (this.References.Any(r => r.FilePath == file))
        {
            return true;
        }

        try
        {
            var reference = MetadataReference.CreateFromFile(file);
            this.References.Add(reference);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public bool AddAssembly(Type type)
    {
        try
        {
            if (this.References.Any(r => r.FilePath == type.Assembly.Location))
                return true;

            var systemReference = MetadataReference.CreateFromFile(type.Assembly.Location);
            this.References.Add(systemReference);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public void AddNetCoreDefaultReferences()
    {
        var rtPath = Path.GetDirectoryName(typeof(object).Assembly.Location) + Path.DirectorySeparatorChar;

        this.AddAssemblies(
            rtPath + "System.Private.CoreLib.dll",
            rtPath + "System.Runtime.dll",
            rtPath + "System.Console.dll",
            rtPath + "netstandard.dll",

            rtPath + "System.Text.RegularExpressions.dll", // IMPORTANT!
            rtPath + "System.Linq.dll",
            rtPath + "System.Linq.Expressions.dll", // IMPORTANT!

            rtPath + "System.IO.dll",
            rtPath + "System.Net.Primitives.dll",
            rtPath + "System.Net.Http.dll",
            rtPath + "System.Private.Uri.dll",
            rtPath + "System.Reflection.dll",
            rtPath + "System.ComponentModel.Primitives.dll",
            rtPath + "System.Globalization.dll",
            rtPath + "System.Collections.Concurrent.dll",
            rtPath + "System.Collections.NonGeneric.dll",
            rtPath + "Microsoft.CSharp.dll");

        // this library and CodeAnalysis libs
        this.AddAssembly(typeof(RoslynBatchCompiler)); // Scripting Library
    }

    public void AddNetFrameworkDefaultReferences()
    {
        this.AddAssembly("mscorlib.dll");
        this.AddAssembly("System.dll");
        this.AddAssembly("System.Core.dll");
        this.AddAssembly("Microsoft.CSharp.dll");
        this.AddAssembly("System.Net.Http.dll");
    }

    /// <summary>
    /// Compiles the <see cref="sourceCode"/> in the specified <see cref="languageOrExtension"/>.
    /// </summary>
    /// <param name="debug">When true the source is compiled in debug mode.</param>
    /// <param name="languageOrExtension">E.g. "csharp" or "visualbasic"</param>
    /// <param name="outputAssembly">E.g. "File.Name.dll" (optional)</param>
    /// <param name="sourceCode">The source code to compile.</param>
    /// <param name="excludeAssemblies">The full name of assemblies to exclude.</param>
    /// <returns></returns>
    public Assembly Compile(bool debug, string languageOrExtension, string outputAssembly, IEnumerable<string> sourceCode, IEnumerable<string> excludeAssemblies)
    {
        Assembly assembly = null;

        if (!this.links.Any())
        {
            throw new ConfigurationErrorsException("No IRoslynCompilationLink links");
        }

        var assemblyName = !string.IsNullOrEmpty(outputAssembly)
            // Strips the path from the outputAssembly full path...
            ? Path.GetFileName(outputAssembly)
            // ... or generates a random assembly name
            : Path.GetRandomFileName();

        assemblyName = Path.GetFileNameWithoutExtension(assemblyName);

        // We need to add different references when we target .net framework or .net core
        // https://github.com/jaredpar/basic-reference-assemblies/
        // https://weblog.west-wind.com/posts/2022/Jun/07/Runtime-CSharp-Code-Compilation-Revisited-for-Roslyn#adding-references
        
#if NETFRAMEWORK
        this.AddNetFrameworkDefaultReferences();
#else
        this.AddNetCoreDefaultReferences();
#endif

        // TODO: Is this needed?
        //this.AddAssembly(typeof(System.Drawing.Color));
        foreach(var assemblyLocation in settings.UseAssemblies)
        {
            // Assumes full path to assemblies
            if (assemblyLocation.EndsWith(".dll"))
            {
                this.AddAssembly(assemblyLocation);
            }
        }
        
        foreach (var currentAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (currentAssembly.IsDynamic())
            {
                continue;
            }

            if (excludeAssemblies.Contains(currentAssembly.FullName))
            {
                continue;
            }

            var reference = MetadataReference.CreateFromFile(currentAssembly.Location);

            this.References.Add(reference);
        }

        var match = false;
        foreach (var visitor in this.links)
        {
            if (visitor.ShouldVisit(languageOrExtension))
            {
                match = true;

                assembly = visitor.Compile(debug, assemblyName, outputAssembly, this.References, sourceCode);

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

public class RoslynCompilerException(string message, EmitResult emitResult) : CompilerException(message)
{
    public EmitResult EmitResult { get; set; } = emitResult;
}