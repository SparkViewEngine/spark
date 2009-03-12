// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.CSharp;

namespace Spark.Compiler
{
    public class BatchCompiler
    {
        public string OutputAssembly { get; set; }

        public Assembly Compile(bool debug, params string[] sourceCode)
        {
            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v3.5" } };

            var codeProvider = new CSharpCodeProvider(providerOptions);

            var compilerParameters = new CompilerParameters();


            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly is AssemblyBuilder)
                    continue;

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

            CompilerResults compilerResults;
            var basePath = AppDomain.CurrentDomain.SetupInformation.DynamicBase ?? Path.GetTempPath();
            if (debug)
            {
                compilerParameters.IncludeDebugInformation = true;

                var baseFile = Path.Combine(basePath, Guid.NewGuid().ToString("n"));

                var codeFiles = new List<string>();
                int fileCount = 0;
                foreach (string sourceCodeItem in sourceCode)
                {
                    ++fileCount;
                    var codeFile = baseFile + "-" + fileCount + ".cs";
                    using (var stream = new FileStream(codeFile, FileMode.Create, FileAccess.Write))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(sourceCodeItem);
                        }
                    }
                    codeFiles.Add(codeFile);
                }

                if (!string.IsNullOrEmpty(OutputAssembly))
                {
                    compilerParameters.OutputAssembly = Path.Combine(basePath, OutputAssembly);
                }
                else
                {
                    compilerParameters.OutputAssembly = baseFile + ".dll";
                }
                compilerResults = codeProvider.CompileAssemblyFromFile(compilerParameters, codeFiles.ToArray());
            }
            else
            {
                if (!string.IsNullOrEmpty(OutputAssembly))
                {
                    compilerParameters.OutputAssembly = Path.Combine(basePath, OutputAssembly);
                }
                else
                {
                    // This should result in the assembly being loaded without keeping the file on disk
                    compilerParameters.GenerateInMemory = true;
                }

                compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, sourceCode);
            }

            if (compilerResults.Errors.Count != 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Dynamic view compilation failed.");

                foreach (CompilerError err in compilerResults.Errors)
                {
                    sb.AppendFormat("{4}({0},{1}): {2} {3}: ", err.Line, err.Column, err.IsWarning ? "warning" : "error", err.ErrorNumber, err.FileName);
                    sb.AppendLine(err.ErrorText);
                }

                sb.AppendLine();
                foreach (var sourceCodeItem in sourceCode)
                {
                    using (var reader = new StringReader(sourceCodeItem))
                    {
                        for (int lineNumber = 1; ; ++lineNumber)
                        {
                            var line = reader.ReadLine();
                            if (line == null)
                                break;
                            sb.Append(lineNumber).Append(' ').AppendLine(line);
                        }
                    }
                }
                throw new CompilerException(sb.ToString());
            }   

            return compilerResults.CompiledAssembly;
        }
    }
}
