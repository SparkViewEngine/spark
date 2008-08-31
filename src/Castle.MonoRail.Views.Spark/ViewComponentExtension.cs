// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

using System;
using Castle.MonoRail.Framework;
using Spark.Parser.Code;

namespace Castle.MonoRail.Views.Spark
{
    using System.Collections.Generic;
    using System.Text;

    using global::Spark;
    using global::Spark.Compiler;
    using global::Spark.Compiler.ChunkVisitors;
    using global::Spark.Compiler.NodeVisitors;
    using global::Spark.Parser.Markup;

    internal class ViewComponentExtension : ISparkExtension
    {
        private readonly ElementNode node;
        private readonly ViewComponentInfo info;

        private IDictionary<string, IList<Chunk>> sectionsChunks;
        private IDictionary<string, IList<AttributeNode>> sectionsAttributes;

        public ViewComponentExtension(ElementNode node, ViewComponentInfo info)
        {
            this.node = node;
            this.info = info;
        }

        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            if (visitor is ChunkBuilderVisitor)
            {
                var sectionVisitor = new ViewComponentVisitor((ChunkBuilderVisitor)visitor, info);
                sectionVisitor.Accept(body);
                sectionsChunks = sectionVisitor.Sections;
                sectionsAttributes = sectionVisitor.Attributes;
            }
            else
            {
                visitor.Accept(body);
            }
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
        {
            if (location == OutputLocation.RenderMethod)
            {
                output.AppendFormat("RenderComponent(\"{0}\", new System.Collections.Generic.Dictionary<string,object> {{", node.Name);

                var delimiter = "";
                foreach (var attribute in node.Attributes)
                {
                    var code = attribute.AsCode();
                    output.AppendFormat("{2}{{\"{0}\",{1}}}", attribute.Name, code, delimiter);
                    delimiter = ", ";
                }
                output.AppendLine("}, new System.Action(delegate {");
                visitor.Accept(body); //only append body if there are no sections
                output.AppendLine("}),");

                output.AppendLine("new System.Collections.Generic.Dictionary<string,System.Action> {");
                foreach (var section in sectionsChunks)
                {
                    output.Append("{\"")
                        .Append(section.Key)
                        .AppendLine("\", new System.Action(delegate {");

                    foreach (var attr in sectionsAttributes[section.Key])
                    {
                        output.Append("var ")
                            .Append(attr.Name)
                            .Append("=(")
                            .Append(attr.AsCode())
                            .Append(")ViewData[\"")
                            .Append(attr.Name)
                            .AppendLine("\"];");
                    }
                    visitor.Accept(section.Value);
                    output.AppendLine("})},");
                }
                output.AppendLine("});");
            }
        }
    }
}
