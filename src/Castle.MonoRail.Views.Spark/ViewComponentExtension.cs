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
        private IDictionary<string, IList<Chunk>> sections;
        private IDictionary<string, IList<AttributeNode>> attributes;

        public ViewComponentExtension(ElementNode node)
        {
            this.node = node;
        }

        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            visitor.Accept(body);

            var sectionVisitor = new ViewComponentSectionChunkBuilderVisitor();
            sectionVisitor.Accept(body);
            sections = sectionVisitor.Sections;
            attributes = sectionVisitor.Attributes;
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
        {
            if (location == OutputLocation.RenderMethod)
            {                
                output.AppendFormat("RenderComponent(\"{0}\", new System.Collections.Generic.Dictionary<string,object> {{", node.Name);

                var delimiter = "";
                foreach (var attribute in node.Attributes)
                {
                    var code = attribute.Value.Replace("[[", "<").Replace("]]", ">");
                    output.AppendFormat("{2}{{\"{0}\",{1}}}", attribute.Name, code, delimiter);
                    delimiter = ", ";
                }
                output.AppendLine("}, new Action(delegate {");
                visitor.Accept(body);
                output.AppendLine("}),");

                output.AppendLine("new System.Collections.Generic.Dictionary<string,Action> {");
                foreach(var section in sections)
                {
                    output.Append("{\"")
                        .Append(section.Key)
                        .AppendLine("\", new Action(delegate {");
                    
                    foreach(var attr in attributes[section.Key])
                    {
                        output.Append("var ")
                            .Append(attr.Name)
                            .Append("=(")
                            .Append(attr.Value)
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

    public class ViewComponentSectionChunkBuilderVisitor : ChunkBuilderVisitor
    {
        private string sectionName;
        private int sectionDepth;

        private readonly IDictionary<string, IList<Chunk>> sections = new Dictionary<string, IList<Chunk>>();

        public IDictionary<string, IList<Chunk>> Sections
        {
            get { return sections; }
        }

        private readonly IDictionary<string, IList<AttributeNode>> attributes = new Dictionary<string, IList<AttributeNode>>();

        public IDictionary<string, IList<AttributeNode>> Attributes
        {
            get { return attributes; }
        }

        protected override void Visit(ElementNode elementNode)
        {
            if (sectionName == null)
            {
                if (!sections.ContainsKey(elementNode.Name))
                {
                    sections.Add(elementNode.Name, new List<Chunk>());
                    attributes.Add(elementNode.Name, new List<AttributeNode>());
                }

                foreach(var attr in elementNode.Attributes)
                    attributes[elementNode.Name].Add(attr);

                if (!elementNode.IsEmptyElement)
                {
                    sectionName = elementNode.Name;
                    sectionDepth = 1;
                    Chunks = sections[sectionName];
                }
            }
            else if (string.Equals(sectionName, elementNode.Name))
            {
                if (!elementNode.IsEmptyElement)
                    ++sectionDepth;
            }
            else
            {
                base.Visit(elementNode);
            }
        }

        protected override void Visit(EndElementNode endElementNode)
        {
            if (string.Equals(sectionName, endElementNode.Name))
            {
                --sectionDepth;
                if (sectionDepth == 0)
                {
                    sectionName = null;
                    Chunks = new List<Chunk>();
                }
            }
            else
            {
                base.Visit(endElementNode);
            }
        }
    }
}