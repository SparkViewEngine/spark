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
        }
        public string BaseClass { get; set; }
        public string SourceCode { get; set; }
        public Type CompiledType { get; set; }

        public void CompileView(IList<Chunk> view)
        {
            CompileView(view, new Chunk[0]);
        }

        public void CompileView(IList<Chunk> view, IList<Chunk> master)
        {
            var source = new StringBuilder();
            var usingGenerator = new UsingNamespaceVisitor(source);
            var baseClassGenerator = new BaseClassVisitor { BaseClass = BaseClass };
            var globalsGenerator = new GlobalMembersVisitor(source);
            var viewGenerator = new GeneratedCodeVisitor(source) {Indent = 8};

            //usingGenerator.Using("System.Web.Mvc");
            usingGenerator.Accept(view);
            usingGenerator.Accept(master);

            baseClassGenerator.Accept(view);
            if (string.IsNullOrEmpty(baseClassGenerator.TModel))
                baseClassGenerator.Accept(master);

            source.AppendLine();
            source.AppendLine(string.Format("public class CompiledSparkView : {0}\r\n{{", baseClassGenerator.BaseClassTypeName));

            globalsGenerator.Accept(view);
            globalsGenerator.Accept(master);

            source.AppendLine();
            source.AppendLine("    public void RenderViewContent()");
            source.AppendLine("    {");
            viewGenerator.Accept(view);
            source.AppendLine("    }");

            if (master == null || master.Count == 0)
            {
                source.AppendLine();
                source.AppendLine("    public override void RenderView(System.IO.TextWriter writer)");
                source.AppendLine("    {");
                source.AppendLine("        using (OutputScope(writer)) {RenderViewContent();}");
                source.AppendLine("    }");
            }
            else
            {
                source.AppendLine();
                source.AppendLine("    public void RenderMasterContent()");
                source.AppendLine("    {");
                viewGenerator.Accept(master);
                source.AppendLine("    }");

                source.AppendLine();
                source.AppendLine("    public override void RenderView(System.IO.TextWriter writer)");
                source.AppendLine("    {");
                source.AppendLine("        using (OutputScope(\"view\")) {RenderViewContent();}");
                source.AppendLine("        using (OutputScope(writer)) {RenderMasterContent();}");
                source.AppendLine("    }");
            }

            source.AppendLine("}");


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
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Dynamic view compilation failed.");

                foreach (CompilerError err in compilerResults.Errors)
                {
                    sb.AppendFormat("generated.cs({0},{1}): {2} {3}: ", err.Line, err.Column, err.IsWarning ? "warning" : "error", err.ErrorNumber);
                    sb.AppendLine(err.ErrorText);
                }

                //foreach (var text in compilerResults.Output)
                //{
                //    sb.AppendLine(text);
                //}

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

            CompiledType = compilerResults.CompiledAssembly.GetType("CompiledSparkView");
        }

        public ISparkView CreateInstance()
        {
            return (ISparkView)Activator.CreateInstance(CompiledType);
        }
    }
}