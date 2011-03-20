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
using Spark.Compiler;
using Spark.Ruby.Compiler;
using Spark.Ruby.Compiler.ChunkVisitors;
using BaseClassVisitor = Spark.Compiler.CSharp.ChunkVisitors.BaseClassVisitor;

namespace Spark.Ruby.Compiler
{
    public class RubyViewCompiler:ViewCompiler
    {
        public string ScriptHeader { get; set; }


        public override void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            GenerateSourceCode(viewTemplates, allResources);

            var compiler = new BatchCompiler();
            var assembly = compiler.Compile(Debug, "csharp", SourceCode);
            CompiledType = assembly.GetType(ViewClassFullName);
        }

        public override void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            var script = new SourceWriter();
            var globals = new Dictionary<string, object>();

            script.WriteLine(ScriptHeader);

            script.WriteLine("class<<view");
            script.Indent++;

            var globalMembersVisitor = new GlobalMembersVisitor(script, globals);
            foreach (var resource in allResources)
                globalMembersVisitor.Accept(resource);

            var globalFunctionsVisitor = new GlobalFunctionsVisitor(script, globals);
            foreach (var resource in allResources)
                globalFunctionsVisitor.Accept(resource);


            var templateIndex = 0;
            foreach (var template in viewTemplates)
            {
                script.Write("def render_view_level").WriteLine(templateIndex);
                script.Indent++;

                var generator = new GeneratedCodeVisitor(script, globals);
                generator.Accept(template);
                
                script.Indent--;
                script.WriteLine("end");

                templateIndex++;
            }

            script.WriteLine("def render");
            script.Indent++;

            var globalInitializeVisitor = new GlobalInitializeVisitor(script);
            foreach(var resource in allResources)
                globalInitializeVisitor.Accept(resource);

            for (var renderIndex = 0; renderIndex != templateIndex; ++renderIndex)
            {
                if (renderIndex < templateIndex - 1)
                {
                    script.WriteLine("scope = output_scope");
                    script. Write("render_view_level").WriteLine(renderIndex);
                    script.WriteLine("content.set_Item \"view\", output");
                    script.WriteLine("scope.dispose");
                }
                else
                {
                    script.Write("render_view_level").WriteLine(renderIndex);
                }
            }
            script.Indent--;
            script.WriteLine("end");

            script.Indent--;
            script.WriteLine("end");
            script.WriteLine("view.view_data.each {|kv| view.instance_variable_set \"@\"+kv.key, kv.value}");
            script.WriteLine("view.render");


            var baseClassGenerator = new BaseClassVisitor { BaseClass = BaseClass };
            foreach (var resource in allResources)
                baseClassGenerator.Accept(resource);

            BaseClass = baseClassGenerator.BaseClassTypeName;

            var source = new StringBuilder();

            var viewClassName = "View" + GeneratedViewId.ToString("n");
            if (Descriptor != null && !string.IsNullOrEmpty(Descriptor.TargetNamespace))
            {
                ViewClassFullName = Descriptor.TargetNamespace + "." + viewClassName;
                source.Append("namespace ").AppendLine(Descriptor.TargetNamespace);
                source.AppendLine("{");
            }
            else
            {
                ViewClassFullName = viewClassName;
            }

            if (Descriptor != null)
            {
                // [SparkView] attribute
                source.AppendLine("[global::Spark.SparkViewAttribute(");
                if (TargetNamespace != null)
                    source.AppendFormat("    TargetNamespace=\"{0}\",", TargetNamespace).AppendLine();
                source.AppendLine("    Templates = new string[] {");
                source.Append("      ").AppendLine(string.Join(",\r\n      ",
                                                               Descriptor.Templates.Select(
                                                                   t => "\"" + SparkViewAttribute.ConvertToAttributeFormat(t) + "\"").ToArray()));
                source.AppendLine("    })]");
            }

            source.Append("public class ").Append(viewClassName).Append(" : ").Append(BaseClass).AppendLine(", global::Spark.Ruby.IScriptingSparkView");
            source.AppendLine("{");

            source.Append("static System.Guid _generatedViewId = new System.Guid(\"").Append(GeneratedViewId).AppendLine("\");");
            source.AppendLine("public override System.Guid GeneratedViewId");
            source.AppendLine("{");
            source.AppendLine("get { return _generatedViewId; }");
            source.AppendLine("}");

            source.AppendLine("public global::System.IDisposable OutputScopeAdapter(object arg) ");
            source.AppendLine("{");
            source.AppendLine("if (arg == null) return OutputScope();");
            source.AppendLine("if (arg is global::System.IO.TextWriter) return OutputScope((global::System.IO.TextWriter)arg);");
            source.AppendLine("return OutputScope(global::System.Convert.ToString(arg));");
            source.AppendLine("}");

            source.AppendLine("public void OutputWriteAdapter(object arg) ");
            source.AppendLine("{");
            source.AppendLine("Output.Write(arg);");
            source.AppendLine("}");

            source.AppendLine("public global::Microsoft.Scripting.Hosting.CompiledCode CompiledCode {get;set;}");

            source.AppendLine("public string ScriptSource");
            source.AppendLine("{");
            source.Append("get { return @\"").Append(script.ToString().Replace("\"", "\"\"")).AppendLine("\"; }");
            source.AppendLine("}");

            source.AppendLine("public override void Render()");
            source.AppendLine("{");
            source.AppendLine("CompiledCode.Execute(");
            source.AppendLine("CompiledCode.Engine.CreateScope(");
            source.AppendLine("new global::Spark.Ruby.ScriptingViewSymbolDictionary(this)");
            source.AppendLine("));");
            source.AppendLine("}");

            source.AppendLine("}");

            if (Descriptor != null && !string.IsNullOrEmpty(Descriptor.TargetNamespace))
            {
                source.AppendLine("}");
            }

            SourceCode = source.ToString();
        }
    }
}