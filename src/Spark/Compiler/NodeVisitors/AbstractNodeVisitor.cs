// Copyright 2008-2024 Louis DeJardin
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
using Spark.Parser.Markup;

namespace Spark.Compiler.NodeVisitors
{
    public abstract class AbstractNodeVisitor : INodeVisitor
    {
        protected AbstractNodeVisitor(VisitorContext context)
        {
            Context = context;
        }
        public VisitorContext Context { get; set; }

        public abstract IList<Node> Nodes { get; }

        public void Accept(IList<Node> nodes)
        {
            BeforeAcceptNodes();
            foreach (var node in nodes)
                Accept(node);
            AfterAcceptNodes();
        }

        protected virtual void BeforeAcceptNodes()
        {
        }

        protected virtual void AfterAcceptNodes()
        {
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
                Visit((StatementNode)node);
            else if (node is ConditionNode)
                Visit((ConditionNode)node);
            else if (node is XMLDeclNode)
                Visit((XMLDeclNode) node);
            else if (node is ProcessingInstructionNode)
                Visit((ProcessingInstructionNode)node);
            else if (node is IndentationNode)
                Visit((IndentationNode)node);
            else
                throw new ArgumentException(string.Format("Unknown node type {0}", node.GetType()), "node");
        }



        protected abstract void Visit(StatementNode node);
        protected abstract void Visit(ExpressionNode node);
        protected abstract void Visit(EntityNode entityNode);
        protected abstract void Visit(DoctypeNode docTypeNode);
        protected abstract void Visit(TextNode textNode);
        protected abstract void Visit(ElementNode node);
        protected abstract void Visit(EndElementNode node);
        protected abstract void Visit(AttributeNode attributeNode);
        protected abstract void Visit(CommentNode commentNode);
        protected abstract void Visit(SpecialNode specialNode);
        protected abstract void Visit(ExtensionNode node);
        protected abstract void Visit(ConditionNode node);
        protected abstract void Visit(XMLDeclNode node);
        protected abstract void Visit(ProcessingInstructionNode node);
        protected abstract void Visit(IndentationNode node);
    }
}