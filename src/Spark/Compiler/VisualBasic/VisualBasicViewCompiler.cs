// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.VisualBasic.ChunkVisitors;

namespace Spark.Compiler.VisualBasic
{
    public class VisualBasicViewCompiler : ViewCompiler
    {
        public override void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            GenerateSourceCode(viewTemplates, allResources);

            var batchCompiler = new BatchCompiler();
            var assembly = batchCompiler.Compile(Debug, "visualbasic", SourceCode);
            CompiledType = assembly.GetType(ViewClassFullName);
        }


        public override void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            var globalSymbols = new Dictionary<string, object>();

            var source = new StringBuilder();
            // debug symbols not adjusted until the matching-directive issue resolved
            var builder = new SourceBuilder(source) { AdjustDebugSymbols = false };
            var usingGenerator = new UsingNamespaceVisitor(builder);
            var baseClassGenerator = new BaseClassVisitor { BaseClass = BaseClass };
            var globalsGenerator = new GlobalMembersVisitor(builder, globalSymbols, NullBehaviour);

            source.AppendLine("Option Infer On");

            // using <namespaces>;
            foreach (var ns in UseNamespaces ?? new string[0])
                usingGenerator.UsingNamespace(ns);

            foreach (var assembly in UseAssemblies ?? new string[0])
                usingGenerator.UsingAssembly(assembly);

            foreach (var resource in allResources)
                usingGenerator.Accept(resource);

            foreach (var resource in allResources)
                baseClassGenerator.Accept(resource);

            var viewClassName = "View" + GeneratedViewId.ToString("n");

            if (string.IsNullOrEmpty(TargetNamespace))
            {
                ViewClassFullName = viewClassName;
            }
            else
            {
                ViewClassFullName = TargetNamespace + "." + viewClassName;

                source.AppendLine();
                source.AppendLine(string.Format("Namespace {0}", TargetNamespace));
            }

            source.AppendLine();

            if (Descriptor != null)
            {
                // [SparkView] attribute
                source.AppendLine("<Global.Spark.SparkViewAttribute(");
                if (TargetNamespace != null)
                    source.AppendFormat("    TargetNamespace:=\"{0}\",", TargetNamespace).AppendLine();
                source.AppendLine("    Templates := New String() {");
                source.Append("      ").AppendLine(string.Join(",\r\n      ",
                                                               Descriptor.Templates.Select(
                                                                   t => "\"" + t.Replace("\\", "\\\\") + "\"").ToArray()));
                source.AppendLine("    })> _");
            }

            // public class ViewName : BasePageType 
            builder
                .Append("Public Class ")
                .AppendLine(viewClassName)
                .Append("    Inherits ")
                .AppendLine(baseClassGenerator.BaseClassTypeName);

            source.AppendLine();
            source.AppendLine(string.Format("    Private Shared ReadOnly _generatedViewId As Global.System.Guid = New Global.System.Guid(\"{0:n}\")", GeneratedViewId));


            source.AppendLine("    Public Overrides ReadOnly Property GeneratedViewId() As Global.System.Guid");
            source.AppendLine("      Get");
            source.AppendLine("        Return _generatedViewId");
            source.AppendLine("      End Get");
            source.AppendLine("    End Property");

            if (Descriptor != null && Descriptor.Accessors != null)
            {
                //TODO: correct this
                foreach (var accessor in Descriptor.Accessors)
                {
                    source.AppendLine();
                    source.Append("    public ").AppendLine(accessor.Property);
                    source.Append("    { get { return ").Append(accessor.GetValue).AppendLine("; } }");
                }
            }

            // properties and macros
            foreach (var resource in allResources)
                globalsGenerator.Accept(resource);

            // public void RenderViewLevelx()
            int renderLevel = 0;
            foreach (var viewTemplate in viewTemplates)
            {
                source.AppendLine();
                EditorBrowsableStateNever(source, 4); 
                source.AppendLine(string.Format("    Private Sub RenderViewLevel{0}()", renderLevel));
                var viewGenerator = new GeneratedCodeVisitor(builder, globalSymbols, NullBehaviour) { Indent = 8 };
                viewGenerator.Accept(viewTemplate);
                source.AppendLine("    End Sub");
                ++renderLevel;
            }
            
            // public void RenderView()
            source.AppendLine();
            EditorBrowsableStateNever(source, 4); 
            source.AppendLine("    Public Overrides Sub RenderView(ByVal writer As Global.System.IO.TextWriter)");
            for (var invokeLevel = 0; invokeLevel != renderLevel; ++invokeLevel)
            {
                if (invokeLevel != renderLevel - 1)
                {
                    source.AppendLine("        Using OutputScope()");
                    source.AppendLine(string.Format("            RenderViewLevel{0}()", invokeLevel));
                    source.AppendLine("          Content(\"view\") = Output");
                    source.AppendLine("        End Using");
                }
                else
                {
                    source.AppendLine("        Using OutputScope(writer)");
                    source.AppendLine(string.Format("            RenderViewLevel{0}()", invokeLevel));
                    source.AppendLine("        End Using");
                }
            }
            source.AppendLine("    End Sub");


            source.AppendLine("End Class");

            if (!string.IsNullOrEmpty(TargetNamespace))
            {
                source.AppendLine("End Namespace");
            }

            SourceCode = source.ToString();
            SourceMappings = builder.Mappings;
        }

        private static void EditorBrowsableStateNever(StringBuilder source, int indentation)
        {
            source
                .Append(new string(' ', indentation))
                .AppendLine("<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _");
        }
    }
}
