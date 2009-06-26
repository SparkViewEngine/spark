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
using System.Linq;
using System.Text;
using Spark.Compiler.CSharp.ChunkVisitors;

namespace Spark.Compiler.VisualBasic
{
    public class VisualBasicViewCompiler : ViewCompiler
    {
        public override void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            GenerateSourceCode(viewTemplates, allResources);

            var batchCompiler = new BatchCompiler();
            var assembly = batchCompiler.Compile(Debug, SourceCode);
            CompiledType = assembly.GetType(ViewClassFullName);
        }


        public override void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            var globalSymbols = new Dictionary<string, object>();

            var source = new StringBuilder();
            var builder = new SourceBuilder(source);
            var usingGenerator = new UsingNamespaceVisitor(builder);
            var baseClassGenerator = new BaseClassVisitor { BaseClass = BaseClass };
            var globalsGenerator = new GlobalMembersVisitor(builder, globalSymbols, NullBehaviour);
            


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
                source.AppendLine(string.Format("namespace {0}", TargetNamespace));
                source.AppendLine("{");
            }

            source.AppendLine();

            if (Descriptor != null)
            {
                // [SparkView] attribute
                source.AppendLine("[global::Spark.SparkViewAttribute(");
                if (TargetNamespace != null)
                    source.AppendFormat("    TargetNamespace=\"{0}\",", TargetNamespace).AppendLine();
                source.AppendLine("    Templates = new string[] {");
                source.Append("      ").AppendLine(string.Join(",\r\n      ",
                                                               Descriptor.Templates.Select(
                                                                   t => "\"" + t.Replace("\\", "\\\\") + "\"").ToArray()));
                source.AppendLine("    })]");
            }

            // public class ViewName : BasePageType 
            builder
                .Append("public class ")
                .Append(viewClassName)
                .Append(" : ")
                .Append(baseClassGenerator.BaseClassTypeName)
                .AppendLine();
            source.AppendLine("{");

            source.AppendLine();
            source.AppendLine("    public override System.Guid GeneratedViewId");
            source.AppendLine(string.Format("    {{ get {{ return new System.Guid(\"{0:n}\"); }} }}", GeneratedViewId));

            if (Descriptor != null && Descriptor.Accessors != null)
            {
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
                source.AppendLine(string.Format("    public void RenderViewLevel{0}()", renderLevel));
                source.AppendLine("    {");
                var viewGenerator = new GeneratedCodeVisitor(builder, globalSymbols, NullBehaviour) { Indent = 8 };
                viewGenerator.Accept(viewTemplate);
                source.AppendLine("    }");
                ++renderLevel;
            }

            // public void RenderView()
            source.AppendLine();
            source.AppendLine("    public override void RenderView(System.IO.TextWriter writer)");
            source.AppendLine("    {");
            for (int invokeLevel = 0; invokeLevel != renderLevel; ++invokeLevel)
            {
                if (invokeLevel != renderLevel - 1)
                {
                    source.AppendLine(string.Format("        using (OutputScope()) {{RenderViewLevel{0}(); Content[\"view\"] = Output;}}", invokeLevel));
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
            SourceMappings = builder.Mappings;
        }

    }
}
