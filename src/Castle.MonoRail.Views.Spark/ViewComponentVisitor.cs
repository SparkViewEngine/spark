using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.Framework;
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

    public class ViewComponentInfo
    {
        public ViewComponentInfo()
        {
            
        }
        public ViewComponentInfo(Type type)
        {
            Type = type;
            Details = type.GetCustomAttributes(typeof(ViewComponentDetailsAttribute), false).OfType<ViewComponentDetailsAttribute>().FirstOrDefault();
            if (Details == null)
                Instance = (ViewComponent)Activator.CreateInstance(type);
        }
        public Type Type { get; set; }
        public ViewComponentDetailsAttribute Details { get; set; }
        public ViewComponent Instance { get; set; }

        public bool SupportsSection(string sectionName)
        {
            if (Details != null)
                return Details.SupportsSection(sectionName);
            if (Instance != null)
                return Instance.SupportsSection(sectionName);
            return false;
        }
    }
}