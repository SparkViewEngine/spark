/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Linq;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using Spark.Compiler.ChunkVisitors;
using Spark;

namespace Spark.Compiler
{
    public class ViewCompiler
    {
        public ViewCompiler(string baseClass)
        {
            BaseClass = baseClass;
            GeneratedViewId = Guid.NewGuid();
        }
        public ViewCompiler(string baseClass, string targetNamespace)
        {
            BaseClass = baseClass;
            GeneratedViewId = Guid.NewGuid();
            TargetNamespace = targetNamespace;
        }

        public string BaseClass { get; set; }
        public string TargetNamespace { get; set; }
        public string SourceCode { get; set; }
        public Type CompiledType { get; set; }
        public Guid GeneratedViewId { get; set; }

        public void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            var source = new StringBuilder();
            var usingGenerator = new UsingNamespaceVisitor(source);
            var baseClassGenerator = new BaseClassVisitor { BaseClass = BaseClass };
            var globalsGenerator = new GlobalMembersVisitor(source);
            var viewGenerator = new GeneratedCodeVisitor(source) { Indent = 8 };

            foreach (var resource in allResources)
                usingGenerator.Accept(resource);

            foreach (var resource in allResources)
                baseClassGenerator.Accept(resource);

            if (!string.IsNullOrEmpty(TargetNamespace))
            {
                source.AppendLine();
                source.AppendLine(string.Format("namespace {0}", TargetNamespace));
                source.AppendLine("{");
            }

            source.AppendLine();
            source.AppendLine(string.Format("public class CompiledSparkView : {0}", baseClassGenerator.BaseClassTypeName));
            source.AppendLine("{");

            source.AppendLine();
            source.AppendLine("  public override System.Guid GeneratedViewId");
            source.AppendLine(string.Format("  {{ get {{ return new System.Guid(\"{0:n}\"); }} }}", GeneratedViewId));

            foreach (var resource in allResources)
                globalsGenerator.Accept(resource);

            int renderLevel = 0;
            foreach (var viewTemplate in viewTemplates)
            {
                source.AppendLine();
                source.AppendLine(string.Format("    public void RenderViewLevel{0}()", renderLevel));
                source.AppendLine("    {");
                viewGenerator.Accept(viewTemplate);
                source.AppendLine("    }");
                ++renderLevel;
            }


            source.AppendLine();
            source.AppendLine("    public override void RenderView(System.IO.TextWriter writer)");
            source.AppendLine("    {");
            for (int invokeLevel = 0; invokeLevel != renderLevel; ++invokeLevel)
            {
                if (invokeLevel != renderLevel - 1)
                {
                    source.AppendLine(string.Format("        using (OutputScope(new System.IO.StringWriter())) {{RenderViewLevel{0}(); Content[\"view\"] = Output;}}", invokeLevel));
                }
                else
                {
                    source.AppendLine(string.Format("        using (OutputScope(writer)) {{RenderViewLevel{0}();}}", invokeLevel));
                }
            }
            source.AppendLine("    }");

            source.AppendLine("}");

            if (!string.IsNullOrEmpty(TargetNamespace))
            {
                source.AppendLine("}");
            }

            SourceCode = source.ToString();

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v3.5" } };

            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

            var compilerParameters = new CompilerParameters();
            compilerParameters.IncludeDebugInformation = true;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string location;
                try
                {
                    location = assembly.Location;
                }
                catch (NotSupportedException)
                {
                    continue;
                }
                compilerParameters.ReferencedAssemblies.Add(location);
            }


            var codeFile = Path.Combine(AppDomain.CurrentDomain.SetupInformation.DynamicBase ?? Path.GetTempPath(), Guid.NewGuid().ToString("n") + ".cs");
            using (var stream = new FileStream(codeFile, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(SourceCode);
                }
            }

            compilerParameters.OutputAssembly = Path.ChangeExtension(codeFile, "dll");
            var compilerResults = codeProvider.CompileAssemblyFromFile(compilerParameters, codeFile);

            if (compilerResults.Errors.Count != 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Dynamic view compilation failed.");

                foreach (CompilerError err in compilerResults.Errors)
                {
                    sb.AppendFormat("generated.cs({0},{1}): {2} {3}: ", err.Line, err.Column, err.IsWarning ? "warning" : "error", err.ErrorNumber);
                    sb.AppendLine(err.ErrorText);
                }

                sb.AppendLine();
                using (TextReader reader = new StringReader(SourceCode))
                {
                    for (int lineNumber = 1; ; ++lineNumber)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                            break;
                        sb.Append(lineNumber).Append(' ').AppendLine(line);
                    }
                }
                throw new CompilerException(sb.ToString());
            }

            CompiledType = compilerResults.CompiledAssembly.GetType((TargetNamespace ?? "") + ".CompiledSparkView");
        }

        public ISparkView CreateInstance()
        {
            return (ISparkView)Activator.CreateInstance(CompiledType);
        }
    }
}
