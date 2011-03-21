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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Spark.Compiler.Javascript.ChunkVisitors;

namespace Spark.Compiler.Javascript
{
    public class JavascriptViewCompiler : ViewCompiler
    {
        public override void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            GenerateSourceCode(viewTemplates, allResources);
        }

        public override void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            var source = new StringBuilder();

            var anonymousTypeVisitor = new JavascriptAnonymousTypeVisitor();
            var globalMembers = new JavascriptGlobalMembersVisitor(source);
            var preRenderVisitor = new JavascriptPreRenderVisitor(source);
            var generatedJavascript = new JavascriptGeneratedCodeVisitor(source);
            var postRenderVisitor = new JavascriptPostRenderVisitor(source);

            var primaryName = Descriptor.Templates[0];
            var nameParts = primaryName
                .Split(new[] { Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => SafeName(name));

            // convert some syntax from csharp to javascript
            foreach (var template in viewTemplates)
                anonymousTypeVisitor.Accept(template);

            var cumulativeName = "window.Spark";
            foreach (var part in nameParts)
            {
                source.Append("if (!").Append(cumulativeName).Append(") ").Append(cumulativeName).
                    AppendLine(" = {};");
                cumulativeName = cumulativeName + "." + part;
            }

            source.Append(cumulativeName).AppendLine(" = {");
            foreach (var chunks in allResources)
            {
                globalMembers.Accept(chunks);
            }
            source.AppendLine("RenderView: function(viewData) {");


            source.Append("var StringWriter = function() {");
            source.Append("this._parts = [];");
            source.Append("this.Write = function(arg) {this._parts.push(arg.toString());};");
            source.Append("this.toString = function() {return this._parts.join('');};");
            source.AppendLine("};");

            source.AppendLine("var Output = new StringWriter();");

            source.AppendLine("var Content = {};");

            source.Append("function OutputScope(arg) {");
            source.Append("if (typeof arg == 'string') {if (!Content[arg]) Content[arg] = new StringWriter(); arg = Content[arg];}");
            source.Append("OutputScope._frame = {_frame:OutputScope.Frame, _output:Output};");
            source.Append("Output = arg;");
            source.AppendLine("};");

            source.Append("function DisposeOutputScope() {");
            source.Append("Output = OutputScope._frame._output;");
            source.Append("OutputScope._frame = OutputScope._frame._frame;");
            source.AppendLine("};");

            foreach(var chunks in allResources)
            {
                preRenderVisitor.Accept(chunks);
            }

            var level = 0;
            foreach (var template in viewTemplates)
            {
                source.Append("function RenderViewLevel").Append(level).AppendLine("() {");
                generatedJavascript.Accept(template);
                source.AppendLine("}");
                ++level;
            }

            source.AppendLine("RenderViewLevel0();");

            foreach (var chunks in allResources)
            {
                postRenderVisitor.Accept(chunks);
            }

            source.AppendLine("return Output.toString();");
            source.AppendLine("} // function RenderView");

            source.Append("} // ").AppendLine(cumulativeName);

            SourceCode = source.ToString();
        }

        static string SafeName(string name)
        {
            var safeName = name;
            if (safeName.EndsWith(".spark"))
                safeName = safeName.Substring(0, safeName.Length - ".spark".Length);
            return safeName.Replace(".", "");
        }
    }
}