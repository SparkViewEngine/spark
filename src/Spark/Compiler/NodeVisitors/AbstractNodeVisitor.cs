/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public abstract class AbstractNodeVisitor : INodeVisitor
    {
        public abstract IList<Node> Nodes { get; }

        public void Accept(IList<Node> nodes)
        {
            foreach (var node in nodes)
                Accept(node);
        }

        public void Accept(Node node)
        {
            if (node is TextNode)
                Visit((TextNode)node);
            else if (node is EntityNode)
                Visit((EntityNode)node);
            else if (node is ExpressionNode)
                Visit((ExpressionNode)node);
            else if (node is ElementNode)
                Visit((ElementNode)node);
            else if (node is AttributeNode)
                Visit((AttributeNode)node);
            else if (node is EndElementNode)
                Visit((EndElementNode)node);
            else if (node is DoctypeNode)
                Visit((DoctypeNode)node);
            else if (node is CommentNode)
                Visit((CommentNode)node);
            else if (node is SpecialNode)
                Visit((SpecialNode)node);
            else if (node is ExtensionNode)
                Visit((ExtensionNode)node);
            else if (node is StatementNode)
                Visit((StatementNode) node);
            else
                throw new ArgumentException(string.Format("Unknown node type {0}", node.GetType()), "node");
        }

        protected abstract void Visit(StatementNode node);
        protected abstract void Visit(ExpressionNode expressionNode);
        protected abstract void Visit(EntityNode entityNode);
        protected abstract void Visit(DoctypeNode docTypeNode);
        protected abstract void Visit(TextNode textNode);
        protected abstract void Visit(ElementNode elementNode);
        protected abstract void Visit(EndElementNode endElementNode);
        protected abstract void Visit(AttributeNode attributeNode);
        protected abstract void Visit(CommentNode commentNode);
        protected abstract void Visit(SpecialNode specialNode);
        protected abstract void Visit(ExtensionNode node);
    }
}