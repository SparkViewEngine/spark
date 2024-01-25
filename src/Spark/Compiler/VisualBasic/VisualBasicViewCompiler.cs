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
using System;
using System.Collections.Generic;
using System.IO;
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

            var writer = new StringWriter();
            var source = new SourceWriter(writer);

            // debug symbols not adjusted until the matching-directive issue resolved
            source.AdjustDebugSymbols = false;

            var usingGenerator = new UsingNamespaceVisitor(source);
            var baseClassGenerator = new BaseClassVisitor { BaseClass = BaseClass };
            var globalsGenerator = new GlobalMembersVisitor(source, globalSymbols, NullBehaviour);

            // needed for proper vb functionality
            source.WriteLine("Option Infer On");

            usingGenerator.UsingNamespace("Microsoft.VisualBasic");
            foreach (var ns in UseNamespaces ?? Array.Empty<string>())
                usingGenerator.UsingNamespace(ns);

            usingGenerator.UsingAssembly("Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            foreach (var assembly in UseAssemblies ?? Array.Empty<string>())
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

                source.WriteLine();
                source.Write("Namespace ").WriteLine(TargetNamespace);
            }

            source.WriteLine();

            if (Descriptor != null)
            {
                // [SparkView] attribute
                source.WriteLine("<Global.Spark.SparkViewAttribute( _");
                if (TargetNamespace != null)
                    source.WriteFormat("    TargetNamespace:=\"{0}\", _", TargetNamespace).WriteLine();
                source.WriteLine("    Templates := New String() { _");
                source.Write("      ").Write(string.Join(", _\r\n      ", 
                    Descriptor.Templates.Select(t => "\"" + SparkViewAttribute.ConvertToAttributeFormat(t) + "\"").ToArray()));
				
                source.WriteLine("    })> _");
            }

            // public class ViewName : BasePageType 
            source
                .Write("Public Class ")
                .WriteLine(viewClassName)
                .Write("    Inherits ")
                .WriteLine(baseClassGenerator.BaseClassTypeName)
                .AddIndent();

            source.WriteLine();
            source.WriteLine(string.Format("    Private Shared ReadOnly _generatedViewId As Global.System.Guid = New Global.System.Guid(\"{0:n}\")", GeneratedViewId));


            source
                .WriteLine("    Public Overrides ReadOnly Property GeneratedViewId() As Global.System.Guid")
                .WriteLine("      Get")
                .WriteLine("        Return _generatedViewId")
                .WriteLine("      End Get")
                .WriteLine("    End Property");

            if (Descriptor != null && Descriptor.Accessors != null)
            {
                //TODO: correct this
                foreach (var accessor in Descriptor.Accessors)
                {
                    source.WriteLine();
                    source.Write("    public ").WriteLine(accessor.Property);
                    source.Write("    { get { return ").Write(accessor.GetValue).WriteLine("; } }");
                }
            }

            // properties and macros
            foreach (var resource in allResources)
                globalsGenerator.Accept(resource);

            // public void RenderViewLevelx()
            int renderLevel = 0;
            foreach (var viewTemplate in viewTemplates)
            {
                source.WriteLine();
                EditorBrowsableStateNever(source, 4);
                source
                    .WriteLine("Private Sub RenderViewLevel{0}()", renderLevel)
                    .AddIndent();
                var viewGenerator = new GeneratedCodeVisitor(source, globalSymbols, NullBehaviour);
                viewGenerator.Accept(viewTemplate);
                source
                    .RemoveIndent()
                    .WriteLine("End Sub");
                ++renderLevel;
            }

            // public void RenderView()
            source.WriteLine();
            EditorBrowsableStateNever(source, 4);
            source
                .WriteLine("Public Overrides Sub Render()")
                .AddIndent();
            for (var invokeLevel = 0; invokeLevel != renderLevel; ++invokeLevel)
            {
                if (invokeLevel != renderLevel - 1)
                {
                    source
                        .WriteLine("Using OutputScope()")
                        .AddIndent()
                        .WriteLine("RenderViewLevel{0}()", invokeLevel)
                        .WriteLine("Content(\"view\") = Output")
                        .RemoveIndent()
                        .WriteLine("End Using");
                }
                else
                {
                    source
                        .WriteLine("RenderViewLevel{0}()", invokeLevel);
                }
            }
            source
                .RemoveIndent()
                .WriteLine("End Sub");


            source
                .RemoveIndent()
                .WriteLine("End Class");

            if (!string.IsNullOrEmpty(TargetNamespace))
            {
                source.WriteLine("End Namespace");
            }

            SourceCode = source.ToString();
            SourceMappings = source.Mappings;
        }

        private static void EditorBrowsableStateNever(SourceWriter source, int indentation)
        {
            source
                .Indent(indentation)
                .WriteLine("<System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)> _");
        }
    }
}
