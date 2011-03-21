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
using Spark.Compiler.CSharp.ChunkVisitors;

namespace Spark.Compiler.CSharp
{
    public class CSharpViewCompiler : ViewCompiler
    {
        public override void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            GenerateSourceCode(viewTemplates, allResources);

            var batchCompiler = new BatchCompiler();
            var assembly = batchCompiler.Compile(Debug, "csharp", SourceCode);
            CompiledType = assembly.GetType(ViewClassFullName);
        }


        public override void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            var globalSymbols = new Dictionary<string, object>();

            var writer = new StringWriter();
            var source = new SourceWriter(writer);

            var usingGenerator = new UsingNamespaceVisitor(source);
            var baseClassGenerator = new BaseClassVisitor { BaseClass = BaseClass };
            var globalsGenerator = new GlobalMembersVisitor(source, globalSymbols, NullBehaviour);
            


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

                source
                    .WriteLine()
                    .WriteLine(string.Format("namespace {0}", TargetNamespace))
                    .WriteLine("{").AddIndent();
            }

            source.WriteLine();

			if (Descriptor != null)
            {
                // [SparkView] attribute
                source.WriteLine("[global::Spark.SparkViewAttribute(");
                if (TargetNamespace != null)
                    source.WriteFormat("    TargetNamespace=\"{0}\",", TargetNamespace).WriteLine();
                source.WriteLine("    Templates = new string[] {");
                source.Write("      ").WriteLine(string.Join(",\r\n      ",
                                                               Descriptor.Templates.Select(
                                                                   t => "\"" + SparkViewAttribute.ConvertToAttributeFormat(t) + "\"").ToArray()));
                source.WriteLine("    })]");
            }

            // public class ViewName : BasePageType 
            source
                .Write("public class ")
                .Write(viewClassName)
                .Write(" : ")
                .WriteCode(baseClassGenerator.BaseClassTypeName)
                .WriteLine();
            source.WriteLine("{").AddIndent();

            source.WriteLine();
            EditorBrowsableStateNever(source, 4);
            source.WriteLine("private static System.Guid _generatedViewId = new System.Guid(\"{0:n}\");", GeneratedViewId);
            source.WriteLine("public override System.Guid GeneratedViewId");
            source.WriteLine("{ get { return _generatedViewId; } }");

            if (Descriptor != null && Descriptor.Accessors != null)
            {
                foreach (var accessor in Descriptor.Accessors)
                {
                    source.WriteLine();
                    source.Write("public ").WriteLine(accessor.Property);
                    source.Write("{ get { return ").Write(accessor.GetValue).WriteLine("; } }");
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
                source.WriteLine(string.Format("private void RenderViewLevel{0}()", renderLevel));
                source.WriteLine("{").AddIndent();
                var viewGenerator = new GeneratedCodeVisitor(source, globalSymbols, NullBehaviour);
                viewGenerator.Accept(viewTemplate);
                source.RemoveIndent().WriteLine("}");
                ++renderLevel;
            }

            // public void RenderView()

            source.WriteLine();
            EditorBrowsableStateNever(source, 4); 
            source.WriteLine("public override void Render()");
            source.WriteLine("{").AddIndent();
            for (var invokeLevel = 0; invokeLevel != renderLevel; ++invokeLevel)

            {
                if (invokeLevel != renderLevel - 1)
                {
                    source.WriteLine("using (OutputScope()) {{RenderViewLevel{0}(); Content[\"view\"] = Output;}}", invokeLevel);
                }
                else
                {

                    source.WriteLine("        RenderViewLevel{0}();", invokeLevel);
                }
            }
            source.RemoveIndent().WriteLine("}");

            // end class
            source.RemoveIndent().WriteLine("}");

            if (!string.IsNullOrEmpty(TargetNamespace))
            {
                source.RemoveIndent().WriteLine("}");
            }

            SourceCode = source.ToString();
            SourceMappings = source.Mappings;
        }

        private static void EditorBrowsableStateNever(SourceWriter source, int indentation)
        {
            source
                .Indent(indentation)
                .WriteLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        }
    }
}
