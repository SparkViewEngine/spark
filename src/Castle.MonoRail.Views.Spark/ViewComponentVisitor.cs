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
using System.Collections.Generic;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;

namespace Castle.MonoRail.Views.Spark
{
    public class ViewComponentVisitor
    {
        private readonly ChunkBuilderVisitor chunkBuilderVisitor;
        private readonly ViewComponentInfo info;
        private string sectionName;
        private int sectionDepth;
        private readonly IList<Chunk> bodyChunks;

        public ViewComponentVisitor(ChunkBuilderVisitor chunkBuilderVisitor, ViewComponentInfo info)
        {
            this.chunkBuilderVisitor = chunkBuilderVisitor;
            this.info = info;
            bodyChunks = chunkBuilderVisitor.Chunks;
        }


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


        public void Accept(IList<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is ElementNode)
                {
                    Visit((ElementNode)node);
                }
                else if (node is EndElementNode)
                {
                    Visit((EndElementNode)node);
                }
                else
                {
                    chunkBuilderVisitor.Accept(node);
                }
            }
        }

        void Visit(ElementNode elementNode)
        {
            if (sectionName == null && info.SupportsSection(elementNode.Name))
            {
                if (!sections.ContainsKey(elementNode.Name))
                {
                    sections.Add(elementNode.Name, new List<Chunk>());
                    attributes.Add(elementNode.Name, new List<AttributeNode>());
                }

                foreach (var attr in elementNode.Attributes)
                    attributes[elementNode.Name].Add(attr);

                if (!elementNode.IsEmptyElement)
                {
                    sectionName = elementNode.Name;
                    sectionDepth = 1;
                    chunkBuilderVisitor.Chunks = sections[sectionName];
                }
                return;
            }

            if (string.Equals(sectionName, elementNode.Name))
            {
                if (!elementNode.IsEmptyElement)
                    ++sectionDepth;
            }
            chunkBuilderVisitor.Accept(elementNode);
        }

        void Visit(EndElementNode endElementNode)
        {
            if (string.Equals(sectionName, endElementNode.Name))
            {
                --sectionDepth;
                if (sectionDepth == 0)
                {
                    sectionName = null;
                    chunkBuilderVisitor.Chunks = bodyChunks;
                    return;
                }
            }

            chunkBuilderVisitor.Accept(endElementNode);
        }
    }
}